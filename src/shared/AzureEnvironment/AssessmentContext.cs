// -----------------------------------------------------------------------
// <copyright file="AssessmentContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Environments
{
    public sealed class AssessmentContext : IAssessmentContext
    {
        public ParsedUsageReport UsageReport { get; }
        public IEnumerable<string> SubscriptionNames { get; }
        public IEnumerable<string> SubscriptionIds { get; }
        public IEnumerable<string> ResourceGroupNames { get; }
        public string ARMAccessToken { get; }
        public string ARMBaseUri { get; }
        public string ResourceCachePath { get; }
        public IConfigManager ConfigManager { get; }
        public ITelemetryManager TelemetryManager { get; }

        public AssessmentContext(
            ParsedUsageReport usageReport,
            IEnumerable<string> subscriptionNames,
            IEnumerable<string> subscriptionIds,
            IEnumerable<string> resourceGroupNames,
            string accessToken,
            string armBaseUri,
            string resourceCachePath,
            IConfigManager configManager,
            ITelemetryManager telemetryManager)
        {
            this.UsageReport = usageReport;
            this.SubscriptionNames = subscriptionNames;
            this.SubscriptionIds = subscriptionIds;
            this.ResourceGroupNames = resourceGroupNames;
            this.ARMAccessToken = accessToken;
            this.ARMBaseUri = armBaseUri;
            this.ResourceCachePath = resourceCachePath;
            this.ConfigManager = configManager;
            this.TelemetryManager = telemetryManager;
        }
    }
}