// -----------------------------------------------------------------------
// <copyright file="MicrosoftResourcesDataType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models
{
    public sealed class MicrosoftResourcesDataType
    {
        public string ResourceUri { get; set; }

        public IDictionary<string, string> Tags { get; set; }

        public IDictionary<string, string> AdditionalInfo { get; set; }

        public string Location { get; set; }

        public string PartNumber { get; set; }

        public string OrderNumber { get; set; }
    }
}
