// -----------------------------------------------------------------------
// <copyright file="ListPriceMeter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers.Extensions;
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
            this.MeterUnit = meter.Unit;
            this.MeterRates = meter.MeterRates;
        }

        internal ListPriceMeter(
            string meterId,
            string meterName,
            string meterCategory,
            string meterSubCategory,
            string meterUnit,
            SortedList<double, decimal> meterRates)
        {
            this.MeterId = meterId;
            this.MeterName = meterName;
            this.MeterCategory = meterCategory;
            this.MeterSubCategory = meterSubCategory;
            this.MeterUnit = meterUnit;
            this.MeterRates = meterRates;
        }

        public string MeterId { get; }

        public string MeterName { get; }

        public string MeterCategory { get; }

        public string MeterSubCategory { get; }

        public string MeterUnit { get; }

        public SortedList<double, decimal> MeterRates { get; }

        public string CrossEnvironmentId => this.GetCrossEnvironmentId();
    }
}
