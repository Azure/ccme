// -----------------------------------------------------------------------
// <copyright file="IBillingProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.BillingProviders
{
    public interface IBillingProvider
    {
        Task<IEnumerable<ResourceUsage>> GetUsagesAsync(
            IEnumerable<SubscriptionModel> subscriptions,
            DateTime startTime,
            DateTime endTime);
    }
}
