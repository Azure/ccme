// -----------------------------------------------------------------------
// <copyright file="ISubscriptionHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    internal interface ISubscriptionHelper
    {
        Task<string> GetSubscriptionNameByIdAsync(string subscriptionId);

        Task<IDictionary<string, string>> ListSubscriptionsAsync();

        Task<IEnumerable<string>> GetSubscriptionIdsAsync(
            IEnumerable<string> subscriptionNames,
            IEnumerable<string> subscriptionIds);

        Task<string> GetSubscriptionIdByNameAsync(string subscriptionName);
    }
}