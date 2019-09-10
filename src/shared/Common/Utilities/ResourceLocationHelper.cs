// -----------------------------------------------------------------------
// <copyright file="ResourceLocationHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Rest;

namespace Microsoft.Azure.CCME.Assessment.Utilities
{
    public static class ResourceLocationHelper
    {
        public static async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> GetLocationMap(
            string accessToken,
            string endpoint,
            IList<string> subscrptionIds)
        {
            if (accessToken == null)
            {
                return subscrptionIds.ToDictionary(
                    subscriptionId => subscriptionId,
                    subscriptionId => new Dictionary<string, string>() as IReadOnlyDictionary<string, string>);
            }

            using (var client = new SubscriptionClient(
                new Uri(endpoint),
                new TokenCredentials(accessToken)))
            {
                var locationMap = new Dictionary<string, IReadOnlyDictionary<string, string>>();
                foreach (var subscriptionId in subscrptionIds)
                {
                    try
                    {
                        var locations = await client.Subscriptions.ListLocationsAsync(subscriptionId);
                        var subLocationMap = locations.ToDictionary(location => location.Name, location => location.DisplayName);

                        locationMap.Add(subscriptionId, subLocationMap);
                    }
                    catch
                    {
                        locationMap.Add(subscriptionId, new Dictionary<string, string>());
                    }
                }

                return locationMap;
            }
        }
    }
}
