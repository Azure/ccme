// -----------------------------------------------------------------------
// <copyright file="ConfigManagerFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.CCME.Assessment.Managers.ConfigProviders;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    /// <summary>
    /// Defines the configuration manager factory.
    /// </summary>
    public static class ConfigManagerFactory
    {
        /// <summary>
        /// Creates the storage account configuration manager instance.
        /// </summary>
        /// <param name="connectionString">
        /// The storage account connection string.
        /// </param>
        /// <returns>
        /// The storage account configuration manager instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the given connection string is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the given connection string is empty or white space or an
        /// invalid storage account connection string.
        /// </exception>
        public static IConfigManager CreateStorageAccountConfigManager(
            string connectionString)
        {
            var configProvider =
                new StorageAccountConfigProvider(connectionString);

            return new ConfigManager(configProvider);
        }
    }
}
