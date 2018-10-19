// -----------------------------------------------------------------------
// <copyright file="AzureEnvironmentHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace Microsoft.Azure.CCME.Assessment.Environments
{
    public class AzureEnvironmentHelper
    {
        private static readonly IReadOnlyDictionary<string, string> RegionToEnvironmentNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "China North", "AzureChinaCloud" },
            { "China East", "AzureChinaCloud" },
            { "China North 2", "AzureChinaCloud" },
            { "China East 2", "AzureChinaCloud" }
        };

        public AzureEnvironmentHelper(string azureEnvironmentName)
        {
            if (AzureEnvironment.FromName(azureEnvironmentName) == null)
            {
                var region = Environment.GetEnvironmentVariable("REGION_NAME");
                if (!string.IsNullOrWhiteSpace(region) && RegionToEnvironmentNameMapping.TryGetValue(region, out var cloudEnvironment))
                {
                    azureEnvironmentName = cloudEnvironment;
                }
                else
                {
                    azureEnvironmentName = "AzureGlobalCloud";
                }
            }

            var azureEnvironment = AzureEnvironment.FromName(azureEnvironmentName);

            this.AzureEnvironmentName = azureEnvironment.Name;
            this.AuthenticationEndpoint = azureEnvironment.AuthenticationEndpoint;
            this.ResourceManagerEndpoint = azureEnvironment.ResourceManagerEndpoint;
            this.StorageEndpointSuffix = azureEnvironment.StorageEndpointSuffix;
        }

        public string AzureEnvironmentName { get; }
        public string AuthenticationEndpoint { get; }
        public string ResourceManagerEndpoint { get; }
        public string StorageEndpointSuffix { get; }
    }
}