// -----------------------------------------------------------------------
// <copyright file="LocalConfigSecretHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    internal static class LocalConfigSecretHelper
    {
        public static Dictionary<string, string> Load()
        {
            try
            {
                var executingFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                var localConfigPath = Path.Combine(executingFolder, "../../../../local.config");

                var content = File.ReadAllText(localConfigPath);
                var configurations = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(content);
                var secrets = configurations.ToDictionary(pair => pair.Key, pair => pair.Value);

                Trace.TraceInformation(FormattableString.Invariant($"Secrets loaded from local config: {string.Join(", ", secrets.Keys)}"));
                return secrets;
            }
            catch (Exception ex)
            {
                Trace.TraceError(FormattableString.Invariant($"Failed to load secret from local config: {ex}"));
                return new Dictionary<string, string>();
            }
        }
    }
}