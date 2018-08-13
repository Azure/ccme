// -----------------------------------------------------------------------
// <copyright file="ConfigHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.CCME.Assessment.Environments;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    internal static class ConfigHelper
    {
        private static readonly IReadOnlyDictionary<string, string> LocalConfig;

        static ConfigHelper()
        {
            LocalConfig = LoadLocalConfig();

            var azureEnvironmentHelper = new AzureEnvironmentHelper(ConfigurationManager.AppSettings["CloudEnvironment"]);

            AzureEnvironmentName = azureEnvironmentHelper.AzureEnvironmentName;

            ApplicationId = LookupLocalConfig(ConfigurationManager.AppSettings[@"ApplicationId"]);

            ApplicationSecret = LookupLocalConfig(ConfigurationManager.AppSettings[@"ApplicationSecret"]);

            ReplyUri = LookupLocalConfig(ConfigurationManager.AppSettings["ReplyUri"]);

            AuthenticationEndpoint = azureEnvironmentHelper.AuthenticationEndpoint;

            ResourceManagerEndpoint = azureEnvironmentHelper.ResourceManagerEndpoint;

            StorageEndpointSuffix = azureEnvironmentHelper.StorageEndpointSuffix;

            DatabaseConnectionString = LookupLocalConfig(ConfigurationManager.ConnectionStrings["CCMEDB"].ConnectionString);

            StorageAccountConnectionString = EnsureEndpointSuffix(LookupLocalConfig(ConfigurationManager.AppSettings["StorageAccountConnectionString"]));

            ConfigStorageAccountConnectionString = EnsureEndpointSuffix(LookupLocalConfig(ConfigurationManager.AppSettings["ConfigStorageAccountConnectionString"]));

            ApplicationInsightsKey = LookupLocalConfig(ConfigurationManager.AppSettings["ApplicationInsightsKey"]);

            DeploymentId = ConfigurationManager.AppSettings["DeploymentId"];
            if (DeploymentId == "local_debug")
            {
                DeploymentId += $"_{Environment.MachineName}";
            }
        }

        public static string AzureEnvironmentName { get; }

        public static string AuthenticationEndpoint { get; }

        public static string ResourceManagerEndpoint { get; }

        public static string StorageEndpointSuffix { get; }

        public static string ApplicationId { get; }

        public static string ApplicationSecret { get; }

        public static string ReplyUri { get; }

        public static string DatabaseConnectionString { get; }

        public static string StorageAccountConnectionString { get; }

        public static string ConfigStorageAccountConnectionString { get; }

        public static string ApplicationInsightsKey { get; }

        public static string DeploymentId { get; }

        private static IReadOnlyDictionary<string, string> LoadLocalConfig()
        {
            try
            {
                var executingFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                var localConfigPath = Path.Combine(executingFolder, "../../../../local.config");

                var content = File.ReadAllText(localConfigPath);
                var configurations = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(content);

                return configurations.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        private static string LookupLocalConfig(string value)
        {
            foreach (var pair in LocalConfig)
            {
                if (string.Equals(value, $"{{{pair.Key}}}"))
                {
                    return pair.Value;
                }
            }

            return value;
        }

        private static string EnsureEndpointSuffix(string connectionString)
        {
            var parts = connectionString.Split(';');
            if (parts.Any(part => part.StartsWith("EndpointSuffix=")) || parts.Any(part => part.StartsWith("BlobEndpoint=")))
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