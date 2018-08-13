// -----------------------------------------------------------------------
// <copyright file="PrimitiveValueSet.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Helpers;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Models
{
    [JsonDictionary(ItemConverterType = typeof(PrimitiveValueConverter))]
    internal sealed class PrimitiveValueSet : Dictionary<string, object>
    {
        /// <summary>
        /// Expand single value set contains array field to multiple value sets contains only primitive value
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PrimitiveValueSet> Expand()
        {
            var arrayKey = this.Keys.FirstOrDefault(key => this[key] is Array);
            if (arrayKey == null)
            {
                yield return this;
            }
            else
            {
                foreach (var value in this[arrayKey] as Array)
                {
                    var child = this.CreateVariation(arrayKey, value);
                    foreach (var expanded in child.Expand())
                    {
                        yield return expanded;
                    }
                }
            }
        }

        private PrimitiveValueSet CreateVariation(string key, object newValue)
        {
            var result = new PrimitiveValueSet();
            foreach (var pair in this)
            {
                result.Add(pair.Key, pair.Key == key ? newValue : pair.Value);
            }

            return result;
        }
    }
}