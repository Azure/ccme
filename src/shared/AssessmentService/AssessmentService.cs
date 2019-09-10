// -----------------------------------------------------------------------
// <copyright file="AssessmentService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Exceptions;
using Microsoft.Azure.CCME.Assessment.Managers;
using Microsoft.Azure.CCME.Assessment.Models;
using Microsoft.Azure.CCME.Assessment.Services.ServiceFactories;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Services
{
    public sealed class AssessmentService : IAssessmentService
    {
        private const string TelemetryLogSection = "AssessmentService";

        public async Task<AssessmentReport> GenerateReportAsync(
            IAssessmentContext context,
            string targetRegion)
        {
            var serviceFactory = this.GetServiceFactory(context);

            // TODO: add telemetry and performance metrics for each steps.
            var billProvider = serviceFactory.GetBillProvider();
            var listPriceProvider = serviceFactory.GetListPriceProvider();

            // TODO: resolve from IoC container.
            var serviceParityManager = new ServiceParityManager(context);

            var detailedResourceTypes = serviceParityManager.LoadRules();

            context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Verbose,
                TelemetryLogSection,
                FormattableString.Invariant($"Got {detailedResourceTypes.Count()} detailed resource types"));

            IEnumerable<SubscriptionModel> subscriptions = null;
            try
            {
                subscriptions = await this.GetResourcesAsync(context, detailedResourceTypes);
            }
            catch (Exception ex)
            {
                throw new ResourceException("Failed to retrieve Azure resource data due to insufficient permission. Please check your Azure RBAC role and ensure you have any of the role below for the whole subscription: Reader, Contributor and Owner.", ex);
            }

            var resources = subscriptions.SelectMany(m => m.ResourceGroups.SelectMany(r => r.Value));

            context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Verbose,
                TelemetryLogSection,
                FormattableString.Invariant($"Got {resources.Count()} resources"));

            var serviceParityResult = serviceParityManager.Process(subscriptions, targetRegion);

            context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Verbose,
                TelemetryLogSection,
                FormattableString.Invariant($"Got service parity pass result: {serviceParityResult.Pass} with {serviceParityResult.Details.Count()} detail items"));

            var costEstimationManager = new CostEstimationManager(
                context,
                billProvider,
                listPriceProvider);

            var costEstimationResult = await costEstimationManager.ProcessAsync(
                subscriptions,
                targetRegion);

            context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Verbose,
                TelemetryLogSection,
                FormattableString.Invariant($"Got cost estimation result for {costEstimationResult.ResourcesCount} resources of {costEstimationResult.ResourceGroupsCount} resource groups"));

            var reportManager = new ReportManager(context);

            var assessmentReport = await reportManager.ProcessAsync(
                serviceParityResult,
                costEstimationResult);

            context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Verbose,
                TelemetryLogSection,
                FormattableString.Invariant($"Generated assessment report in local file: {assessmentReport.ReportFilePath}"));

            return assessmentReport;
        }

        private IServiceFactory GetServiceFactory(IAssessmentContext context)
        {
            if (context.ARMAccessToken != null)
            {
                return new ServiceFactoryForAccessToken(context);
            }

            if (context.UsageReport != null)
            {
                return new ServiceFactoryForUsageReport(context);
            }

            throw new ArgumentException("Missing access token or usage file");
        }

        private async Task<IEnumerable<SubscriptionModel>> GetResourcesAsync(IAssessmentContext context, IEnumerable<string> detailedResourceTypes)
        {
            if (!string.IsNullOrWhiteSpace(context.ResourceCachePath))
            {
                try
                {
                    var content = File.ReadAllText(context.ResourceCachePath);
                    return JsonConvert.DeserializeObject<IEnumerable<SubscriptionModel>>(content);
                }
                catch
                {
                    // Failed get resources from local cache. Clear `detailedResourceTypes` to force the full list retrieved by API
                    detailedResourceTypes = null;
                }
            }

            var serviceFactory = this.GetServiceFactory(context);
            var resourceManager = serviceFactory.GetResourceManager();
            var subscriptions = await resourceManager.GetResourcesAsync(detailedResourceTypes);

            if (!string.IsNullOrWhiteSpace(context.ResourceCachePath))
            {
                File.WriteAllText(
                    context.ResourceCachePath,
                    JsonConvert.SerializeObject(
                        subscriptions,
                        Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        }));
            }

            return subscriptions;
        }
    }
}
