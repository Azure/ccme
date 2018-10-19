// -----------------------------------------------------------------------
// <copyright file="TargetListPriceProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders
{
    public class TargetListPriceProvider
    {
        private const string TelemetryLogSection = "CostEstimation";

        private static readonly IReadOnlyDictionary<string, PriceListMappingItem> PriceListNameByTargetRegion = new Dictionary<string, PriceListMappingItem>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "chinaeast",
                new PriceListMappingItem
                {
                    PriceListName = "azurechinaprice.json",
                    MeterRegion = "CN East"
                }
            },
            {
                "chinanorth",
                new PriceListMappingItem
                {
                    PriceListName = "azurechinaprice.json",
                    MeterRegion = "CN North"
                }
            },
            {
                "chinaeast2",
                new PriceListMappingItem
                {
                    PriceListName = "azurechinaprice.json",
                    MeterRegion = "CN East 2"
                }
            },
            {
                "chinanorth2",
                new PriceListMappingItem
                {
                    PriceListName = "azurechinaprice.json",
                    MeterRegion = "CN North 2"
                }
            }
        };

        private readonly IAssessmentContext context;

        public TargetListPriceProvider(IAssessmentContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ListPriceMeter>> GetMetersAsync(
            string targetRegion,
            IEnumerable<ListPriceMeter> sourceMeters)
        {
            if (!PriceListNameByTargetRegion.TryGetValue(targetRegion, out var mappingItem))
            {
                throw new NotSupportedException();
            }

            var content = this.context.ConfigManager.GetValue($"priceList/{mappingItem.PriceListName}", ConfigType.ListPrice);
            var payload = JsonConvert.DeserializeObject<RateCardPayload>(content);

            var sourceMeterCrossEnvironmentIds = new HashSet<string>(sourceMeters.Select(m => m.CrossEnvironmentId));

            return await Task.FromResult(payload.Meters
                .GroupBy(m => m.CrossEnvironmentId)
                .Where(g => sourceMeterCrossEnvironmentIds.Contains(g.Key))
                .Select(g => this.SelectMeter(g, mappingItem.MeterRegion))
                .Select(m => new ListPriceMeter(m))
                .ToList());
        }

        private Meter SelectMeter(IGrouping<string, Meter> meterGroup, string desiredMeterRegion)
        {
            var meter = this.GetMeter(meterGroup, desiredMeterRegion);
            if (meter != null)
            {
                return meter;
            }

            meter = this.GetMeter(meterGroup, "CN");
            if (meter != null)
            {
                return meter;
            }

            meter = this.GetMeter(meterGroup, string.Empty);
            if (meter != null)
            {
                return meter;
            }

            return meterGroup.First();
        }

        private Meter GetMeter(IEnumerable<Meter> meters, string meterRegion)
        {
            var candidates = meters.Where(m => m.MeterRegion == meterRegion);
            switch (candidates.Count())
            {
                case 0:
                    return null;

                case 1:
                    return candidates.Single();

                default:
                    this.context.TelemetryManager.WriteLog(
                        TelemetryLogLevel.Warning,
                        TelemetryLogSection,
                        FormattableString.Invariant($"Conflict target list price meter"),
                        candidates.Select(m => FormattableString.Invariant($"{m.MeterId} @ {m.MeterRegion}: {m.MeterRates.First().Value}/{m.Unit}")));

                    return candidates.OrderByDescending(m => m.EffectiveDate).First();
            }
        }

        private class PriceListMappingItem
        {
            public string PriceListName { get; set; }
            public string MeterRegion { get; set; }
        }
    }
}