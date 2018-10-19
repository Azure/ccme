// -----------------------------------------------------------------------
// <copyright file="SubscriptionHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    internal sealed class SubscriptionHelper : ISubscriptionHelper
    {
        private readonly IFactory factory;

        public SubscriptionHelper(IFactory factory)
        {
            this.factory = factory;
        }

        public async Task<string> GetSubscriptionNameByIdAsync(string subscriptionId)
        {
            using (var client = this.factory.CreateSubscriptionClient())
            {
                var subscription = await client.Subscriptions.GetAsync(subscriptionId);

                return subscription.DisplayName;
            }
        }

        public async Task<IDictionary<string, string>> ListSubscriptionsAsync()
        {
            using (var client = this.factory.CreateSubscriptionClient())
            {
                var result = new Dictionary<string, string>();
                var page = await client.Subscriptions.ListAsync();
                do
                {
                    foreach (var sub in page)
                    {
                        result[sub.SubscriptionId] = sub.DisplayName;
                    }

                    if (!string.IsNullOrWhiteSpace(page.NextPageLink))
                    {
                        page = await client.Subscriptions.ListNextAsync(page.NextPageLink);
                    }
                    else
                    {
                        page = null;
                    }
                }
                while (page != null);

                return new ReadOnlyDictionary<string, string>(result);
            }
        }

        public async Task<IEnumerable<string>> GetSubscriptionIdsAsync(
            IEnumerable<string> subscriptionNames,
            IEnumerable<string> subscriptionIds)
        {
            var idSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (subscriptionNames != null)
            {
                var tasks = subscriptionNames.Select(async name => await this.GetSubscriptionIdByNameAsync(name));
                idSet.UnionWith(await Task.WhenAll(tasks));
            }

            if (subscriptionIds != null)
            {
                idSet.UnionWith(subscriptionIds);
            }

            return idSet.Where(id => !string.IsNullOrWhiteSpace(id));
        }

        public async Task<string> GetSubscriptionIdByNameAsync(string subscriptionName)
        {
            using (var client = this.factory.CreateSubscriptionClient())
            {
                var subscriptions = await client.Subscriptions.ListAsync();
                var targetSubscription = await subscriptions.ForEachAsync(
                    async page => await client.Subscriptions.ListNextAsync(page.NextPageLink),
                    (subscriptionInner) => subscriptionInner.DisplayName != subscriptionName);

                return targetSubscription?.SubscriptionId;
            }
        }
    }
}