// -----------------------------------------------------------------------
// <copyright file="ResourceManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers.Utils;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public sealed class ResourceManager : IResourceManager
    {
        private const string TelemetryLogSection = "ResourceManager";

        private readonly IAssessmentContext context;

        internal IFactory Factory { get; set; }

        public ResourceManager(IAssessmentContext context)
        {
            this.context = context;
        }

        public async Task<string> GetSubscriptionNameAsync(string subscriptionId)
        {
            var clientFactory = this.Factory ?? new Factory(this.context.ARMAccessToken, this.context.ARMBaseUri);

            var subscriptionHelper = clientFactory.CreateSubscriptionHelper();

            return await subscriptionHelper.GetSubscriptionNameByIdAsync(subscriptionId);
        }

        public async Task<IDictionary<string, string>> ListSubscriptionsAsync()
        {
            var clientFactory = this.Factory ?? new Factory(this.context.ARMAccessToken, this.context.ARMBaseUri);

            var subscriptionHelper = clientFactory.CreateSubscriptionHelper();

            return await subscriptionHelper.ListSubscriptionsAsync();
        }

        public async Task<IEnumerable<SubscriptionModel>> GetResourcesAsync(IEnumerable<string> detailedResourceTypes = null)
        {
            var clientFactory = this.Factory ?? new Factory(this.context.ARMAccessToken, this.context.ARMBaseUri);

            var subscriptionHelper = clientFactory.CreateSubscriptionHelper();
            var subscriptionIds = await subscriptionHelper.GetSubscriptionIdsAsync(
                this.context.SubscriptionNames,
                this.context.SubscriptionIds);

            this.context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Information,
                TelemetryLogSection,
                FormattableString.Invariant($"Total {subscriptionIds.Count()} subscriptions found to be analyzed"));

            var resourceGraphHelper = clientFactory.CreateResourceGraphHelper();
            var prefetchedResources = await resourceGraphHelper.GetResourcesAsync(subscriptionIds);
            this.context.TelemetryManager.WriteLog(
                TelemetryLogLevel.Information,
                TelemetryLogSection,
                FormattableString.Invariant($"Total {prefetchedResources.Count()} resources prefetched from Resource Graph"));

            var resourceGroupHelper = clientFactory.CreateResourceGroupHelper();

            var models = new List<SubscriptionModel>();
            foreach (var subscriptionId in subscriptionIds)
            {
                var resourceGroups = new Dictionary<string, IEnumerable<ResourceModel>>();

                IEnumerable<string> resourceGroupNames;
                if (this.context.ResourceGroupNames != null && this.context.ResourceGroupNames.Any())
                {
                    resourceGroupNames = this.context.ResourceGroupNames;
                }
                else
                {
                    resourceGroupNames = await resourceGroupHelper.GetAllResourceGroupNamesAsync(subscriptionId);
                }

                this.context.TelemetryManager.WriteLog(
                    TelemetryLogLevel.Information,
                    TelemetryLogSection,
                    FormattableString.Invariant($"{resourceGroupNames.Count()} resource groups found in subscription `{subscriptionId}`"));

                using (var resourceHelper = clientFactory.CreateResourceHelper(subscriptionId))
                {
                    foreach (var resourceGroupName in resourceGroupNames)
                    {
                        var resources = await resourceHelper.GetResourcesAsync(
                            resourceGroupName,
                            prefetchedResources,
                            detailedResourceTypes);
                        resourceGroups.Add(resourceGroupName, resources);

                        this.context.TelemetryManager.WriteLog(
                            TelemetryLogLevel.Information,
                            TelemetryLogSection,
                            FormattableString.Invariant($"{resources.Count()} resources retrieved from resource group `{resourceGroupName}` ({resourceGroupNames.TakeWhile(s => s != resourceGroupName).Count()}/{resourceGroupNames.Count()}) of subscriptions `{subscriptionId}` ({subscriptionIds.TakeWhile(s => s != subscriptionId).Count()}/{subscriptionIds.Count()})"));
                    }
                }

                models.Add(new SubscriptionModel
                {
                    SubscriptionId = subscriptionId,
                    SubscriptionName = await subscriptionHelper.GetSubscriptionNameByIdAsync(subscriptionId),
                    ResourceGroups = resourceGroups
                });
            }

            return models;
        }
    }
}
