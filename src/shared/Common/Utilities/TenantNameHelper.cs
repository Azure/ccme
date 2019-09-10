// -----------------------------------------------------------------------
// <copyright file="TenantNameHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Utilities
{
    public static class TenantNameHelper
    {
        private static async Task<Dictionary<string, string>> ListTenantsForName(string accessToken, string armEndPoint)
        {
            var tenants = new Dictionary<string, string>();

            using (var client = new SubscriptionClient(
                new Uri(armEndPoint),
                new TokenCredentials(accessToken),
                new OverrideApiHandler("2017-08-01")))
            {
                var result = await client.Tenants.ListWithHttpMessagesAsync();
                var content = await result.Response.Content.ReadAsStringAsync();
                AddTenantName(tenants, content);

                while (true)
                {
                    if (result.Body.NextPageLink != null)
                    {
                        result = await client.Tenants.ListNextWithHttpMessagesAsync(result.Body.NextPageLink);
                        content = await result.Response.Content.ReadAsStringAsync();
                        AddTenantName(tenants, content);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return tenants;
        }

        private static async Task<Dictionary<string, string>> ListTenantsForId(string accessToken, string armEndPoint)
        {
            var tenants = new Dictionary<string, string>();

            using (var client = new SubscriptionClient(
                new Uri(armEndPoint),
                new TokenCredentials(accessToken)))
            {
                var result = await client.Tenants.ListAsync();

                AddTenantId(tenants, result);

                while (true)
                {
                    if (result.NextPageLink != null)
                    {
                        result = await client.Tenants.ListNextAsync(result.NextPageLink);

                        AddTenantId(tenants, result);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return tenants;
        }

        public static async Task<Dictionary<string, string>> ListTenants(string accessToken, string armEndPoint)
        {
            try
            {
                return await ListTenantsForName(accessToken, armEndPoint);
            }
            catch
            {
                // Do nothing
            }

            return await ListTenantsForId(accessToken, armEndPoint);
        }

        private static void AddTenantName(Dictionary<string, string> tenants, string content)
        {
            TenantInfoArray tenantInfo = JsonConvert.DeserializeObject<TenantInfoArray>(content);

            foreach (var t in tenantInfo.Tenants)
            {
                if (string.IsNullOrWhiteSpace(t.TenantName))
                {
                    tenants.Add(t.TenentId, t.TenentId);
                }
                else
                {
                    tenants.Add(t.TenentId, t.TenantName);
                }
            }
        }

        private static void AddTenantId(Dictionary<string, string> tenants, IPage<TenantIdDescription> content)
        {
            foreach (var t in content)
            {
                tenants.Add(t.TenantId, t.TenantId);
            }
        }
    }
}
