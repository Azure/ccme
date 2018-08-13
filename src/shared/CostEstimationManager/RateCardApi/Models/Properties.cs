// -----------------------------------------------------------------------
// <copyright file="Properties.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models
{
    internal sealed class Properties
    {
        public string SubscriptionId { get; set; }

        public string UsageStartTime { get; set; }

        public string UsageEndTime { get; set; }

        public string MeterId { get; set; }

        public InfoFields InfoFields { get; set; }

        [JsonProperty("instanceData")]
        public string InstanceDataRaw { get; set; }

        [JsonIgnore]
        public InstanceDataType InstanceData =>
            this.InstanceDataRaw == null ? null
            : JsonConvert.DeserializeObject<InstanceDataType>(this.InstanceDataRaw.Replace("\\\"", string.Empty));

        public double Quantity { get; set; }

        public string Unit { get; set; }

        public string MeterName { get; set; }

        public string MeterCategory { get; set; }

        public string MeterSubCategory { get; set; }

        public string MeterRegion { get; set; }
    }
}
