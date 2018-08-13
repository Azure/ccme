// -----------------------------------------------------------------------
// <copyright file="RateCardBillingProviderForUsageReport.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.BillingProviders
{
    public sealed class RateCardBillingProviderForUsageReport : IBillingProvider
    {
        private readonly IAssessmentContext context;

        public RateCardBillingProviderForUsageReport(IAssessmentContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ResourceUsage>> GetUsagesAsync(
            IEnumerable<SubscriptionModel> subscriptions,
            DateTime unusedStartTime,
            DateTime unusedEndTime)
        {
            return await Task.FromResult(this.context.UsageReport.Meters
                .GroupBy(m => new
                {
                    resourceId = m.ResourceId,
                    meterId = m.MeterId
                })
                .Select(g => new ResourceUsage
                {
                    ResourceUri = g.Key.resourceId,
                    MeterId = g.Key.meterId,
                    Quantity = g.Sum(m => m.Quantity)
                }));
        }
    }
}