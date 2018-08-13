// -----------------------------------------------------------------------
// <copyright file="ListPriceMeter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders
{
    public sealed class ListPriceMeter
    {
        internal ListPriceMeter(Meter meter)
        {
            this.MeterId = meter.MeterId;
            this.MeterName = meter.MeterName;
            this.MeterCategory = meter.MeterCategory;
            this.MeterSubCategory = meter.MeterSubCategory;
            this.MeterRates = meter.MeterRates;
        }

        internal ListPriceMeter(
            string meterId,
            string meterName,
            string meterCategory,
            string meterSubCategory,
            SortedList<double, decimal> meterRates)
        {
            this.MeterId = meterId;
            this.MeterName = meterName;
            this.MeterCategory = meterCategory;
            this.MeterSubCategory = meterSubCategory;
            this.MeterRates = meterRates;
        }

        public string MeterId { get; }

        public string MeterName { get; }

        public string MeterCategory { get; }

        public string MeterSubCategory { get; }

        public SortedList<double, decimal> MeterRates { get; }

        public string CrossEnvironmentId => $"{this.MeterCategory.Replace(" ", string.Empty)}.{this.MeterSubCategory.Replace(" ", string.Empty)}.{this.MeterName.Replace(" ", string.Empty)}";
    }
}
