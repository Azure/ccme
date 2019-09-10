// -----------------------------------------------------------------------
// <copyright file="DictionaryExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.CCME.Assessment.Extensions
{
    public static class DictionaryExtension
    {
        public static void Merge<TKey, TValue>(
            this IDictionary<TKey, TValue> original,
            IDictionary<TKey, TValue> toBeMerged)
        {
            if (toBeMerged == null)
            {
                return;
            }

            foreach (var kvp in toBeMerged)
            {
                original[kvp.Key] = kvp.Value;
            }
        }

        public static IDictionary<TKey, TValue> Combine<TKey, TValue>(
             this IDictionary<TKey, TValue> original,
             IDictionary<TKey, TValue> toBeMerged)
        {
            var result = new Dictionary<TKey, TValue>();

            result.Merge(original);
            result.Merge(toBeMerged);

            return result;
        }
    }
}
