// -----------------------------------------------------------------------
// <copyright file="TelemetryManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CCME.Assessment.Extensions;
using Microsoft.Azure.CCME.Assessment.Managers.TelemetryProviders;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public sealed class TelemetryManager : ITelemetryManager
    {
        private readonly ITelemetryProvider telemetryProvider;
        private readonly IDictionary<string, string> globalProperties;

        public TelemetryManager(
            ITelemetryProvider telemetryProvider,
            IDictionary<string, string> globalProperties)
        {
            this.telemetryProvider = telemetryProvider;
            this.globalProperties = globalProperties;
        }

        public void Flush()
        {
            this.telemetryProvider.Flush();
        }

        public void WriteLog(
            TelemetryLogLevel logLevel,
            string section,
            string logMessage,
            IDictionary<string, string> properties = null,
            Exception exception = null)
        {
            this.telemetryProvider.WriteLog(
                logLevel,
                string.IsNullOrWhiteSpace(section) ? logMessage : $"[{section}] {logMessage}",
                properties,
                exception);
        }

        public void WriteLog(
            TelemetryLogLevel logLevel,
            string section,
            string summary,
            IEnumerable<string> details,
            IDictionary<string, string> properties = null,
            Exception exception = null)
        {
            this.WriteLog(
                logLevel,
                section,
                $"{summary}{string.Join(string.Empty, details.Select(s => $"\r\n\t{s}"))}",
                properties,
                exception);
        }

        public void WriteMetric(
            string metricName,
            double metricValue,
            IDictionary<string, string> properties = null)
        {
            this.telemetryProvider.WriteMetric(
                metricName,
                metricValue,
                this.globalProperties.Combine(properties));
        }

        public void WriteEvent(
            string eventName,
            IDictionary<string, string> properties = null)
        {
            this.telemetryProvider.WriteEvent(
                eventName,
                this.globalProperties.Combine(properties));
        }
    }
}
