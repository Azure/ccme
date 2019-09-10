// -----------------------------------------------------------------------
// <copyright file="ServiceFactoryForAccessToken.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers;
using Microsoft.Azure.CCME.Assessment.Managers.BillingProviders;
using Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders;

namespace Microsoft.Azure.CCME.Assessment.Services.ServiceFactories
{
    public class ServiceFactoryForAccessToken : ServiceFactoryBase, IServiceFactory
    {
        public ServiceFactoryForAccessToken(IAssessmentContext context)
            : base(context)
        {
        }

        public IResourceManager GetResourceManager()
            => new ResourceManager(this.Context);

        public IBillingProvider GetBillProvider()
            => new RateCardBillingProvider(this.Context);

        public IListPriceProvider GetListPriceProvider()
            => new RateCardListPriceProvider(this.Context);
    }
}
