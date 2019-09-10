// -----------------------------------------------------------------------
// <copyright file="ResourceUsage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Managers.BillingProviders
{
    public sealed class ResourceUsage
    {
        public string ResourceUri { get; set; }

        public string MeterId { get; set; }

        public double Quantity { get; set; }
    }
}
