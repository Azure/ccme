// -----------------------------------------------------------------------
// <copyright file="PrimitiveValueConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Helpers
{
    internal class PrimitiveValueConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Integer:
                case JsonToken.Boolean:
                    return reader.Value;

                case JsonToken.StartArray:
                    var jsonArray = JArray.Load(reader);

                    switch (jsonArray.First.Type)
                    {
                        case JTokenType.String:
                            return jsonArray.Values<string>().ToArray();

                        case JTokenType.Integer:
                            return jsonArray.Values<long>().ToArray();

                        case JTokenType.Boolean:
                            return jsonArray.Values<bool>().ToArray();

                        default:
                            throw new NotSupportedJTokenTypeException(jsonArray.First);
                    }

                default:
                    throw new NotSupportedJsonTokenException(reader);
            }
        }

        [ExcludeFromCodeCoverage]
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}