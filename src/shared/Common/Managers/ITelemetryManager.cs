// -----------------------------------------------------------------------
// <copyright file="ITelemetryManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public interface ITelemetryManager
    {
        void Flush();

        void WriteLog(
            TelemetryLogLevel logLevel,
            string section,
            string logMessage,
            IDictionary<string, string> properties = null,
            Exception exception = null);

        void WriteLog(
            TelemetryLogLevel logLevel,
            string section,
            string summary,
            IEnumerable<string> details,
            IDictionary<string, string> properties = null,
            Exception exception = null);

        void WriteMetric(
            string metricName,
            double metricValue,
            IDictionary<string, string> properties = null);

        void WriteEvent(
            string eventName,
            IDictionary<string, string> properties = null);
    }
}
