// -----------------------------------------------------------------------
// <copyright file="RateCardListPriceProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers.RateCardApi;

namespace Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders
{
    public sealed class RateCardListPriceProvider : IListPriceProvider
    {
        private readonly IAssessmentContext context;

        public RateCardListPriceProvider(IAssessmentContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ListPriceMeter>> GetMetersAsync(
            IEnumerable<string> subscriptionIds,
            ISet<string> meterIds)
        {
            // TODO: resolve from IoC container
            using (var client = new RateCardClient(this.context.ARMAccessToken, this.context.ARMBaseUri))
            {
                var tasks = subscriptionIds.Select(async subscriptionId => await client.GetPayloadAsync(subscriptionId));
                var payloads = await Task.WhenAll(tasks);

                return payloads
                    .SelectMany(p => p.Meters.Select(m => new ListPriceMeter(m)))
                    .Where(m => meterIds.Contains(m.MeterId))
                    .GroupBy(m => m.MeterId)
                    .Select(g => g.First());
            }
        }
    }
}
