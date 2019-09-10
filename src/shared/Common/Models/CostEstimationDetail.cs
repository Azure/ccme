// -----------------------------------------------------------------------
// <copyright file="CostEstimationDetail.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Models
{
    public sealed class CostEstimationDetail
    {
        public string ResourceId { get; set; }

        public ResourceModel ResourceModel { get; set; }

        public string ResourceType { get; set; }

        public string ResourceName { get; set; }

        public string ResourceGroupName { get; set; }

        public string Location { get; set; }

        public string MeterName { get; set; }

        public string MeterCategory { get; set; }

        public string MeterSubCategory { get; set; }

        public double? UsagesQuantity { get; set; }

        public string SourceMeterId { get; set; }

        public string TargetMeterId { get; set; }

        public decimal? OriginalCost { get; set; }

        public decimal? EstimatedCost { get; set; }
    }
}
