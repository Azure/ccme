// -----------------------------------------------------------------------
// <copyright file="ListRuleEvaluator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Helpers;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Models;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Exceptions;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Evaluators
{
    /// <summary>
    /// The built-in evaluator based on blacklist and whitelist
    /// </summary>
    internal sealed class ListRuleEvaluator : IRuleEvaluator
    {
        private readonly ListRuleEvaluatorConfigurationsModel list;
        private readonly bool ignoreCase;

        public ListRuleEvaluator(RuleModel rule, string configurations)
        {
            this.list = JsonConvert.DeserializeObject<ListRuleEvaluatorConfigurationsModel>(configurations);
            this.ignoreCase = rule.IgnoreCase;

            // Expand array to individual values
            this.list.Whitelist = this.list.Whitelist
                ?.Select(set => set.Expand())
                .SelectMany(g => g)
                .ToList();

            this.list.Blacklist = this.list.Blacklist
                ?.Select(model => model.Expand())
                .SelectMany(g => g)
                .ToList();

            // Search for the first blacklist/whitelist item has unexpected or missing placeholders
            var placeholders = new HashSet<string>(PlaceholderHelper.GetPlaceholders(rule.Pattern));
            var brokenItem =
                this.list.Whitelist?.FirstOrDefault(item => !placeholders.SetEquals(item.Keys)) ??
                this.list.Blacklist?.FirstOrDefault(item => !placeholders.SetEquals(item.Values.Keys))?.Values;

            if (brokenItem == null)
            {
                return;
            }

            var unexpectedPlaceholders = brokenItem.Keys.Except(placeholders);
            if (unexpectedPlaceholders.Any())
            {
                throw new InvalidConfigurationUnexpectedPlaceholdersException(rule, unexpectedPlaceholders, brokenItem);
            }

            var missingPlaceholders = placeholders.Except(brokenItem.Keys);
            if (missingPlaceholders.Any())
            {
                throw new InvalidConfigurationUncheckedPlaceholdersException(rule, missingPlaceholders, brokenItem);
            }
        }

        public RuleEngineOutputModel Evaluate(Match match, IReadOnlyDictionary<string, string> additionalReplacements)
        {
            // Search for the given values in the blacklist
            var brokenItem = this.list.Blacklist?.FirstOrDefault(item => match.Equals(item.Values, this.ignoreCase));
            if (brokenItem != null)
            {
                return new RuleEngineOutputModel
                {
                    Pass = false,
                    Message =
                        PopulateMessage(brokenItem.HitResource, brokenItem.HitMessage, match, additionalReplacements) ??
                        PopulateMessage(this.list.BlacklistDefaultHitResource, this.list.BlacklistDefaultHitMessage, match, additionalReplacements),
                    Path = match.Groups.First().Value.Path
                };
            }

            // Search for the given values in the whitelist
            if (this.list.Whitelist != null && !this.list.Whitelist.Any(item => match.Equals(item, this.ignoreCase)))
            {
                return new RuleEngineOutputModel
                {
                    Pass = false,
                    Message = PopulateMessage(this.list.WhitelistNoHitResource, this.list.WhitelistNoHitMessage, match, additionalReplacements),
                    Path = match.Groups.First().Value.Path
                };
            }

            return new RuleEngineOutputModel { Pass = true };
        }

        private static string PopulateMessage(string resource, string message, Match match, IReadOnlyDictionary<string, string> additionalReplacements)
        {
            // ToDo: load localized message from resource
            if (string.IsNullOrEmpty(message))
            {
                return null;
            }

            var replacements = match.Groups.Values.ToDictionary(g => g.Key, g => g.Value.ToString());
            foreach (var pair in additionalReplacements)
            {
                replacements.Add(pair.Key, pair.Value);
            }

            return PlaceholderHelper.ReplacePlaceholders(
                message,
                replacements);
        }
    }
}