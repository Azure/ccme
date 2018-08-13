// -----------------------------------------------------------------------
// <copyright file="CostEstimationResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.CCME.Assessment.Models
{
    public sealed class CostEstimationResult
    {
        public string SubscriptionName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string TargetRegion { get; set; }

        public IEnumerable<CostEstimationDetail> Details { get; set; }

        public IDictionary<string, IEnumerable<CostEstimationDetail>> DetailsByResourceGroup =>
            this.Details.GroupBy(d => d.ResourceGroupName)
            .ToDictionary(g => g.Key, g => g.OrderBy(d => d.ResourceName).Skip(0));

        public int ResourceGroupsCount =>
            this.DetailsByResourceGroup.Keys.Count;

        public int ResourcesCount => this.Details.Select(d => d.ResourceId).Distinct().Count();
    }
}
