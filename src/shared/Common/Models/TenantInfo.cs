// -----------------------------------------------------------------------
// <copyright file="TenantInfo.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Models
{
    public sealed class TenantInfo
    {
        [JsonProperty("tenantId")]
        public string TenentId { get; set; }

        [JsonProperty("displayName")]
        public string TenantName { get; set; }
    }
}
