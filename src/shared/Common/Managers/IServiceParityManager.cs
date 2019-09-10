// -----------------------------------------------------------------------
// <copyright file="IServiceParityManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public interface IServiceParityManager
    {
        IEnumerable<string> LoadRules();

        ServiceParityResult Process(IEnumerable<SubscriptionModel> subscriptions, string targetRegion);
    }
}
