// -----------------------------------------------------------------------
// <copyright file="InfoFields.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models
{
    internal sealed class InfoFields
    {
        public string MeteredRegion { get; set; }

        public string MeteredService { get; set; }

        public string Project { get; set; }

        public string MeteredServiceType { get; set; }

        public string ServiceInfo1 { get; set; }
    }
}
