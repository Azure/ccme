// -----------------------------------------------------------------------
// <copyright file="ServiceParityRuleRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Evaluators;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models;
using Microsoft.Azure.CCME.Assessment.Managers.ServiceParityRules.Exceptions;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.ServiceParityRules
{
    internal sealed class ServiceParityRuleRepository : IServiceParityRuleRepository
    {
        private const string DefaultRuleSetIDListKey = "serviceParity.defaultRuleSetIDList";
        private const string RuleSetsKeyPrefix = "ruleSets/";
        private readonly IConfigManager configManager;
        private readonly string ruleSetIDListKey;

        public ServiceParityRuleRepository(IConfigManager configManager, string ruleSetIDListKey = null)
        {
            this.configManager = configManager;
            this.ruleSetIDListKey = ruleSetIDListKey ?? DefaultRuleSetIDListKey;
        }

        public IEnumerable<RuleModel> GetRules()
        {
            var content = this.configManager.GetValue(this.ruleSetIDListKey);
            var ruleSetIDs = JsonConvert.DeserializeObject<IEnumerable<string>>(content);
            var ruleSets = ruleSetIDs.Select(id => this.LoadRuleSet(id));
            return ruleSets.SelectMany(ruleSet => ruleSet).ToList();
        }

        private IEnumerable<RuleModel> LoadRuleSet(string ruleSetID)
        {
            var content = this.configManager.GetValue($"{RuleSetsKeyPrefix}{ruleSetID}/Rules.json", ConfigType.ServicParityRule);
            var ruleSet = JsonConvert.DeserializeObject<RuleSetModel>(content);
            foreach (var rule in ruleSet.Rules)
            {
                rule.RuleSetID = ruleSetID;
            }

            var decidedRules = ruleSet.Rules
                .Where(r => string.IsNullOrWhiteSpace(r.Base) && (r.IsAbstract || r.IsQualified))
                .ToDictionary(r => r.Name);

            var undecidedRules = ruleSet.Rules
                .Except(decidedRules.Values)
                .ToList();

            while (undecidedRules.Any())
            {
                var rule = undecidedRules.FirstOrDefault(r => decidedRules.ContainsKey(r.Base));
                if (rule == null)
                {
                    throw new InvalidRuleSetDependencyResolveFailedException(ruleSetID, undecidedRules.Select(r => r.Name));
                }

                rule.Populate(decidedRules[rule.Base]);
                if (!rule.IsAbstract && !rule.IsQualified)
                {
                    throw new InvalidRuleSetUnqualifiedRuleExcepton(ruleSetID, rule.Name);
                }

                undecidedRules.Remove(rule);
                decidedRules.Add(rule.Name, rule);
            }

            var rules = decidedRules.Values.Where(r => !r.IsAbstract);
            foreach (var rule in rules)
            {
                switch (rule.EvaluatorModel.Type)
                {
                    case "BuiltIn.List":
                        var key = $"{RuleSetsKeyPrefix}{ruleSetID}/{rule.EvaluatorModel.ConfigKey}";
                        var configurations = this.configManager.GetValue(key, ConfigType.ServicParityRule);
                        rule.Evaluator = new ListRuleEvaluator(rule, configurations);
                        break;

                    default:
                        throw new InvalidRuleSetUnknownEvaluatorTypeException(ruleSetID, rule.Name, rule.EvaluatorModel.Type);
                }
            }

            return rules;
        }
    }
}