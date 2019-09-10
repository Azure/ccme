// -----------------------------------------------------------------------
// <copyright file="RuleSeverityModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum RuleSeverityModel
    {
        Unknown,
        Information,
        Warning,
        Error,
        Critical
    }
}