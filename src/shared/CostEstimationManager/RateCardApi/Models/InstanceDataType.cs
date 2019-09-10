// -----------------------------------------------------------------------
// <copyright file="InstanceDataType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models
{
    internal sealed class InstanceDataType
    {
        [JsonProperty("Microsoft.Resources")]
        public MicrosoftResourcesDataType MicrosoftResources { get; set; }
    }
}
