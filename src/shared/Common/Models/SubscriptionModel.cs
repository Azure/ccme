// -----------------------------------------------------------------------
// <copyright file="SubscriptionModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.CCME.Assessment.Models
{
    public sealed class SubscriptionModel
    {
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public IReadOnlyDictionary<string, IEnumerable<ResourceModel>> ResourceGroups { get; set; }
    }
}