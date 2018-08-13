// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Hosts.Properties;
using Microsoft.Azure.CCME.Assessment.Hosts.Utilities;
using Microsoft.Azure.CCME.Assessment.Managers;
using Microsoft.Azure.CCME.Assessment.Managers.TelemetryProviders;
using Microsoft.Azure.CCME.Assessment.Models;
using Microsoft.Azure.CCME.Assessment.Services;
using Microsoft.Azure.CCME.Assessment.Utilities;

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(opts =>
            {
                string usageReportContent = null;
                if (opts.UsageReportPath != null)
                {
                    try
                    {
                        usageReportContent = File.ReadAllText(opts.UsageReportPath);
                    }
                    catch
                    {
                        Console.Error.WriteLine($"Failed to open usage report file");
                        return;
                    }
                }

                if (opts.ManagementGroupNames == null
                    && opts.SubscriptionIds == null
                    && opts.SubscriptionNames == null
                    && usageReportContent == null)
                {
                    Console.Error.WriteLine($"No management group or subscription specified");
                    return;
                }

                try
                {
                    RunAsync(opts, usageReportContent).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Unhandled exception: {ex}");
                }
            });
        }

        private static async Task RunAsync(Options opts, string usageReportContent)
        {
            var azureEnvironmentHelper = new AzureEnvironmentHelper(opts.AzureEnvironmentName);

            ParsedUsageReport usageReport = null;
            string accessToken = null;

            if (usageReportContent == null)
            {
                var authenticationResult = await TokenProvider.AcquireTokenAsync(
                    azureEnvironmentHelper.AuthenticationEndpoint,
                    opts.TenantId,
                    azureEnvironmentHelper.ResourceManagerEndpoint);
                accessToken = authenticationResult.AccessToken;
            }
            else
            {
                usageReport = UsageFileHelper.Parse(usageReportContent);
            }

            var context = new AssessmentContext(
                usageReport: usageReport,
                subscriptionNames: opts.SubscriptionNames,
                subscriptionIds: opts.SubscriptionIds,
                resourceGroupNames: opts.ResourceGroupNames,
                accessToken: accessToken,
                armBaseUri: azureEnvironmentHelper.ResourceManagerEndpoint,
                resourceCachePath: opts.ResourceCachePath,
                configManager: ConfigManagerFactory.CreateStorageAccountConfigManager(Settings.Default.ConnectionString),
                telemetryManager: new TelemetryManager(
                    new AppInsightsTelemetryProvider(ConfigurationManager.AppSettings["ApplicationInsightsKey"]),
                    new Dictionary<string, string>
                    {
                        { "DeploymentId", $"console_debug_{Environment.MachineName}" }
                    }));

            var assessmentService = new AssessmentService();
            var assessmentReport = await assessmentService.GenerateReportAsync(context, opts.TargetLocation);

            if (File.Exists(assessmentReport.ReportFilePath))
            {
                Process.Start(assessmentReport.ReportFilePath);
            }
        }
    }
}