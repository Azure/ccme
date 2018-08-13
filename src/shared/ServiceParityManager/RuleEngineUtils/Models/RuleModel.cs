// -----------------------------------------------------------------------
// <copyright file="RuleModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Evaluators;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models
{
    internal sealed class RuleModel
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonIgnore]
        public string RuleSetID { get; set; }

        [DefaultValue(false)]
        [JsonProperty("isAbstract", DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsAbstract { get; set; }

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("brief")]
        public string Brief { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [DefaultValue(RuleSeverityModel.Unknown)]
        [JsonProperty("severity", DefaultValueHandling = DefaultValueHandling.Populate)]
        public RuleSeverityModel Severity { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("pattern")]
        public JToken Pattern { get; set; }

        [JsonProperty("evaluator")]
        public RuleEvaluatorModel EvaluatorModel { get; set; }

        [JsonIgnore]
        public IRuleEvaluator Evaluator { get; set; }

        [DefaultValue(true)]
        [JsonProperty("ignoreCase", DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Copy missing fields from base rule
        /// </summary>
        /// <param name="baseRule">The base rule</param>
        public void Populate(RuleModel baseRule)
        {
            this.Description = this.Description ?? baseRule.Description;

            if (this.Severity == RuleSeverityModel.Unknown)
            {
                this.Severity = baseRule.Severity;
            }

            this.Category = this.Category ?? baseRule.Category;

            this.Source = this.Source ?? baseRule.Source;

            if (this.Pattern == null)
            {
                this.Pattern = baseRule.Pattern;
            }
            else
            {
                this.Pattern.Populate(baseRule.Pattern);
            }

            if (this.EvaluatorModel == null)
            {
                this.EvaluatorModel = baseRule.EvaluatorModel;
            }
            else
            {
                this.EvaluatorModel.Populate(baseRule.EvaluatorModel);
            }
        }

        public bool IsQualified => !string.IsNullOrWhiteSpace(this.Name)
            && this.Severity != RuleSeverityModel.Unknown
            && !string.IsNullOrWhiteSpace(this.Category)
            && !string.IsNullOrWhiteSpace(this.Source)
            && this.Pattern != null
            && this.EvaluatorModel != null
            && this.EvaluatorModel.IsQualified;
    }
}