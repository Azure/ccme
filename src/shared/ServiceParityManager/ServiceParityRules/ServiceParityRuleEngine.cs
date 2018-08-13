// -----------------------------------------------------------------------
// <copyright file="ServiceParityRuleEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.ServiceParityRules
{
    internal sealed class ServiceParityRuleEngine : IServiceParityRuleEngine
    {
        private readonly Dictionary<RuleSeverityModel, ServiceParitySeverityModel> severityMapping = new Dictionary<RuleSeverityModel, ServiceParitySeverityModel>
        {
            { RuleSeverityModel.Information, ServiceParitySeverityModel.Information },
            { RuleSeverityModel.Warning, ServiceParitySeverityModel.Warning },
            { RuleSeverityModel.Error, ServiceParitySeverityModel.Error },
            { RuleSeverityModel.Critical, ServiceParitySeverityModel.Critical }
        };

        public ServiceParityResult Process(IEnumerable<ResourceModel> resources, IEnumerable<RuleModel> rules, string targetRegion)
        {
            var ruleEngine = new RuleEngine(rules);

            var details = new Dictionary<string, ServiceParityResourceResult>();
            foreach (var resource in resources.Where(r => r.Details != null))
            {
                try
                {
                    var originalLocation = resource.Details["location"];
                    resource.Details["location"] = targetRegion;
                    var outputs = ruleEngine.Analyze(resource.Details);
                    resource.Details["location"] = originalLocation;

                    details.Add(
                        resource.Id,
                        new ServiceParityResourceResult
                        {
                            Details = outputs.Select(o => this.Convert(o))
                        });
                }
                catch (Exception ex)
                {
                    details.Add(
                        resource.Id,
                        new ServiceParityResourceResult
                        {
                            Exception = ex
                        });
                }
            }

            return new ServiceParityResult { Details = details };
        }

        private ServiceParityResourceDetail Convert(RuleEngineOutputModel model)
        {
            var detail = new ServiceParityResourceDetail
            {
                Pass = model.Pass,
                RuleName = model.RuleName,
                RuleSetID = model.RuleSetID,
                Category = model.Category,
                Source = model.Source,
                Brief = model.Brief,
                Message = model.Message,
                Path = model.Path
            };

            if (this.severityMapping.TryGetValue(model.Severity, out var severity))
            {
                detail.Severity = severity;
            }
            else
            {
                detail.Severity = ServiceParitySeverityModel.Unknown;
            }

            return detail;
        }
    }
}