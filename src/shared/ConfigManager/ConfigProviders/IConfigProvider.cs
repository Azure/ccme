// -----------------------------------------------------------------------
// <copyright file="IConfigProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Managers.ConfigProviders
{
    /// <summary>
    /// Defines the configuration provider interface.
    /// </summary>
    internal interface IConfigProvider
    {
        /// <summary>
        /// Gets the configuration value for specific key.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="configType">
        /// The configuration type, the default value is 
        /// <see cref="ConfigType.GlobalSettings">.
        /// </param>
        /// <returns>The configuration value for given key.</returns>
        string GetValue(
            string key, 
            ConfigType configType = ConfigType.GlobalSettings);
    }
}
