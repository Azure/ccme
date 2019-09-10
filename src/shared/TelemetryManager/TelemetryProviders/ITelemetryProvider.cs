// -----------------------------------------------------------------------
// <copyright file="ITelemetryProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.CCME.Assessment.Managers.TelemetryProviders
{
    public interface ITelemetryProvider
    {
        void Flush();

        void WriteLog(
            TelemetryLogLevel logLevel,
            string logMessage,
            IDictionary<string, string> properties,
            Exception exception = null);

        void WriteMetric(
            string metricName,
            double metricValue,
            IDictionary<string, string> properties);

        void WriteEvent(
            string eventName,
            IDictionary<string, string> properties);
    }
}
