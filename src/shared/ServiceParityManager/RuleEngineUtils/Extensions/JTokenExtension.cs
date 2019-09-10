// -----------------------------------------------------------------------
// <copyright file="JTokenExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Extensions
{
    internal static class JTokenExtension
    {
        /// <summary>
        /// Copy all missing fields from source object to destination object
        /// </summary>
        /// <param name="dst">destination object</param>
        /// <param name="src">source object</param>
        public static void Populate(this JToken dst, JToken src)
        {
            if (src == null)
            {
                return;
            }

            foreach (var property in src.Children<JProperty>())
            {
                if (dst[property.Name] == null)
                {
                    dst[property.Name] = property.Value;
                }
                else
                {
                    Populate(dst[property.Name], property.Value);
                }
            }
        }
    }
}