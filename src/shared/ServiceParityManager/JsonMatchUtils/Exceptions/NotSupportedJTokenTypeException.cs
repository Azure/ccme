// -----------------------------------------------------------------------
// <copyright file="NotSupportedJTokenTypeException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Exceptions
{
    internal class NotSupportedJTokenTypeException : NotSupportedException
    {
        public JTokenType Type { get; }
        public string Path { get; }

        // ToDo: refine message
        public NotSupportedJTokenTypeException(JToken value)
        {
            this.Type = value.Type;
            this.Path = value.Path;
        }
    }
}