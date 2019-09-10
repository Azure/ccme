// -----------------------------------------------------------------------
// <copyright file="Meter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers.Extensions;

namespace Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models
{
    internal sealed class Meter
    {
        public string MeterId { get; set; }
        public string MeterName { get; set; }
        public string MeterCategory { get; set; }
        public string MeterSubCategory { get; set; }
        public string Unit { get; set; }
        public SortedList<double, decimal> MeterRates { get; set; }
        public string EffectiveDate { get; set; }
        public List<string> MeterTags { get; set; }
        public string MeterRegion { get; set; }
        public double IncludedQuantity { get; set; }
        public string MeterStatus { get; set; }

        public string CrossEnvironmentId => this.GetCrossEnvironmentId();
    }
}
