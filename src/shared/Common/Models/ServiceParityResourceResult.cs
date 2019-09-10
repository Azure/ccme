// -----------------------------------------------------------------------
// <copyright file="ServiceParityResourceResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Models
{
    public sealed class ServiceParityResourceResult
    {
        [JsonProperty("details")]
        public IEnumerable<ServiceParityResourceDetail> Details { get; set; }

        [JsonProperty("pass")]
        public bool Pass => this.Details.All(r => r.Pass);

        [JsonProperty("exception")]
        public Exception Exception { get; set; }
    }
}