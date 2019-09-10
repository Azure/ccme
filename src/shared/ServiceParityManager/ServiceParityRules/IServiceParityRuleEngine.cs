// -----------------------------------------------------------------------
// <copyright file="IServiceParityRuleEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.ServiceParityRules
{
    internal interface IServiceParityRuleEngine
    {
        ServiceParityResult Process(IEnumerable<ResourceModel> resources, IEnumerable<RuleModel> rules, string targetRegion);
    }
}