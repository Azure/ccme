// -----------------------------------------------------------------------
// <copyright file="ServiceFactoryBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.CCME.Assessment.Environments;

namespace Microsoft.Azure.CCME.Assessment.Services.ServiceFactories
{
    public class ServiceFactoryBase
    {
        protected readonly IAssessmentContext context;

        public ServiceFactoryBase(IAssessmentContext context)
        {
            this.context = context;
        }
    }
}
