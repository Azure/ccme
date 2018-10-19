// -----------------------------------------------------------------------
// <copyright file="AppInsightsTelemetryProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.CCME.Assessment.Extensions;

namespace Microsoft.Azure.CCME.Assessment.Managers.TelemetryProviders
{
    public sealed class AppInsightsTelemetryProvider : ITelemetryProvider
    {
        private readonly TelemetryClient telemetryClient;

        public AppInsightsTelemetryProvider(string appInsightKey)
        {
            if (appInsightKey == "-placeholder-")
            {
                TelemetryConfiguration.Active.DisableTelemetry = true;
                this.telemetryClient = null;
            }
            else
            {
                this.telemetryClient = new TelemetryClient
                {
                    InstrumentationKey = appInsightKey
                };
            }
        }

        public void Flush()
        {
            this.telemetryClient?.Flush();
        }

        public void WriteLog(
            TelemetryLogLevel logLevel,
            string logMessage,
            IDictionary<string, string> properties,
            Exception exception = null)
        {
            var msg = logMessage;

            if (exception != null)
            {
                msg += FormattableString.Invariant($"{Environment.NewLine}Exception:{Environment.NewLine}{exception.GetDetailMessage()}");
                this.telemetryClient?.TrackException(exception);
            }

            var now = DateTime.UtcNow;

            switch (logLevel)
            {
                case TelemetryLogLevel.Verbose:
                case TelemetryLogLevel.Information:
                default:
                    Trace.TraceInformation(FormattableString.Invariant($"{now}: {msg}"));
                    break;

                case TelemetryLogLevel.Warning:
                    Trace.TraceWarning(FormattableString.Invariant($"{now}: {msg}"));
                    break;

                case TelemetryLogLevel.Error:
                case TelemetryLogLevel.Critical:
                    Trace.TraceError(FormattableString.Invariant($"{now}: {msg}"));
                    break;
            }

            var traceTelemetry = new TraceTelemetry(msg, ToSeverityLevel(logLevel))
            {
                Timestamp = now,
            };

            traceTelemetry.Properties.Merge(properties);

            this.telemetryClient?.TrackTrace(traceTelemetry);
        }

        public void WriteMetric(
            string metricName,
            double metricValue,
            IDictionary<string, string> properties)
        {
            var metricTelemetry = new MetricTelemetry(metricName, metricValue)
            {
                Timestamp = DateTimeOffset.UtcNow,
            };

            metricTelemetry.Properties.Merge(properties);

            Trace.TraceInformation(FormattableString.Invariant($"Write metric {metricName} with value {metricValue}"));
            this.telemetryClient?.TrackMetric(metricTelemetry);
        }

        public void WriteEvent(
            string eventName,
            IDictionary<string, string> properties)
        {
            var eventTelemetry = new EventTelemetry(eventName)
            {
                Timestamp = DateTimeOffset.UtcNow,
            };

            eventTelemetry.Properties.Merge(properties);

            Trace.TraceInformation(FormattableString.Invariant($"Write event {eventName}"));
            this.telemetryClient?.TrackEvent(eventTelemetry);
        }

        private static SeverityLevel ToSeverityLevel(TelemetryLogLevel logLevel)
        {
            switch (logLevel)
            {
                case TelemetryLogLevel.Verbose:
                default:
                    return SeverityLevel.Verbose;

                case TelemetryLogLevel.Information:
                    return SeverityLevel.Information;

                case TelemetryLogLevel.Warning:
                    return SeverityLevel.Warning;

                case TelemetryLogLevel.Error:
                    return SeverityLevel.Error;

                case TelemetryLogLevel.Critical:
                    return SeverityLevel.Critical;
            }
        }
    }
}
