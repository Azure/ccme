// -----------------------------------------------------------------------
// <copyright file="IRuleEvaluator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Models;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Evaluators
{
    internal interface IRuleEvaluator
    {
        /// <summary>
        /// Evaluate the rule with given values
        /// </summary>
        /// <param name="match">Values captured by JsonMatch</param>
        /// <returns>Rule output, including pass/failed, message and so on</returns>
        RuleEngineOutputModel Evaluate(Match match);
    }
}