// -----------------------------------------------------------------------
// <copyright file="RateCardClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers.RateCardApi.Models;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.CCME.Assessment.Managers.RateCardApi
{
    internal sealed class RateCardClient : ServiceClient<RateCardClient>
    {
        private const string ApiVersion = @"2015-06-01-preview";
        private const string AcceptLanguage = @"en-US";
        private const string Locale = "en-US";
        private readonly string offerDurableId;
        private readonly string currency;
        private readonly string regionInfo;

        private static readonly JsonSerializerSettings DeserializationSettings =
            new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new Iso8601TimeSpanConverter(),
                    new CloudErrorJsonConverter()
                }
            };

        private readonly TokenCredentials credentials;
        private readonly Uri baseUri;

        public RateCardClient(
            string accessToken,
            string baseUri)
        {
            if (baseUri == new AzureEnvironmentHelper("AzureChinaCloud").ResourceManagerEndpoint)
            {
                this.offerDurableId = "MS-MC-AZR-0033P";
                this.currency = "CNY";
                this.regionInfo = "CN";
            }
            else
            {
                this.offerDurableId = "MS-AZR-0121P";
                this.currency = "USD";
                this.regionInfo = "US";
            }

            this.credentials = new TokenCredentials(accessToken);
            this.baseUri = new Uri(baseUri);
        }

        public async Task<RateCardPayload> GetPayloadAsync(string subscriptionId)
        {
            var payloadRequestUri = this.GetPayloadRequestUri(subscriptionId);

            return await this.GetResponseModelAsync<RateCardPayload>(payloadRequestUri);
        }

        public async Task<IEnumerable<UsageAggregate>> GetUsagesAsync(
            string subscriptionId,
            DateTime startTime,
            DateTime endTime)
        {
            var pageUri = this.GetUsagesAggregatesRequestUri(subscriptionId, startTime, endTime);

            var resultList = new List<UsageAggregate>();

            var usagePage = await this.GetResponseModelAsync<UsagePage>(pageUri);
            resultList.AddRange(usagePage.Items);

            while (usagePage.NextLink != null)
            {
                pageUri = new Uri(usagePage.NextLink);

                usagePage = await this.GetResponseModelAsync<UsagePage>(pageUri);
                resultList.AddRange(usagePage.Items);
            }

            return resultList.Skip(0);
        }

        private async Task ConfigureRequestAsync(HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.Headers.TryAddWithoutValidation(
                "x-ms-client-request-id",
                Guid.NewGuid().ToString());

            httpRequestMessage.Headers.TryAddWithoutValidation(
                "accept-language",
                AcceptLanguage);

            await this.credentials.ProcessHttpRequestAsync(
                httpRequestMessage,
                CancellationToken.None).ConfigureAwait(false);
        }

        private async Task<TModel> GetResponseModelAsync<TModel>(
            Uri requestUri)
        {
            // TODO: add telemetry
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            await this.ConfigureRequestAsync(httpRequestMessage);

            var httpResponseMessage = await this.HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            string responseContent = null;
            try
            {
                responseContent = await this.GetResponseContentAsync(
                    httpRequestMessage,
                    httpResponseMessage);

                // TODO: dump response content for unit test
                var resultModel = SafeJsonConvert.DeserializeObject<TModel>(
                    responseContent,
                    DeserializationSettings);

                return resultModel;
            }
            catch (CloudException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                throw new SerializationException(
                    @"Unable to deserialize the response.",
                    responseContent,
                    ex);
            }
            finally
            {
                httpRequestMessage.Dispose();
                httpResponseMessage.Dispose();
            }
        }

        private async Task<string> GetResponseContentAsync(
            HttpRequestMessage httpRequestMessage,
            HttpResponseMessage httpResponseMessage)
        {
            var responseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return responseContent;
            }

            CloudError cloudError = null;
            try
            {
                cloudError = SafeJsonConvert.DeserializeObject<CloudError>(
                    responseContent,
                    DeserializationSettings);
            }
            catch (JsonException)
            {
                // Ignore the exception
            }

            CloudException cloudException;
            if (cloudError != null)
            {
                cloudException = new CloudException(cloudError.Message)
                {
                    Body = cloudError
                };
            }
            else
            {
                cloudException = new CloudException(FormattableString.Invariant($"Operation returned an invalid status code '{httpResponseMessage.StatusCode}'"));
            }

            cloudException.Request = new HttpRequestMessageWrapper(httpRequestMessage, null);
            cloudException.Response = new HttpResponseMessageWrapper(
                httpResponseMessage,
                responseContent);

            if (httpResponseMessage.Headers.Contains("x-ms-request-id"))
            {
                cloudException.RequestId = httpResponseMessage.Headers.GetValues("x-ms-request-id").FirstOrDefault();
            }

            throw cloudException;
        }

        private Uri GetPayloadRequestUri(string subscriptionId)
        {
            var filterConditions = new Dictionary<string, string>
            {
                { "OfferDurableId", this.offerDurableId },
                { "Currency", this.currency },
                { "Locale", Locale },
                { "RegionInfo", this.regionInfo },
            };

            var filter = string.Join(
                " and ",
                filterConditions.Select(kvp => $"{kvp.Key} eq '{kvp.Value}'"));

            var queryParameters = new Dictionary<string, string>
            {
                { "$filter", filter },
            };

            return this.BuildCommerceRequestUri(
                subscriptionId,
                "RateCard",
                queryParameters);
        }

        private Uri GetUsagesAggregatesRequestUri(
            string subscriptionId,
            DateTime startTime,
            DateTime endTime)
        {
            var queryParameters = new Dictionary<string, string>
            {
                { "reportedStartTime", startTime.ToString("s", CultureInfo.InvariantCulture) },
                { "reportedEndTime", endTime.ToString("s", CultureInfo.InvariantCulture) },
            };

            return this.BuildCommerceRequestUri(
                subscriptionId,
                "UsageAggregates",
                queryParameters);
        }

        private Uri BuildCommerceRequestUri(
            string subscriptionId,
            string commerceResourceType,
            IDictionary<string, string> queryParameters)
        {
            var uriBuilder = new UriBuilder(this.baseUri);

            var escapedSubId = Uri.EscapeDataString(subscriptionId);
            uriBuilder.Path = $"subscriptions/{escapedSubId}/providers/Microsoft.Commerce/{commerceResourceType}";

            queryParameters["api-version"] = ApiVersion;

            uriBuilder.Query = string.Join(
                "&",
                queryParameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            return uriBuilder.Uri;
        }
    }
}