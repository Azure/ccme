// -----------------------------------------------------------------------
// <copyright file="StorageAccountConfigProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using Microsoft.Azure.CCME.Assessment.Extensions;
using Microsoft.Azure.CCME.Assessment.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.CCME.Assessment.Managers.ConfigProviders
{
    /// <summary>
    /// Defines the storage account configuration provider class.
    /// </summary>
    /// <seealso cref="IConfigProvider" />
    internal sealed class StorageAccountConfigProvider : IConfigProvider
    {
        /// <summary>
        /// The settings file name
        /// </summary>
        public const string SettingsFileName = @"settings.json";

        /// <summary>
        /// The default configuration container name.
        /// </summary>
        private const string DefaultConfigContainerName = @"config";

        /// <summary>
        /// The configuration container reference.
        /// </summary>
        private readonly CloudBlobContainer configContainer;

        /// <summary>
        /// The cache dictionary, the key is blob file path.
        /// </summary>
        private readonly Dictionary<string, CachedBlob> cachedBlobsByPath;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="StorageAccountConfigProvider"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// The storage account connection string.
        /// </param>
        /// <param name="configContainerName">
        /// The configuration container name, default value is 'config'.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the given connection string or config container name is
        /// null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the given connection string or config container name is
        /// empty or white space, or the storage account connection string is
        /// invalid.
        /// </exception>
        public StorageAccountConfigProvider(
            string connectionString,
            string configContainerName = DefaultConfigContainerName)
        {
            CheckUtility.NotNullNorEmptyNorWhiteSpace(
                nameof(connectionString),
                connectionString);

            CheckUtility.NotNullNorEmptyNorWhiteSpace(
                nameof(configContainerName),
                configContainerName);

            if (!CloudStorageAccount.TryParse(connectionString, out var storageAccount))
            {
                throw new ArgumentException(
                    ExceptionMessages.InvalidStorageAccountConnectionString,
                    nameof(connectionString));
            }

            var blobClient = storageAccount.CreateCloudBlobClient();
            this.configContainer =
                blobClient.GetContainerReference(configContainerName);

            this.cachedBlobsByPath = new Dictionary<string, CachedBlob>();
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="configType">The configuration type.</param>
        /// <returns>
        /// The configuration value for specific key.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if the given configuration type is not supported.
        /// </exception>
        /// <exception cref="ConfigManagerException">
        /// Thrown if the configuration container is not existed, the global 
        /// settings file not existed, the settings value not found or the
        /// blob file not existed.
        /// </exception>
        public string GetValue(
            string key,
            ConfigType configType = ConfigType.GlobalSettings)
        {
            if (!this.configContainer.Exists())
            {
                var exceptionFormat =
                    ExceptionMessages.ConfigContainerNotExisted;

                var exceptionMessage =
                    exceptionFormat.FormatInvariant(this.configContainer.Name);

                throw GetConfigManagerExceptionInstance(exceptionMessage);
            }

            switch (configType)
            {
                case ConfigType.GlobalSettings:
                    return this.GetValueFromSettingsFile(key);
                case ConfigType.ServicParityRule:
                case ConfigType.ListPrice:
                    return this.GetBlobFileContent(key);
                default:
                    {
                        var exceptionFormat =
                            ExceptionMessages.ConfigTypeNotSupported;

                        var exceptionMessage =
                            exceptionFormat.FormatInvariant(configType);

                        throw new NotSupportedException(exceptionMessage);
                    }
            }
        }

        /// <summary>
        /// Get a <see cref="ConfigManagerException"/> instance.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <returns>
        /// Return a <see cref="ConfigManagerException"/> instance with 
        /// ConfigProviderType.
        /// </returns>
        private static ConfigManagerException GetConfigManagerExceptionInstance(
            string message)
            => new ConfigManagerException(message)
            {
                ConfigProviderType =
                        typeof(StorageAccountConfigProvider).AssemblyQualifiedName
            };

        /// <summary>
        /// Gets the blob file content.
        /// </summary>
        /// <param name="path">The blob file path.</param>
        /// <returns>The blob file content.</returns>
        /// <exception cref="ConfigManagerException">
        /// Thrown if the blob file not existed.
        /// </exception>
        private string GetBlobFileContent(string path)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(path));

            var fileBlob = this.configContainer.GetBlockBlobReference(path);

            if (!fileBlob.Exists())
            {
                var exceptionFormat = ExceptionMessages.BlobFileNotExisted;
                var exceptionMessage = exceptionFormat.FormatInvariant(path);

                throw GetConfigManagerExceptionInstance(exceptionMessage);
            }

            fileBlob.FetchAttributes();

            var remoteETag = fileBlob.Properties.ETag;

            var pathKey = path.ToLowerInvariant();

            if (this.cachedBlobsByPath.TryGetValue(pathKey, out var cachedBlob)
                && remoteETag.EqualsOic(cachedBlob.ETag))
            {
                return cachedBlob.Content;
            }

            string fileContent = null;

            var memoryStream = new MemoryStream();
            StreamReader streamReader = null;

            try
            {
                var accessCondition = new AccessCondition
                {
                    IfMatchETag = remoteETag
                };

                // TODO: handle HTTP 412 response and retry if etag changed.
                // reference: https://azure.microsoft.com/en-us/blog/managing-concurrency-in-microsoft-azure-storage-2/
                fileBlob.DownloadToStream(memoryStream, accessCondition);

                memoryStream.Position = 0;

                streamReader = new StreamReader(memoryStream, Encoding.UTF8);
                fileContent = streamReader.ReadToEnd();
            }
            finally
            {
                memoryStream.Dispose();

                if (streamReader != null)
                {
                    streamReader.Dispose();
                }
            }

            this.cachedBlobsByPath[pathKey] =
                new CachedBlob(remoteETag, fileContent);

            return fileContent;
        }

        /// <summary>
        /// Gets the configuration value for given key from settings file.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>
        /// The configuration value for specific key in settings file.
        /// </returns>
        /// <exception cref="ConfigManagerException">
        /// Thrown if the settings file is empty, or the settings value for
        /// given key not found.
        /// </exception>
        private string GetValueFromSettingsFile(string key)
        {
            string value = null;

            var settingsContent = this.GetBlobFileContent(SettingsFileName);

            if (!string.IsNullOrWhiteSpace(settingsContent))
            {
                var settingsJsonObject =
                    JsonConvert.DeserializeObject<JObject>(settingsContent);

                var token = settingsJsonObject.SelectToken(key);

                value = token?.ToString();
            }

            if (value == null)
            {
                var exceptionFormat = ExceptionMessages.SettingsValueNotFound;
                var exceptionMessage = exceptionFormat.FormatInvariant(key);
                throw GetConfigManagerExceptionInstance(exceptionMessage);
            }

            return value;
        }

        /// <summary>
        /// Defines the cached blob struct.
        /// </summary>
        private struct CachedBlob
        {
            /// <summary>
            /// The ETag value for cached blob.
            /// </summary>
            public readonly string ETag;

            /// <summary>
            /// The cached blob content.
            /// </summary>
            public readonly string Content;

            /// <summary>
            /// Initializes a new instance of the <see cref="CachedBlob"/>
            /// struct.
            /// </summary>
            /// <param name="etag">The ETag value for cached blob.</param>
            /// <param name="content">The cached blob content.</param>
            public CachedBlob(string etag, string content)
            {
                this.ETag = etag;
                this.Content = content;
            }
        }
    }
}
