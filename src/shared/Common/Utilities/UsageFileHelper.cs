// -----------------------------------------------------------------------
// <copyright file="UsageFileHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Utilities
{
    public static class UsageFileHelper
    {
        private const int SubscriptionNameLineIdx = 2;
        private const string WrappedPattern = @"""(?<unwrapped>[^""]*)""";
        private static readonly Regex Regex = new Regex(WrappedPattern);

        public static ParsedUsageReport Parse(string content)
        {
            var lines = CsvFileHelper.Parse(content);

            string subscriptionId;
            string subscriptionName;
            IEnumerable<ParsedUsageReport.Meter> meters;

            try
            {
                var status = CsvFileHelper.DeserializeLine<ParsedUsageReport.Status>(lines.ElementAt(SubscriptionNameLineIdx));
                subscriptionId = Unwrap(status.SubscriptionId);
                subscriptionName = Unwrap(status.SubscriptionName);

                var desiredLength = lines.Last().Length;
                var meterLines = lines.Where(l => l.Length == desiredLength);
                meters = meterLines
                    .Select(line => ParsedUsageReport.Meter.TryCreate(line
                        .Select(text => Unwrap(text))
                        .ToArray()))
                    .Where(m => m != null)
                    .ToList();
            }
            catch
            {
                throw new InvalidDataException("Invalid CSV file");
            }

            return new ParsedUsageReport
            {
                SubscriptionId = subscriptionId,
                SubscriptionName = subscriptionName,
                Meters = meters
            };
        }

        private static string Unwrap(string item)
        {
            var match = Regex.Match(item);
            return match.Success ? match.Groups["unwrapped"].Value : item;
        }
    }
}