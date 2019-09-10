// -----------------------------------------------------------------------
// <copyright file="ServiceParityResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Models
{
    public sealed class ServiceParityResult
    {
        [JsonProperty("details")]
        public IReadOnlyDictionary<string, ServiceParityResourceResult> Details { get; set; }

        [JsonProperty("pass")]
        public bool Pass => this.Details.Values.All(r => r.Pass);
    }
}