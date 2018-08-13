// -----------------------------------------------------------------------
// <copyright file="RuleSetModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models
{
    internal sealed class RuleSetModel
    {
        [JsonProperty("contentVersion")]
        public string ContentVersion { get; set; }

        [JsonProperty("rules")]
        public IEnumerable<RuleModel> Rules { get; set; }
    }
}