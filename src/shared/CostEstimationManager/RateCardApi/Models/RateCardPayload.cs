// -----------------------------------------------------------------------
// <copyright file="RateCardPayload.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models
{
    internal sealed class RateCardPayload
    {
        public List<object> OfferTerms { get; set; }

        public List<Meter> Meters { get; set; }

        public string Currency { get; set; }

        public string Locale { get; set; }

        public string RatingDate { get; set; }

        public bool IsTaxIncluded { get; set; }
    }
}
