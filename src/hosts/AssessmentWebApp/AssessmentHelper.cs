// -----------------------------------------------------------------------
// <copyright file="AssessmentHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    internal static class AssessmentHelper
    {
        public static IAssessmentContext GetEnvironmentContext(
            ITelemetryManager telemetryManager,
            string accessToken,
            string armBaseUri,
            string subscriptionId = null)
        {
            return new AssessmentContext(
                usageReport: null,
                subscriptionNames: null,
                subscriptionIds: subscriptionId == null ? null : new[] { subscriptionId },
                resourceGroupNames: null,
                accessToken: accessToken,
                armBaseUri: armBaseUri,
                resourceCachePath: null,
                configManager: GetConfigManager(),
                telemetryManager: telemetryManager);
        }

        public static IAssessmentContext GetEnvironmentContext(
            ITelemetryManager telemetryManager,
            ParsedUsageReport usageReport)
        {
            return new AssessmentContext(
                usageReport: usageReport,
                subscriptionNames: null,
                subscriptionIds: null,
                resourceGroupNames: null,
                accessToken: null,
                armBaseUri: null,
                resourceCachePath: null,
                configManager: GetConfigManager(),
                telemetryManager: telemetryManager);
        }

        private static IConfigManager GetConfigManager()
        {
            return ConfigManagerFactory.CreateStorageAccountConfigManager(
                ConfigHelper.ConfigStorageAccountConnectionString);
        }
    }
}