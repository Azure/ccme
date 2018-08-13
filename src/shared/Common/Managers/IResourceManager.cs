// -----------------------------------------------------------------------
// <copyright file="IResourceManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public interface IResourceManager
    {
        Task<string> GetSubscriptionNameAsync(string subscriptionId);

        Task<IDictionary<string, string>> ListSubscriptionsAsync();

        Task<IEnumerable<SubscriptionModel>> GetResourcesAsync(IEnumerable<string> detailedResourceTypes = null);
    }
}
