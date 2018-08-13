// -----------------------------------------------------------------------
// <copyright file="ListRuleEvaluatorConfigurationsModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models
{
    internal sealed class ListRuleEvaluatorConfigurationsModel
    {
        [JsonProperty("localization")]
        public LocalizationModel Localization { get; set; }

        [JsonProperty("blacklist")]
        public IEnumerable<BlacklistItemModel> Blacklist { get; set; }

        [JsonProperty("blacklistDefaultHitMessage")]
        public string BlacklistDefaultHitMessage { get; set; }

        [JsonProperty("blacklistDefaultHitResource")]
        public string BlacklistDefaultHitResource { get; set; }

        [JsonProperty("whitelist")]
        public IEnumerable<PrimitiveValueSet> Whitelist { get; set; }

        [JsonProperty("whitelistNoHitMessage")]
        public string WhitelistNoHitMessage { get; set; }

        [JsonProperty("whitelistNoHitResource")]
        public string WhitelistNoHitResource { get; set; }
    }
}