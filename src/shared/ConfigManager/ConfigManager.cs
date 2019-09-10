// -----------------------------------------------------------------------
// <copyright file="ConfigManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Azure.CCME.Assessment.Managers.ConfigProviders;
using Microsoft.Azure.CCME.Assessment.Utilities;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    /// <summary>
    /// Defines the configuration manager implementation.
    /// </summary>
    /// <remarks>
    /// In current version, the configuration manager only contains one
    /// provider, it could support multiple providers based on some routing
    /// policy in the future.
    /// </remarks>
    internal sealed class ConfigManager : IConfigManager
    {
        /// <summary>
        /// The internal configuration provider instance.
        /// </summary>
        private readonly IConfigProvider configProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigManager" /> class.
        /// </summary>
        /// <param name="configProvider">The configuration provider.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the given configuration provider instance is null.
        /// </exception>
        internal ConfigManager(IConfigProvider configProvider)
        {
            CheckUtility.NotNull(nameof(configProvider), configProvider);

            this.configProvider = configProvider;
        }

        /// <summary>
        /// Gets the configuration value by specific key.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>
        /// The configuration value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the given key is null.
        /// </exception>
        /// /// <exception cref="ArgumentException">
        /// Thrown if the given key is empty or white space.
        /// </exception>
        public string GetValue(
            string key, 
            ConfigType configType = ConfigType.GlobalSettings)
        {
            CheckUtility.NotNullNorEmptyNorWhiteSpace(nameof(key), key);
            CheckUtility.IsDefinedEnumValue(nameof(configType), configType);

            return this.configProvider.GetValue(key, configType);
        }
    }
}
