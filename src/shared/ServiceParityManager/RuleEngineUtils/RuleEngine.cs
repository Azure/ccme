// -----------------------------------------------------------------------
// <copyright file="RuleEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Models;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils
{
    internal sealed class RuleEngine : IRuleEngine
    {
        private readonly IEnumerable<RuleModel> rules;

        public RuleEngine(IEnumerable<RuleModel> rules)
        {
            this.rules = rules;
        }

        public IEnumerable<RuleEngineOutputModel> Analyze(JToken input)
        {
            return this.rules
                .Select(rule => Analyze(input, rule))
                .SelectMany(m => m)
                .ToList();
        }

        private static IEnumerable<RuleEngineOutputModel> Analyze(JToken input, RuleModel rule)
        {
            var jsonMatch = new JsonMatchEngine(rule.Pattern, rule.IgnoreCase);
            var collection = jsonMatch.Matches(input) ?? new Match[] { };

            return collection.Select(match =>
            {
                var output = rule.Evaluator.Evaluate(match);
                output.RuleName = rule.Name;
                output.RuleSetID = rule.RuleSetID;
                output.Severity = rule.Severity;
                output.Category = rule.Category;
                output.Source = rule.Source;
                output.Brief = rule.Brief ?? rule.Name;
                return output;
            });
        }
    }
}