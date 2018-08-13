// -----------------------------------------------------------------------
// <copyright file="PlaceholderHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Extensions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Helpers
{
    internal static class PlaceholderHelper
    {
        private static readonly System.Text.RegularExpressions.Regex RegexPlaceHolder = new System.Text.RegularExpressions.Regex(@"(?<!{)(?<full>{(?<name>\w+)})(?!})");

        public static bool IsPlaceholder(string input, out string placeholder)
        {
            var matches = RegexPlaceHolder.Matches(input);
            if (matches.Count != 1 || matches[0].Length != input.Length)
            {
                placeholder = null;
                return false;
            }
            else
            {
                placeholder = matches[0].Groups["name"].Value;
                return true;
            }
        }

        public static string Unescape(string input)
        {
            return input
                .Replace("{{", "{")
                .Replace("}}", "}");
        }

        public static string ReplacePlaceholders(
            string input,
            IReadOnlyDictionary<string, string> replacements,
            string defaultReplacement = "<null>")
        {
            return RegexPlaceHolder.Replace(
                input,
                match => replacements.Lookup(match.Groups["name"].Value, defaultReplacement));
        }

        public static IEnumerable<string> GetPlaceholders(JToken node)
        {
            foreach (var property in node.Children<JProperty>())
            {
                if (property.Value.Type == JTokenType.String)
                {
                    if (IsPlaceholder(property.Value.Value<string>(), out var placeholder))
                    {
                        yield return placeholder;
                    }
                }
                else if (property.Value.Type == JTokenType.Object)
                {
                    foreach (var placeholder in GetPlaceholders(property.Value))
                    {
                        yield return placeholder;
                    }
                }
                else if (property.Value.Type == JTokenType.Array)
                {
                    foreach (var item in property.Value as JArray)
                    {
                        foreach (var placeholder in GetPlaceholders(item))
                        {
                            yield return placeholder;
                        }
                    }
                }
            }
        }
    }
}