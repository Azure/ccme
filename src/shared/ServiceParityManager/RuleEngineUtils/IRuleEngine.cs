// -----------------------------------------------------------------------
// <copyright file="IRuleEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils
{
    internal interface IRuleEngine
    {
        IEnumerable<RuleEngineOutputModel> Analyze(JToken input);
    }
}