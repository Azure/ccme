// -----------------------------------------------------------------------
// <copyright file="Factory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Rest;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    [ExcludeFromCodeCoverage]
    internal sealed class Factory : IFactory
    {
        private readonly TokenCredentials credentials;
        private readonly Uri baseUri;

        public Factory(string accessToken, string baseUri)
        {
            this.credentials = new TokenCredentials(accessToken);
            this.baseUri = new Uri(baseUri);
        }

        public ISubscriptionClient CreateSubscriptionClient()
        {
            return new SubscriptionClient(this.baseUri, this.credentials);
        }

        public IResourceManagementClient CreateResourceManagementClient()
        {
            return new ResourceManagementClient(this.baseUri, this.credentials);
        }

        public ISubscriptionHelper CreateSubscriptionHelper()
        {
            return new SubscriptionHelper(this);
        }

        public IResourceGroupHelper CreateResourceGroupHelper()
        {
            return new ResourceGroupHelper(this);
        }

        public IResourceHelper CreateResourceHelper(string subscriptionId)
        {
            return new ResourceHelper(this, subscriptionId);
        }
    }
}