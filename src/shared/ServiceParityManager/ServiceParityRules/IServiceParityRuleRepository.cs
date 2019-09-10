// -----------------------------------------------------------------------
// <copyright file="IServiceParityRuleRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.ServiceParityRules
{
    internal interface IServiceParityRuleRepository
    {
        IEnumerable<RuleModel> GetRules();
    }
}