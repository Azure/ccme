// -----------------------------------------------------------------------
// <copyright file="ServiceParityResourceDetail.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Models
{
    public sealed class ServiceParityResourceDetail
    {
        [JsonProperty("pass")]
        public bool Pass { get; set; }

        [JsonProperty("ruleName")]
        public string RuleName { get; set; }

        [JsonProperty("ruleSetId")]
        public string RuleSetID { get; set; }

        [JsonProperty("severity")]
        public ServiceParitySeverityModel Severity { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("brief")]
        public string Brief { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}