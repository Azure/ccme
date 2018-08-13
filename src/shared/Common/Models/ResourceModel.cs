// -----------------------------------------------------------------------
// <copyright file="ResourceModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Models
{
    public sealed class ResourceModel
    {
        public string Id { get; set; }
        public JToken Details { get; set; }
        public Exception Exception { get; set; }
    }
}
