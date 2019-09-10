// -----------------------------------------------------------------------
// <copyright file="TelemetryHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Extensions;
using Microsoft.Azure.CCME.Assessment.Managers;
using Microsoft.Azure.CCME.Assessment.Managers.TelemetryProviders;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics
{
    internal static class TelemetryHelper
    {
        private static readonly ITelemetryProvider Provider;
        private static readonly ITelemetryManager TelemetryManager;

        static TelemetryHelper()
        {
            Provider = new AppInsightsTelemetryProvider(ConfigHelper.ApplicationInsightsKey);

            TelemetryManager = new TelemetryManager(
                Provider,
                CreateGlobalProperties());
        }

        public static ITelemetryManager CreateTelemetryManager(TelemetryContext context)
        {
            var globalProperties = CreateGlobalProperties();

            globalProperties.Merge(GetProperties(context));

            return new TelemetryManager(Provider, globalProperties);
        }

        /// <summary>
        /// Writes the event.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="context">The context.</param>
        /// <remarks>
        /// customEvents
        /// | extend DeploymentId = customDimensions.DeploymentId
        /// | where DeploymentId == "xxx"
        /// | project DeploymentId, timestamp, name, customDimensions
        /// | top 100 by timestamp desc
        /// </remarks>
        public static void WriteEvent(
            string eventName,
            TelemetryContext context = null)
        {
            RunTelemetryAction(() =>
            {
                TelemetryManager.WriteEvent(
                    eventName,
                    GetProperties(context));
            });
        }

        public static void WriteMetric(
            string metricName,
            double metricValue = 1,
            TelemetryContext context = null)
        {
            RunTelemetryAction(() =>
            {
                TelemetryManager.WriteMetric(
                    metricName,
                    metricValue,
                    GetProperties(context));
            });
        }

        public static void LogVerbose(
            string message,
            TelemetryContext context = null)
        {
            Log(TelemetryLogLevel.Verbose, message, context);
        }

        public static void LogInformation(
            string message,
            TelemetryContext context = null)
        {
            Log(TelemetryLogLevel.Information, message, context);
        }

        public static void LogWarning(
            string message,
            Exception exception = null,
            TelemetryContext context = null)
        {
            Log(TelemetryLogLevel.Warning, message, context, exception);
        }

        public static void LogError(
            string message,
            Exception exception = null,
            TelemetryContext context = null)
        {
            Log(TelemetryLogLevel.Error, message, context, exception);
        }

        public static void LogCritical(
            string message,
            Exception exception = null,
            TelemetryContext context = null)
        {
            Log(TelemetryLogLevel.Critical, message, context, exception);
        }

        public static void Flush() => TelemetryManager.Flush();

        private static void Log(
            TelemetryLogLevel logLevel,
            string message,
            TelemetryContext context = null,
            Exception exception = null)
        {
            RunTelemetryAction(() =>
            {
                TelemetryManager.WriteLog(
                   logLevel,
                   "WebApp",
                   message,
                   GetProperties(context),
                   exception);

                if (exception != null)
                {
                    WriteMetric(TelemetryMetricNames.ExceptionOccurred);
                }
            });
        }

        private static void RunTelemetryAction(Action action)
        {
            action.BeginInvoke(
                new AsyncCallback(
                    result => action.EndInvoke(result)),
                null);
        }

        private static IDictionary<string, string> GetProperties(
            TelemetryContext context)
        {
            if (context == null)
            {
                return null;
            }

            var properties = new Dictionary<string, string>
            {
                { "SessionId", context.SessionId },
                { "ThreadId", context.ThreadId },
                { "TenantId", context.TenantId },
                { "UserObjectId", context.UserObjectId }
            };

            properties.Merge(context.Properties);
            return properties;
        }

        private static IDictionary<string, string> CreateGlobalProperties()
        {
            return new Dictionary<string, string>
            {
                { "DeploymentId", ConfigHelper.DeploymentId }
            };
        }
    }
}