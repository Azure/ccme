// -----------------------------------------------------------------------
// <copyright file="RateCardListPriceProviderForUsageReport.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Environments;

namespace Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders
{
    public sealed class RateCardListPriceProviderForUsageReport : IListPriceProvider
    {
        private readonly IAssessmentContext context;

        public RateCardListPriceProviderForUsageReport(IAssessmentContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ListPriceMeter>> GetMetersAsync(
            IEnumerable<string> subscriptionIds,
            ISet<string> meterIds)
        {
            return await Task.FromResult(this.context.UsageReport.Meters
                .GroupBy(m => m.MeterId, StringComparer.InvariantCultureIgnoreCase)
                .Select(g => new ListPriceMeter(
                    g.Key,
                    g.First().MeterName,
                    g.First().MeterCategory,
                    g.First().MeterSubCategory,
                    new SortedList<double, decimal>())));
        }
    }
}