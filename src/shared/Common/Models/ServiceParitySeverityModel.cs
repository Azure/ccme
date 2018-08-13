// -----------------------------------------------------------------------
// <copyright file="ServiceParitySeverityModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.CCME.Assessment.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceParitySeverityModel
    {
        Unknown,
        Information,
        Warning,
        Error,
        Critical
    }
}