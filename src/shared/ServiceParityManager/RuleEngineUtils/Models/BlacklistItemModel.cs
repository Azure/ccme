// -----------------------------------------------------------------------
// <copyright file="BlacklistItemModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models
{
    internal sealed class BlacklistItemModel
    {
        [JsonProperty("values")]
        public PrimitiveValueSet Values { get; set; }

        [JsonProperty("hitMessage")]
        public string HitMessage { get; set; }

        [JsonProperty("hitResource")]
        public string HitResource { get; set; }

        public IEnumerable<BlacklistItemModel> Expand()
        {
            return this.Values.Expand().Select(set => new BlacklistItemModel
            {
                Values = set,
                HitMessage = this.HitMessage,
                HitResource = this.HitResource
            });
        }
    }
}