// -----------------------------------------------------------------------
// <copyright file="RuleEngineOutputModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models
{
    internal sealed class RuleEngineOutputModel
    {
        public bool Pass { get; set; }
        public string RuleName { get; set; }
        public string RuleSetID { get; set; }
        public RuleSeverityModel Severity { get; set; }
        public string Category { get; set; }
        public string Source { get; set; }
        public string Brief { get; set; }
        public string Message { get; set; }
        public string Path { get; set; }
    }
}