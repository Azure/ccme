// -----------------------------------------------------------------------
// <copyright file="CrossEnvironmentIdHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders;
using Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.Extensions
{
    internal static class CrossEnvironmentIdHelper
    {
        public static string GetCrossEnvironmentId(this Meter meter)
        {
            var category = meter.MeterCategory.Replace(" ", string.Empty);
            var subCategory = meter.MeterSubCategory.Replace(" ", string.Empty);
            var name = meter.MeterName.Replace(" ", string.Empty);
            var unit = meter.Unit.Replace(" ", string.Empty);

            return $"{category}/{subCategory}/{name}/{unit}";
        }

        public static string GetCrossEnvironmentId(this ListPriceMeter meter)
        {
            var category = meter.MeterCategory.Replace(" ", string.Empty);
            var subCategory = meter.MeterSubCategory.Replace(" ", string.Empty);
            var name = meter.MeterName.Replace(" ", string.Empty);
            var unit = meter.MeterUnit.Replace(" ", string.Empty);

            return $"{category}/{subCategory}/{name}/{unit}";
        }
    }
}
