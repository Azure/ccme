// -----------------------------------------------------------------------
// <copyright file="RuleEvaluatorModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models
{
    internal sealed class RuleEvaluatorModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("configKey")]
        public string ConfigKey { get; set; }

        public void Populate(RuleEvaluatorModel baseEvaluator)
        {
            this.Type = this.Type ?? baseEvaluator.Type;
            this.ConfigKey = this.ConfigKey ?? baseEvaluator.ConfigKey;
        }

        public bool IsQualified => !string.IsNullOrWhiteSpace(this.Type)
            && !string.IsNullOrWhiteSpace(this.ConfigKey);
    }
}