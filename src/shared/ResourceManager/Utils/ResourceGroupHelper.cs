// -----------------------------------------------------------------------
// <copyright file="ResourceGroupHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    internal sealed class ResourceGroupHelper : IResourceGroupHelper
    {
        private readonly IFactory factory;

        public ResourceGroupHelper(IFactory factory)
        {
            this.factory = factory;
        }

        public async Task<IEnumerable<string>> GetAllResourceGroupNamesAsync(string subscriptionId)
        {
            using (var client = this.factory.CreateResourceManagementClient())
            {
                client.SubscriptionId = subscriptionId;
                var resourceGroups = await client.ResourceGroups.ListAsync();

                var resourceGroupNames = new List<string>();
                await resourceGroups.ForEachAsync(
                    async page => await client.ResourceGroups.ListNextAsync(page.NextPageLink),
                    (resourceGroupInner) =>
                    {
                        resourceGroupNames.Add(resourceGroupInner.Name);
                        return true;
                    });

                return resourceGroupNames;
            }
        }
    }
}