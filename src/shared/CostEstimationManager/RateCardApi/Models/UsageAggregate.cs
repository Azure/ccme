// -----------------------------------------------------------------------
// <copyright file="UsageAggregate.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models
{
    internal sealed class UsageAggregate
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public Properties Properties { get; set; }

        [JsonIgnore]
        public string ResourceUri =>
            this.Properties.InstanceData?.MicrosoftResources.ResourceUri;
    }
}
