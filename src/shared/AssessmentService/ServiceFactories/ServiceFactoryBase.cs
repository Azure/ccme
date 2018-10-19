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
        protected IAssessmentContext Context { get; }

        public ServiceFactoryBase(IAssessmentContext context)
        {
            this.Context = context;
        }
    }
}
