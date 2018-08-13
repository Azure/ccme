// -----------------------------------------------------------------------
// <copyright file="TargetListPriceProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders
{
    public static class TargetListPriceProvider
    {
        // TODO: replace list price files link
        private const string ChinaListPriceFileLink =
            @"https://globalconnectioncenter.blob.core.windows.net/pricelist/azurechinaprice.json";

        private const string GermanyListPriceFileLink =
            @"https://globalconnectioncenter.blob.core.windows.net/pricelist/azuregermanyprice.json";

        private static readonly Dictionary<string, string> ListPriceFileLinks = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "chinaeast", ChinaListPriceFileLink },
            { "chinanorth", ChinaListPriceFileLink },
            { "chinaeast2", ChinaListPriceFileLink },
            { "chinanorth2", ChinaListPriceFileLink },
            { "germanycentral", GermanyListPriceFileLink },
            { "germanynortheast", GermanyListPriceFileLink }
        };

        public static async Task<IEnumerable<ListPriceMeter>> GetMetersAsync(
            string targetRegion,
            IEnumerable<ListPriceMeter> sourceMeters)
        {
            if (!ListPriceFileLinks.TryGetValue(targetRegion, out var fileLink))
            {
                throw new NotSupportedException();
            }

            var payload = await GetPayloadFromFileAsync(fileLink);
            return payload.Meters
                .Where(m => sourceMeters.Any(src =>
                    src.MeterCategory.Equals(m.MeterCategory)
                    && src.MeterName.Equals(m.MeterName)
                    && src.MeterSubCategory.Equals(m.MeterSubCategory)))
                .Select(m => new ListPriceMeter(m)).ToList().Skip(0);
        }

        private static async Task<RateCardPayload> GetPayloadFromFileAsync(
            string fileLink)
        {
            using (var webClient = new WebClient
            {
                Encoding = Encoding.UTF8
            })
            {
                var content = await webClient.DownloadStringTaskAsync(new Uri(fileLink));
                return JsonConvert.DeserializeObject<RateCardPayload>(content);
            }
        }
    }
}