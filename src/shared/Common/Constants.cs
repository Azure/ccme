﻿// -----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.CCME.Assessment
{
    public static class Constants
    {
        public static readonly IDictionary<string, string> TargetRegions = new Dictionary<string, string>
        {
            { "chinanorth", "China North" },
            { "chinaeast", "China East" },
            { "chinanorth2", "China North 2" },
            { "chinaeast2", "China East 2" }
        };
    }
}
