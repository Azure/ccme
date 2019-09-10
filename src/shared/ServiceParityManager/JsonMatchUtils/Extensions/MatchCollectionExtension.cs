// -----------------------------------------------------------------------
// <copyright file="MatchCollectionExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Extensions
{
    internal static class MatchCollectionExtension
    {
        public static IEnumerable<Match> Multiply(this IEnumerable<Match> collectionA, IEnumerable<Match> collectionB)
        {
            foreach (var matchA in collectionA)
            {
                foreach (var matchB in collectionB)
                {
                    yield return new Match
                    {
                        Groups = matchA.Groups.Union(matchB.Groups).ToDictionary(pair => pair.Key, pair => pair.Value)
                    };
                }
            }
        }
    }
}