// -----------------------------------------------------------------------
// <copyright file="InvalidRuleSetDependencyResolveFailedException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.ServiceParityRules.Exceptions
{
    internal class InvalidRuleSetDependencyResolveFailedException : ArgumentException
    {
        public string RuleSetID { get; }
        public IEnumerable<string> UndecidedRules { get; }

        // ToDo: refine message
        public InvalidRuleSetDependencyResolveFailedException(string ruleSetID, IEnumerable<string> undecidedRules)
        {
            this.RuleSetID = ruleSetID;
            this.UndecidedRules = undecidedRules.ToArray();
        }
    }
}