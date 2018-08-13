// -----------------------------------------------------------------------
// <copyright file="MatchCollectionHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Helpers
{
    internal static class MatchCollectionHelper
    {
        public static IEnumerable<Match> CreateWithEmptyMatch()
        {
            return new[]
            {
                new Match
                {
                    Groups = new Dictionary<string, CapturingGroup>()
                }
            };
        }

        public static IEnumerable<Match> CreateWithSingleGroup(CapturingGroup group)
        {
            return new[]
            {
                new Match
                {
                    Groups = new Dictionary<string, CapturingGroup>
                    {
                        {
                            group.Key,
                            group
                        }
                    }
                }
            };
        }
    }
}