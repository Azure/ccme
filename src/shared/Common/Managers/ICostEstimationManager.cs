// -----------------------------------------------------------------------
// <copyright file="ICostEstimationManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public interface ICostEstimationManager
    {
        Task<CostEstimationResult> ProcessAsync(
            IEnumerable<SubscriptionModel> subscriptions,
            string targetRegion);
    }
}
