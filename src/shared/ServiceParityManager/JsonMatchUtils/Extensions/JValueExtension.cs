// -----------------------------------------------------------------------
// <copyright file="JValueExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Exceptions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Extensions
{
    internal static class JValueExtension
    {
        public static object GetPrimitiveAsObject(this JValue value)
        {
            switch (value.Type)
            {
                case JTokenType.String:
                    return value.Value<string>();

                case JTokenType.Integer:
                    return value.Value<long>();

                case JTokenType.Boolean:
                    return value.Value<bool>();

                default:
                    throw new NotSupportedJTokenTypeException(value);
            }
        }
    }
}