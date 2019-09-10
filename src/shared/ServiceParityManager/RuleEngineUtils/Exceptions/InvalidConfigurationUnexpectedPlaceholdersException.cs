// -----------------------------------------------------------------------
// <copyright file="InvalidConfigurationUnexpectedPlaceholdersException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Exceptions
{
    internal class InvalidConfigurationUnexpectedPlaceholdersException : ArgumentException
    {
        public string RuleName { get; }
        public string RuleSetID { get; }
        public IEnumerable<string> UnexpectedPlaceholders { get; }
        public string BrokenListItem { get; }

        // ToDo: refine message
        public InvalidConfigurationUnexpectedPlaceholdersException(RuleModel rule, IEnumerable<string> unexpectedPlaceholders, PrimitiveValueSet brokenListItem)
        {
            this.RuleName = rule.Name;
            this.RuleSetID = rule.RuleSetID;
            this.UnexpectedPlaceholders = unexpectedPlaceholders.ToArray();
            this.BrokenListItem = JsonConvert.SerializeObject(brokenListItem, Formatting.Indented);
        }
    }
}