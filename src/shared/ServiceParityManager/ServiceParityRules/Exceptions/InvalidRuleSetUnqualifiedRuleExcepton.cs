// -----------------------------------------------------------------------
// <copyright file="InvalidRuleSetUnqualifiedRuleExcepton.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Azure.CCME.Assessment.Managers.ServiceParityRules.Exceptions
{
    internal class InvalidRuleSetUnqualifiedRuleExcepton : ArgumentException
    {
        public string RuleSetID { get; }
        public string RuleName { get; }

        // ToDo: refine message
        public InvalidRuleSetUnqualifiedRuleExcepton(string ruleSetID, string ruleName)
        {
            this.RuleSetID = ruleSetID;
            this.RuleName = ruleName;
        }
    }
}