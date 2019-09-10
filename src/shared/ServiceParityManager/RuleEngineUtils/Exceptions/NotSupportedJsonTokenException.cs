// -----------------------------------------------------------------------
// <copyright file="NotSupportedJsonTokenException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.RuleEngineUtils.Exceptions
{
    internal class NotSupportedJsonTokenException : NotSupportedException
    {
        public JsonToken TokenType { get; }
        public string Path { get; }

        // ToDo: refine message
        public NotSupportedJsonTokenException(JsonReader reader)
        {
            this.TokenType = reader.TokenType;
            this.Path = reader.Path;
        }
    }
}