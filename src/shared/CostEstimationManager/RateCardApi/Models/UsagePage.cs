// -----------------------------------------------------------------------
// <copyright file="UsagePage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models
{
    internal sealed class UsagePage
    {
        [JsonProperty("value")]
        public IList<UsageAggregate> Items { get; set; }

        public string NextLink { get; set; }
    }
}
