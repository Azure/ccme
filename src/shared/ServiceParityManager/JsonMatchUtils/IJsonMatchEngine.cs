// -----------------------------------------------------------------------
// <copyright file="IJsonMatchEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils
{
    internal interface IJsonMatchEngine
    {
        IEnumerable<Match> Matches(JToken input);
    }
}