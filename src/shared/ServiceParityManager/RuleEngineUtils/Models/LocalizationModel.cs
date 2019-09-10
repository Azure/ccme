// -----------------------------------------------------------------------
// <copyright file="LocalizationModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models
{
    internal sealed class LocalizationModel
    {
        [JsonProperty("resources")]
        public IEnumerable<string> Resources { get; set; }
    }
}