// -----------------------------------------------------------------------
// <copyright file="ResourceGraphHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Models;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    internal class ResourceGraphHelper : IResourceGraphHelper
    {
        private readonly IFactory factory;

        public ResourceGraphHelper(IFactory factory)
        {
            this.factory = factory;
        }

        public async Task<IReadOnlyDictionary<string, ResourceModel>> GetResourcesAsync(IEnumerable<string> subscriptionIds)
        {
            try
            {
                var resources = await this.GetResourcesInternalAsync(subscriptionIds.ToList());

                return resources
                    .Select(resource => new ResourceModel
                    {
                        Id = resource.Value<string>("id"),
                        Details = resource
                    })
                    .ToDictionary(
                        resource => resource.Id,
                        StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, ResourceModel>();
            }
        }

        private async Task<IEnumerable<JObject>> GetResourcesInternalAsync(IList<string> subscriptionIds)
        {
            var resources = new List<JObject>();

            using (var client = this.factory.CreateResourceGraphClient())
            {
                string skipToken = null;

                do
                {
                    var query = new QueryRequest(
                        subscriptionIds,
                        string.Empty,
                        new QueryRequestOptions
                        {
                            SkipToken = skipToken
                        });

                    var response = await client.ResourcesAsync(query);

                    resources.AddRange(
                        response.Data.Rows.Select(
                            row => new JObject(response.Data.Columns.Zip(
                                    row,
                                    (c, v) =>
                                    {
                                        if (c.Type == ColumnDataType.String && v is string text)
                                        {
                                            return new JProperty(c.Name, new JValue(text));
                                        }
                                        else if (c.Type == ColumnDataType.Object && v is string)
                                        {
                                            return new JProperty(c.Name, null);
                                        }
                                        else if (c.Type == ColumnDataType.Object && v is JObject obj)
                                        {
                                            return new JProperty(c.Name, obj.Count == 0 ? null : obj);
                                        }
                                        else
                                        {
                                            throw new InvalidCastException();
                                        }
                                    }))));

                    skipToken = response.SkipToken;
                }
                while (skipToken != null);
            }

            return resources;
        }
    }
}
