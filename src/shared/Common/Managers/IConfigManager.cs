// -----------------------------------------------------------------------
// <copyright file="IConfigManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    /// <summary>
    /// Defines the configuration manager interface.
    /// </summary>
    public interface IConfigManager
    {
        /// <summary>
        /// Gets the configuration value by specific key.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="configType">
        /// The configuration type, the default value is 
        /// <see cref="ConfigType.GlobalSettings">.
        /// </param>
        /// <returns>
        /// The configuration value.
        /// </returns>
        string GetValue(
            string key, 
            ConfigType configType = ConfigType.GlobalSettings);
    }
}
