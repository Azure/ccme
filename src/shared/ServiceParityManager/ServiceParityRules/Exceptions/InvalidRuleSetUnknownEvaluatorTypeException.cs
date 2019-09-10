// -----------------------------------------------------------------------
// <copyright file="InvalidRuleSetUnknownEvaluatorTypeException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Azure.CCME.Assessment.Managers.ServiceParityRules.Exceptions
{
    internal class InvalidRuleSetUnknownEvaluatorTypeException : ArgumentOutOfRangeException
    {
        public string RuleSetID { get; }
        public string RuleName { get; }
        public string EvaluatorType { get; }

        // ToDo: refine message
        public InvalidRuleSetUnknownEvaluatorTypeException(string ruleSetID, string ruleName, string evaluatorType)
        {
            this.RuleSetID = ruleSetID;
            this.RuleName = ruleName;
            this.EvaluatorType = evaluatorType;
        }
    }
}