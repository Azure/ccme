// -----------------------------------------------------------------------
// <copyright file="ResourceManagerForUsageReport.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public sealed class ResourceManagerForUsageReport : IResourceManager
    {
        private const string TelemetryLogSection = "ResourceManager";

        private readonly IAssessmentContext context;

        public ResourceManagerForUsageReport(IAssessmentContext context)
        {
            this.context = context;
        }

        public Task<string> GetSubscriptionNameAsync(string subscriptionId)
        {
            throw new NotSupportedException();
        }

        public Task<IDictionary<string, string>> ListSubscriptionsAsync()
        {
            throw new NotSupportedException();
        }

        public async Task<IEnumerable<SubscriptionModel>> GetResourcesAsync(IEnumerable<string> detailedResourceTypes = null)
        {
            var resourceGroups = this.context.UsageReport.Meters
                .GroupBy(m => m.ResourceId, StringComparer.InvariantCultureIgnoreCase)
                .Select(g => g.First())
                .GroupBy(m => m.ResourceGroup, StringComparer.InvariantCultureIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => new ResourceModel
                    {
                        Id = r.ResourceId,
                        Details = JObject.FromObject(new
                        {
                            type = r.ResourceType,
                            location = r.Location
                        })
                    }));

            foreach (var pair in resourceGroups)
            {
                this.context.TelemetryManager.WriteLog(
                    TelemetryLogLevel.Information,
                    TelemetryLogSection,
                    $"{pair.Value.Count()} resources retrieved from resource group `{pair.Key}`");
            }

            return await Task.FromResult(new[]
            {
                new SubscriptionModel
                {
                    SubscriptionId = this.context.UsageReport.SubscriptionId,
                    SubscriptionName = this.context.UsageReport.SubscriptionName,
                    ResourceGroups = resourceGroups
                }
            });
        }
    }
}