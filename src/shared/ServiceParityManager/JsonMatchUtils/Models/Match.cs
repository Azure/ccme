// -----------------------------------------------------------------------
// <copyright file="Match.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Helpers;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Models
{
    internal sealed class Match
    {
        public Dictionary<string, CapturingGroup> Groups { get; set; } = new Dictionary<string, CapturingGroup>();

        public bool Equals(Dictionary<string, object> dict, bool ignoreCase)
        {
            return this.Groups.Count == dict.Count
                && this.Groups.Values.All(g => ObjectEqualityComparer.Equals(dict[g.Key], g.Value, ignoreCase));
        }
    }
}