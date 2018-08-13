// -----------------------------------------------------------------------
// <copyright file="IFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    internal interface IFactory
    {
        ISubscriptionClient CreateSubscriptionClient();
        IResourceManagementClient CreateResourceManagementClient();
        ISubscriptionHelper CreateSubscriptionHelper();
        IResourceGroupHelper CreateResourceGroupHelper();
        IResourceHelper CreateResourceHelper(string subscriptionId);
    }
}