// -----------------------------------------------------------------------
// <copyright file="JsonMatchEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Exceptions;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Extensions;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Helpers;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils
{
    internal sealed class JsonMatchEngine : IJsonMatchEngine
    {
        private readonly JToken pattern;
        private readonly StringComparison stringComparison;

        public JsonMatchEngine(JToken pattern, bool ignoreCase = true)
        {
            this.pattern = pattern;
            this.stringComparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        }

        public IEnumerable<Match> Matches(JToken input)
        {
            return this.MatchSingleLevel(input, this.pattern);
        }

        private IEnumerable<Match> MatchSingleLevel(JToken input, JToken pattern)
        {
            if (pattern is JValue)
            {
                return this.MatchSingleValue(input as JValue, pattern as JValue);
            }

            if (pattern is JArray)
            {
                var patternArray = pattern as JArray;

                if (input is JArray)
                {
                    if (patternArray.Count == 1)
                    {
                        return this.MatchArraySinglePattern(input as JArray, patternArray.Single());
                    }
                    else
                    {
                        throw new NotSupportedArrayToArrayMatchException(input, pattern);
                    }
                }
                else
                {
                    return this.MatchArraySingleInput(input, patternArray);
                }
            }

            if (pattern is JObject)
            {
                var matches = MatchCollectionHelper.CreateWithEmptyMatch();
                foreach (var propertyPattern in pattern.Children<JProperty>())
                {
                    if (input.Type == JTokenType.Null)
                    {
                        return null;
                    }

                    var childInput = input[propertyPattern.Name];
                    if (childInput == null)
                    {
                        return null;
                    }

                    var subMatches = this.MatchSingleLevel(childInput, propertyPattern.Value);
                    if (subMatches == null)
                    {
                        return null;
                    }

                    matches = matches.Multiply(subMatches);
                }

                return matches;
            }

            throw new NotSupportedJTokenTypeException(pattern);
        }

        private IEnumerable<Match> MatchSingleValue(JValue input, JValue pattern)
        {
            switch (pattern.Type)
            {
                case JTokenType.String:
                    var patternValue = pattern.Value<string>();
                    if (PlaceholderHelper.IsPlaceholder(patternValue, out var placeholder))
                    {
                        return MatchCollectionHelper.CreateWithSingleGroup(new CapturingGroup
                        {
                            Key = placeholder,
                            Value = (input as JValue).GetPrimitiveAsObject(),
                            Path = input.Path
                        });
                    }
                    else
                    {
                        return string.Equals(input.Value<string>(), PlaceholderHelper.Unescape(patternValue), this.stringComparison) ? MatchCollectionHelper.CreateWithEmptyMatch() : null;
                    }

                case JTokenType.Integer:
                    return input.Value<long>() == pattern.Value<long>() ? MatchCollectionHelper.CreateWithEmptyMatch() : null;

                case JTokenType.Boolean:
                    return input.Value<bool>() == pattern.Value<bool>() ? MatchCollectionHelper.CreateWithEmptyMatch() : null;

                default:
                    throw new NotSupportedJTokenTypeException(pattern);
            }
        }

        private IEnumerable<Match> MatchArraySingleInput(JToken input, JArray pattern)
        {
            var collections = pattern.Children()
                .Select(item => this.MatchSingleLevel(input, item))
                .Where(collection => collection != null);

            return collections.Any() ? collections.SelectMany(c => c) : null;
        }

        private IEnumerable<Match> MatchArraySinglePattern(JArray input, JToken pattern)
        {
            var collections = input.Children()
                .Select(item => this.MatchSingleLevel(item, pattern))
                .Where(collection => collection != null);

            return collections.Any() ? collections.SelectMany(c => c) : null;
        }
    }
}