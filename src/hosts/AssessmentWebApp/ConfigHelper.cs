// -----------------------------------------------------------------------
// <copyright file="ConfigHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Environments;

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    internal static class ConfigHelper
    {
        private static readonly Dictionary<string, string> Substitutes = new Dictionary<string, string>();

        public static async Task InitializeAsync()
        {
            // Load secret from local config
            var secrets = LocalConfigSecretHelper.Load();

            foreach (var pair in secrets)
            {
                Substitutes.Add(pair.Key, pair.Value);
            }

            // Initialize azure environment - only from web.config
            var azureEnvironmentHelper = new AzureEnvironmentHelper(GetConfiguration("CloudEnvironment"));
            AzureEnvironmentName = azureEnvironmentHelper.AzureEnvironmentName;
            AuthenticationEndpoint = azureEnvironmentHelper.AuthenticationEndpoint;
            ResourceManagerEndpoint = azureEnvironmentHelper.ResourceManagerEndpoint;
            StorageEndpointSuffix = azureEnvironmentHelper.StorageEndpointSuffix;

            DeploymentId = ConfigurationManager.AppSettings["DeploymentId"];
            if (DeploymentId == "local_debug")
            {
                DeploymentId += $"_{Environment.MachineName}";
            }

            // Load configuration - from local config or web.config
            KeyVaultBaseUri = GetConfiguration("KeyVaultBaseUri");
            ApplicationId = GetConfiguration("ApplicationId");
            AppCertThumbprint = GetConfiguration("AppCertThumbprint");

            // Load secret from key vault if the substitute is not in the local config
            secrets = await KeyVaultSecretHelper.LoadAsync(
                KeyVaultBaseUri,
                ApplicationId,
                AppCertThumbprint);

            foreach (var pair in secrets)
            {
                if (Substitutes.ContainsKey(pair.Key))
                {
                    Trace.TraceInformation($"Secret {pair.Key} from keyVault is ignored due to duplicated one in local config");
                    continue;
                }

                Substitutes.Add(pair.Key, pair.Value);
            }

            // Load configuration - from local config, key vault or web.config
            ApplicationSecret = GetConfiguration("ApplicationSecret");
            ReplyUri = GetConfiguration("ReplyUri");
            DatabaseConnectionString = GetConfiguration("CCMEDB");
            StorageAccountConnectionString = EnsureEndpointSuffix(GetConfiguration("StorageAccountConnectionString"));
            ConfigStorageAccountConnectionString = EnsureEndpointSuffix(GetConfiguration("ConfigStorageAccountConnectionString"));
            ApplicationInsightsKey = GetConfiguration("ApplicationInsightsKey");
        }

        public static string AzureEnvironmentName { get; private set; }

        public static string AuthenticationEndpoint { get; private set; }

        public static string ResourceManagerEndpoint { get; private set; }

        public static string StorageEndpointSuffix { get; private set; }

        public static string KeyVaultBaseUri { get; private set; }

        public static string ApplicationId { get; private set; }

        public static string AppCertThumbprint { get; private set; }

        public static string ApplicationSecret { get; private set; }

        public static string ReplyUri { get; private set; }

        public static string DatabaseConnectionString { get; private set; }

        public static string StorageAccountConnectionString { get; private set; }

        public static string ConfigStorageAccountConnectionString { get; private set; }

        public static string ApplicationInsightsKey { get; private set; }

        public static string DeploymentId { get; private set; }

        private static string GetConfiguration(string key)
        {
            if (Substitutes.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                return ConfigurationManager.AppSettings[key] ?? ConfigurationManager.ConnectionStrings[key]?.ConnectionString;
            }
        }

        private static string EnsureEndpointSuffix(string connectionString)
        {
            var parts = connectionString.Split(';');
            if (parts.Any(part => part.StartsWith("EndpointSuffix=", StringComparison.OrdinalIgnoreCase)) ||
                parts.Any(part => part.StartsWith("BlobEndpoint=", StringComparison.OrdinalIgnoreCase)))
            {
                return connectionString;
            }
            else
            {
                return $"{connectionString};EndpointSuffix={StorageEndpointSuffix}";
            }
        }
    }
}