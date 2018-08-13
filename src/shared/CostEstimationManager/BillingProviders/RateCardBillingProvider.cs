// -----------------------------------------------------------------------
// <copyright file="RateCardBillingProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers.RateCardApi;
using Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models;
using Microsoft.Azure.CCME.Assessment.Models;
using Microsoft.Rest.Azure;

namespace Microsoft.Azure.CCME.Assessment.Managers.BillingProviders
{
    public sealed class RateCardBillingProvider : IBillingProvider
    {
        private readonly IAssessmentContext context;

        public RateCardBillingProvider(IAssessmentContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ResourceUsage>> GetUsagesAsync(
            IEnumerable<SubscriptionModel> subscriptions,
            DateTime startTime,
            DateTime endTime)
        {
            var resourceUsages = new List<ResourceUsage>();

            using (var client = new RateCardClient(this.context.ARMAccessToken, this.context.ARMBaseUri))
            {
                // TODO: revisit here to check whether contains non resource id specific usages, e.g. legacy resources.
                foreach (var subscription in subscriptions)
                {
                    IEnumerable<UsageAggregate> usages;

                    try
                    {
                        usages = await client.GetUsagesAsync(subscription.SubscriptionId, startTime, endTime);
                    }
                    catch (CloudException ex)
                    {
                        if (this.context.ARMBaseUri == new AzureEnvironmentHelper("AzureChinaCloud").ResourceManagerEndpoint
                            && ex.Message.StartsWith("The resource type could not be found in the namespace 'Microsoft.Commerce'"))
                        {
                            // Skip usage retrieving since the RP is not available yet in Azure China Cloud
                            usages = new UsageAggregate[] { };
                        }
                        else
                        {
                            throw;
                        }
                    }

                    var resourceIds = new HashSet<string>(
                        subscription.ResourceGroups
                            .SelectMany(r => r.Value)
                            .Select(m => m.Id)
                            .Distinct(),
                        StringComparer.InvariantCultureIgnoreCase);

                    resourceUsages.AddRange(usages
                        .Where(u => resourceIds.Contains(u.ResourceUri))
                        .GroupBy(u => new
                        {
                            resourceUri = u.ResourceUri,
                            meterId = u.Properties.MeterId
                        })
                        .Select(g => new ResourceUsage
                        {
                            ResourceUri = g.Key.resourceUri,
                            MeterId = g.Key.meterId,
                            Quantity = g.Sum(m => m.Properties.Quantity)
                        }));
                }
            }

            return resourceUsages;
        }
    }
}
