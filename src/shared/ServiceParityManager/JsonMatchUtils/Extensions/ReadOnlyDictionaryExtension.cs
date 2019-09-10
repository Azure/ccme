// -----------------------------------------------------------------------
// <copyright file="ReadOnlyDictionaryExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Extensions
{
    internal static class ReadOnlyDictionaryExtension
    {
        public static string Lookup(this IReadOnlyDictionary<string, string> dict, string key, string defaultValue)
        {
            return dict.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}