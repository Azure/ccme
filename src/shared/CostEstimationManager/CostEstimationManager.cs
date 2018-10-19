// -----------------------------------------------------------------------
// <copyright file="CostEstimationManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers.BillingProviders;
using Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders;
using Microsoft.Azure.CCME.Assessment.Models;
using Microsoft.Azure.CCME.Assessment.Utilities;
using Microsoft.Rest.Azure;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public class CostEstimationManager : ICostEstimationManager
    {
        private const string TelemetryLogSection = "CostEstimation";

        private readonly IAssessmentContext context;
        private readonly IBillingProvider billingProvider;
        private readonly IListPriceProvider listPriceProvider;

        public CostEstimationManager(
            IAssessmentContext context,
            IBillingProvider billingProvider,
            IListPriceProvider listPriceProvider)
        {
            this.context = context;
            this.billingProvider = billingProvider;
            this.listPriceProvider = listPriceProvider;
        }

        public async Task<CostEstimationResult> ProcessAsync(
            IEnumerable<SubscriptionModel> subscriptions,
            string targetRegion)
        {
            var resources = subscriptions.SelectMany(m => m.ResourceGroups.SelectMany(r => r.Value));
            this.context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Information,
                TelemetryLogSection,
                FormattableString.Invariant($"Estimating cost of {resources.Count()} resources for region: {targetRegion}"));

            DateTime startTime, endTime;

            if (this.context.ARMAccessToken != null)
            {
                startTime = DateTime.UtcNow.Date.AddDays(-2);
                endTime = DateTime.UtcNow.Date.AddDays(-1);
            }
            else if (this.context.UsageReport != null)
            {
                startTime = this.context.UsageReport.Meters.Min(m => m.Date);
                endTime = this.context.UsageReport.Meters.Max(m => m.Date);
            }
            else
            {
                throw new ArgumentException("Missing access token or usage file");
            }

            this.context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Information,
                TelemetryLogSection,
                FormattableString.Invariant($"Retrieving usage of {resources.Count()} resources from {startTime} to {endTime}."));

            var usages = await this.billingProvider.GetUsagesAsync(subscriptions, startTime, endTime);
            this.context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Information,
                TelemetryLogSection,
                FormattableString.Invariant($"Got {usages.Count()} usages from billing provider:"),
                usages.Select(usage => FormattableString.Invariant($"[{usage.MeterId}] [{usage.Quantity}] {usage.ResourceUri}")));

            var listPriceProvider = new RateCardListPriceProvider(this.context);
            var meterIds = new HashSet<string>(usages.Select(u => u.MeterId).Distinct());

            Dictionary<string, ListPriceMeter> sourceMeters = null;
            Dictionary<string, ListPriceMeter> targetMeters = null;
            bool sourceMeterError = false;

            try
            {
                sourceMeters = (await this.listPriceProvider.GetMetersAsync(subscriptions.Select(m => m.SubscriptionId), meterIds))
                  .ToDictionary(m => m.MeterId, StringComparer.OrdinalIgnoreCase);
                this.context.TelemetryManager.WriteLog(
                    TelemetryLogLevel.Information,
                    TelemetryLogSection,
                    FormattableString.Invariant($"Got {sourceMeters.Count()} price meters from source regions:"),
                    sourceMeters.Values
                        .OrderBy(m => m.CrossEnvironmentId)
                        .Select(meter => FormattableString.Invariant($"[{meter.MeterId}] {meter.CrossEnvironmentId}")));
            }
            catch (CloudException ex)
            {
                sourceMeterError = true;
                this.context.TelemetryManager.WriteLog(
                    TelemetryLogLevel.Error,
                    TelemetryLogSection,
                    "Got exception when getting price meters!",
                    null,
                    ex);
            }

            if (!sourceMeterError)
            {
                var targetListPriceProvider = new TargetListPriceProvider(this.context);

                targetMeters = (await targetListPriceProvider.GetMetersAsync(targetRegion, sourceMeters.Values))
                    .ToDictionary(m => m.CrossEnvironmentId, StringComparer.OrdinalIgnoreCase);
                this.context.TelemetryManager.WriteLog(
                    TelemetryLogLevel.Information,
                    TelemetryLogSection,
                    FormattableString.Invariant($"Got {targetMeters.Count()} price meters from target region:"),
                    targetMeters.Values
                        .OrderBy(m => m.CrossEnvironmentId)
                        .Select(meter => FormattableString.Invariant($"[{meter.MeterId}] {meter.CrossEnvironmentId}")));
            }

            var details = resources.SelectMany(resource => this.GetCostEstimationResult(
                resource,
                usages.Where(u => u.ResourceUri.Equals(resource.Id, StringComparison.OrdinalIgnoreCase)),
                sourceMeters,
                targetMeters)).ToList();

            this.context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Information,
                TelemetryLogSection,
                FormattableString.Invariant($"Got {details.Count()} cost estimation results:"),
                details.Select(detail => FormattableString.Invariant($"[{detail.UsagesQuantity}] [{detail.OriginalCost}] [{detail.EstimatedCost}] {detail.ResourceId} - {detail.MeterCategory}/{detail.MeterSubCategory}/{detail.MeterName}")));

            return new CostEstimationResult
            {
                Details = details,
                StartTime = startTime,
                EndTime = endTime,
                TargetRegion = targetRegion,
                SubscriptionName = subscriptions.First().SubscriptionName,
                SubscriptionId = subscriptions.First().SubscriptionId,
                LocationMap = await ResourceLocationHelper.GetLocationMap(
                    this.context.ARMAccessToken,
                    this.context.ARMBaseUri,
                    subscriptions.Select(s => s.SubscriptionId).ToList()),
                HasError = sourceMeterError
            };
        }

        private IEnumerable<CostEstimationDetail> GetCostEstimationResult(
            ResourceModel resource,
            IEnumerable<ResourceUsage> usages,
            IReadOnlyDictionary<string, ListPriceMeter> sourceMeters,
            IReadOnlyDictionary<string, ListPriceMeter> targetMeters)
        {
            if (!usages.Any())
            {
                usages = new ResourceUsage[] { null };
            }

            return usages.Select(usage =>
            {
                ListPriceMeter sourceMeter = null;
                ListPriceMeter targetMeter = null;

                if (usage != null)
                {
                    if (sourceMeters.TryGetValue(usage.MeterId, out sourceMeter))
                    {
                        if (!targetMeters.TryGetValue(sourceMeter.CrossEnvironmentId, out targetMeter))
                        {
                            this.context.TelemetryManager.WriteLog(
                                TelemetryLogLevel.Warning,
                                TelemetryLogSection,
                                FormattableString.Invariant($"Unknown target price meter: {sourceMeter.CrossEnvironmentId}"));
                        }
                    }
                    else
                    {
                        this.context.TelemetryManager.WriteLog(
                            TelemetryLogLevel.Warning,
                            TelemetryLogSection,
                            FormattableString.Invariant($"Unknown source price meter: {usage.MeterId}"));
                    }
                }
                else
                {
                    this.context.TelemetryManager.WriteLog(
                        TelemetryLogLevel.Warning,
                        TelemetryLogSection,
                        FormattableString.Invariant($"Cannot find usage for resource {resource.Id}"));
                }

                var resourceId = new ResourceId(resource.Id);
                return new CostEstimationDetail
                {
                    ResourceId = resource.Id,
                    ResourceType = resource.Details?.Value<string>("type") ?? $"{resourceId.ResourceType}",
                    ResourceName = resourceId.ResourceName,
                    ResourceGroupName = resourceId.ResourceGroup,
                    ResourceModel = resource,
                    Location = resource.Details?.Value<string>("location") ?? "N/A",
                    UsagesQuantity = usage?.Quantity,
                    MeterName = sourceMeter?.MeterName,
                    MeterCategory = sourceMeter?.MeterCategory,
                    MeterSubCategory = sourceMeter?.MeterSubCategory,
                    SourceMeterId = sourceMeter?.MeterId,
                    TargetMeterId = targetMeter?.MeterId,
                    OriginalCost = CalculateCost(usage?.Quantity, sourceMeter),
                    EstimatedCost = CalculateCost(usage?.Quantity, targetMeter)
                };
            });
        }

        private static decimal? CalculateCost(
            double? quantity,
            ListPriceMeter meter)
        {
            if (quantity == null || meter == null)
            {
                return null;
            }

            decimal cost = 0;
            double remainQuantity = quantity.Value;
            for (int i = meter.MeterRates.Count - 1; i >= 0; i--)
            {
                double highestTier = meter.MeterRates.Keys[i];
                if (remainQuantity > highestTier)
                {
                    cost += (decimal)(remainQuantity - highestTier) * meter.MeterRates.Values[i];
                    remainQuantity = highestTier;
                }
            }

            return cost;
        }
    }
}
