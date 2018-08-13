// -----------------------------------------------------------------------
// <copyright file="ParsedUsageReport.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Azure.CCME.Assessment.Utilities;

namespace Microsoft.Azure.CCME.Assessment.Models
{
    public class ParsedUsageReport
    {
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public IEnumerable<Meter> Meters { get; set; }

        public class Meter
        {
            public static Meter TryCreate(string[] line)
            {
                var meter = CsvFileHelper.DeserializeLine<Meter>(line);

                var resourceType = ResourceIdHelper.GetResourceType(meter.ResourceId);
                if (resourceType == null)
                {
                    meter = null;
                }
                else
                {
                    meter.ResourceType = resourceType;
                }

                return meter;
            }

            [Column(Order = 0)]
            public DateTime Date { get; private set; }

            [Column(Order = 1)]
            public string MeterCategory { get; private set; }

            [Column(Order = 2)]
            public string MeterId { get; private set; }

            [Column(Order = 3)]
            public string MeterSubCategory { get; private set; }

            [Column(Order = 4)]
            public string MeterName { get; private set; }

            [Column(Order = 6)]
            public string Unit { get; private set; }

            [Column(Order = 7)]
            public double Quantity { get; private set; }

            [Column(Order = 8)]
            public string Location { get; private set; }

            [Column(Order = 10)]
            public string ResourceGroup { get; private set; }

            [Column(Order = 11)]
            public string ResourceId { get; private set; }

            public string ResourceType { get; private set; }
        }

        public class Status
        {
            [Column(Order = 0)]
            public string SubscriptionId { get; private set; }

            [Column(Order = 1)]
            public string SubscriptionName { get; private set; }
        }
    }
}
