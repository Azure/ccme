// -----------------------------------------------------------------------
// <copyright file="NotSupportedArrayToArrayMatchException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Exceptions
{
    internal class NotSupportedArrayToArrayMatchException : NotSupportedException
    {
        public string InputPath { get; }
        public string PatternPath { get; }

        // ToDo: refine message
        public NotSupportedArrayToArrayMatchException(JToken input, JToken pattern)
        {
            this.InputPath = input.Path;
            this.PatternPath = pattern.Path;
        }
    }
}