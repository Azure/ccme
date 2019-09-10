// -----------------------------------------------------------------------
// <copyright file="TenantInfoArray.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Models
{
    public sealed class TenantInfoArray
    {
        [JsonProperty("value")]
        public IEnumerable<TenantInfo> Tenants { get; set; }
    }
}
