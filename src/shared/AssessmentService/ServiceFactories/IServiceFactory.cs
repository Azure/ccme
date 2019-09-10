// -----------------------------------------------------------------------
// <copyright file="IServiceFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.CCME.Assessment.Managers;
using Microsoft.Azure.CCME.Assessment.Managers.BillingProviders;
using Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders;

namespace Microsoft.Azure.CCME.Assessment.Services.ServiceFactories
{
    public interface IServiceFactory
    {
        IResourceManager GetResourceManager();
        IBillingProvider GetBillProvider();
        IListPriceProvider GetListPriceProvider();
    }
}
