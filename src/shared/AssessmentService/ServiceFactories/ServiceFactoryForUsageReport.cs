// -----------------------------------------------------------------------
// <copyright file="ServiceFactoryForUsageReport.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers;
using Microsoft.Azure.CCME.Assessment.Managers.BillingProviders;
using Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders;

namespace Microsoft.Azure.CCME.Assessment.Services.ServiceFactories
{
    public class ServiceFactoryForUsageReport : ServiceFactoryBase, IServiceFactory
    {
        public ServiceFactoryForUsageReport(IAssessmentContext context)
            : base(context)
        {
        }

        public IResourceManager GetResourceManager()
            => new ResourceManagerForUsageReport(this.context);

        public IBillingProvider GetBillProvider()
            => new RateCardBillingProviderForUsageReport(this.context);

        public IListPriceProvider GetListPriceProvider()
            => new RateCardListPriceProviderForUsageReport(this.context);
    }
}
