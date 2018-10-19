// -----------------------------------------------------------------------
// <copyright file="ServiceParityManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Helpers;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models;
using Microsoft.Azure.CCME.Assessment.Managers.ServiceParityRules;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public sealed class ServiceParityManager : IServiceParityManager
    {
        private const string TelemetryLogSection = "ServiceParity";

        private readonly IAssessmentContext context;

        private IEnumerable<RuleModel> rules;

        public ServiceParityManager(IAssessmentContext context)
        {
            this.context = context;
        }

        public IEnumerable<string> LoadRules()
        {
            var ruleRepository = new ServiceParityRuleRepository(this.context.ConfigManager);

            this.context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Information,
                TelemetryLogSection,
                "Retrieving rules from configuration store...");

            this.rules = ruleRepository.GetRules();

            this.context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Information,
                TelemetryLogSection,
                FormattableString.Invariant($"{this.rules.Count()} rules retrieved from configuration store"));

            return this.rules
                .Select(r => r.Pattern.Value<string>("type"))
                .Where(s => !PlaceholderHelper.IsPlaceholder(s, out var unused))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public ServiceParityResult Process(IEnumerable<SubscriptionModel> subscriptions, string targetRegion)
        {
            var ruleEngine = new ServiceParityRuleEngine();

            var resources = subscriptions.SelectMany(m => m.ResourceGroups.SelectMany(r => r.Value));
            var result = ruleEngine.Process(resources, this.rules, targetRegion);

            this.context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Information,
                TelemetryLogSection,
                FormattableString.Invariant($"Overall status: Pass = {result.Pass}"));

            foreach (var pair in result.Details.Where(pair => !pair.Value.Pass))
            {
                if (pair.Value.Exception != null)
                {
                    this.context.TelemetryManager.WriteLog(
                        TelemetryLogLevel.Warning,
                        TelemetryLogSection,
                        FormattableString.Invariant($"Exception raised for resource {pair.Key}"),
                        null,
                        pair.Value.Exception);
                    continue;
                }

                this.context.TelemetryManager.WriteLog(
                    TelemetryLogLevel.Information,
                    TelemetryLogSection,
                    FormattableString.Invariant($"Resource {pair.Key}, Pass = {pair.Value.Pass}"),
                    pair.Value.Details.Where(d => !d.Pass).Select(detail => $"[{detail.RuleName}] {detail.Message}, path = {detail.Path}"));
            }

            return result;
        }
    }
}