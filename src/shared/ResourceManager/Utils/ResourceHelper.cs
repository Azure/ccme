// -----------------------------------------------------------------------
// <copyright file="ResourceHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    internal sealed class ResourceHelper : IResourceHelper, IDisposable
    {
        private static readonly Regex OfficialApiVersionRegex = new Regex(@"^\d{4}-\d{2}-\d{2}$");
        private static readonly JsonSerializer IgnoreNullSerializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly IResourceManagementClient client;
        private Dictionary<string, string> apiVersionMapping;

        public ResourceHelper(IFactory factory, string subscriptionId)
        {
            this.client = factory.CreateResourceManagementClient();
            this.client.SubscriptionId = subscriptionId;
        }

        public async Task<IEnumerable<ResourceModel>> GetResourcesAsync(
            string resourceGroupName,
            IReadOnlyDictionary<string, ResourceModel> prefetchedResources,
            IEnumerable<string> detailedResourceTypes = null)
        {
            var results = new List<ResourceModel>();

            if (this.apiVersionMapping == null)
            {
                this.apiVersionMapping = await this.GetApiVersionAsync();
            }

            IEnumerable<GenericResourceInner> resources;
            try
            {
                var firstPage = await this.client.Resources.ListByResourceGroupAsync(resourceGroupName);
                resources = await firstPage.GetAllAsync(async (page) => await this.client.Resources.ListByResourceGroupNextAsync(page.NextPageLink));
            }
            catch (Exception ex)
            {
                throw new AssessmentException($"Failed to list resources in resource group {resourceGroupName} of subscription {this.client.SubscriptionId}", ex);
            }

            foreach (var resource in resources)
            {
                if (prefetchedResources.TryGetValue(resource.Id, out var prefetchedResource))
                {
                    results.Add(prefetchedResource);
                    continue;
                }

                if (detailedResourceTypes == null || detailedResourceTypes.Contains(resource.Type, StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        var resourceId = resource.Id;
                        if (resourceId.Contains("#"))
                        {
                            resourceId = resourceId.Replace("#", "%23");
                        }

                        var apiVersion = this.apiVersionMapping[resource.Type];

                        JToken details;
                        if (apiVersion == null)
                        {
                            details = JObject.FromObject(new
                            {
                                type = resource.Type,
                                location = resource.Location
                            });
                        }
                        else
                        {
                            var genericResource = await this.client.Resources.GetByIdAsync(resource.Id, apiVersion);

                            details = JObject.FromObject(genericResource, IgnoreNullSerializer);
                            details["apiVersion"] = new JValue(apiVersion);
                        }

                        results.Add(new ResourceModel
                        {
                            Id = resource.Id,
                            Details = details
                        });
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(FormattableString.Invariant($"Exception raised while getting detail of resource {resource.Id}: {ex}"));
                        results.Add(new ResourceModel
                        {
                            Id = resource.Id,
                            Exception = ex
                        });
                    }
                }
                else
                {
                    var obj = JObject.FromObject(resource, IgnoreNullSerializer);
                    results.Add(new ResourceModel
                    {
                        Id = resource.Id,
                        Details = obj
                    });
                }
            }

            return results;
        }

        private async Task<Dictionary<string, string>> GetApiVersionAsync()
        {
            IEnumerable<ProviderInner> providers;
            try
            {
                var firstPage = await this.client.Providers.ListAsync();
                providers = await firstPage.GetAllAsync(async (page) => await this.client.Providers.ListNextAsync(page.NextPageLink));
            }
            catch (Exception ex)
            {
                throw new AssessmentException($"Failed to get providers for subscription {this.client.SubscriptionId}", ex);
            }

            return providers
                .Select(provider => this.GetApiVersion(provider))
                .SelectMany(d => d)
                .ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<KeyValuePair<string, string>> GetApiVersion(ProviderInner provider)
        {
            return provider.ResourceTypes.Select(r => new KeyValuePair<string, string>(
                $"{provider.NamespaceProperty}/{r.ResourceType}",
                SelectApiVersion(r.ApiVersions)));
        }

        private static string SelectApiVersion(IEnumerable<string> apiVersions)
        {
            return apiVersions.Where(s => OfficialApiVersionRegex.IsMatch(s)).OrderByDescending(s => s).FirstOrDefault()
                ?? apiVersions.OrderByDescending(s => s).FirstOrDefault();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.client.Dispose();
                }

                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
        #endregion
    }
}