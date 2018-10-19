// -----------------------------------------------------------------------
// <copyright file="TaskSchedulerJob.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Exceptions;
using Microsoft.Azure.CCME.Assessment.Hosts.DAL;
using Microsoft.Azure.CCME.Assessment.Hosts.DAL.Models;
using Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics;
using Microsoft.Azure.CCME.Assessment.Hosts.Tokens;
using Microsoft.Azure.CCME.Assessment.Services;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Tasks
{
    internal static class TaskSchedulerJob
    {
        private static readonly TimeSpan SleepTime = TimeSpan.FromSeconds(5);
        private static readonly CancellationTokenSource CancellationTokenSource =
            new CancellationTokenSource();

        private static readonly TaskFactory TaskFactory =
            new TaskFactory(CancellationTokenSource.Token);

        public static void Start()
        {
            TelemetryHelper.WriteEvent(TelemetryEventNames.TaskSchedulerStart);

            TaskFactory.StartNew(() =>
            {
                while (true)
                {
                    if (CancellationTokenSource.IsCancellationRequested)
                    {
                        TelemetryHelper.LogInformation(@"Break task iteration loop.");
                        break;
                    }

                    try
                    {
                        StartIteration(CancellationTokenSource.Token);
                        TelemetryHelper.WriteMetric(
                            TelemetryMetricNames.TaskSchedulerHeartBeat);
                    }
                    catch (Exception ex)
                    {
                        TelemetryHelper.LogError(
                            @"Run task iteration failed.",
                            ex);
                    }

                    Thread.Sleep(SleepTime);
                }
            });
        }

        public static void Stop()
        {
            CancellationTokenSource.Cancel();

            TelemetryHelper.WriteEvent(TelemetryEventNames.TaskSchedulerEnd);
        }

        private static void StartIteration(CancellationToken token)
        {
            var tasks = DataAccess.ListAllNotStartedTasks();

            // TODO: retry or abandon hanging tasks
            if (!tasks.Any())
            {
                return;
            }

            TelemetryHelper.WriteMetric(
                TelemetryMetricNames.PendingTasks,
                tasks.Count());

            var runner = new TaskRunner(token);

            foreach (var task in tasks)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                runner.Start(task);
            }
        }

        private class TaskRunner
        {
            private readonly CancellationToken token;

            public TaskRunner(CancellationToken token)
            {
                this.token = token;
            }

            public void Start(AssessmentTask taskDefinition)
            {
                var telemetryContext = GetTelementryContext(taskDefinition);

                TelemetryHelper.WriteEvent(
                    TelemetryEventNames.TaskStart,
                    telemetryContext);

                TaskFactory.StartNew(() =>
                    this.StartAsync(taskDefinition, telemetryContext)
                        .GetAwaiter().GetResult());
            }

            private async Task StartAsync(
                AssessmentTask taskDefinition,
                TelemetryContext telemetryContext)
            {
                TelemetryHelper.LogVerbose(@"Task starting.", telemetryContext);

                var accessToken = TokenStore.Instance.GetTokenByTaskId(taskDefinition.Id);

                if (accessToken == null)
                {
                    return;
                }

                DataAccess.UpdateTaskStatusProcessing(taskDefinition.Id);
                
                TelemetryHelper.LogInformation(@"Updated task status to processing.", telemetryContext);

                try
                {
                    await this.ProcessTaskAsync(taskDefinition, telemetryContext, accessToken);
                }
                catch (Exception ex)
                {
                    string failedReason;
                    if (ex is AssessmentException assessmentException)
                    {
                        failedReason = $"Generate assessment report failed: {ex.Message}";
                    }
                    else if (ex is ResourceException resourceException)
                    {
                        failedReason = ex.Message;
                    }
                    else
                    {
                        failedReason = "Generate assessment report failed";
                    }

                    TelemetryHelper.LogError(failedReason, ex, telemetryContext);

                    DataAccess.UpdateTaskStatusFailed(
                       taskDefinition.Id,
                       failedReason,
                       ex);

                    TokenStore.Instance.RemoveTokenWrapperByTaskId(taskDefinition.Id);

                    TelemetryHelper.LogInformation(@"Updated task status to failed.", telemetryContext);
                }
            }

            private async Task ProcessTaskAsync(
                AssessmentTask taskDefinition,
                TelemetryContext telemetryContext,
                string accessToken)
            {
                var telemetryManager = TelemetryHelper.CreateTelemetryManager(telemetryContext);

                var context = AssessmentHelper.GetEnvironmentContext(
                    telemetryManager,
                    accessToken,
                    ConfigHelper.ResourceManagerEndpoint,
                    taskDefinition.SubscriptionId);

                var assessmentService = new AssessmentService();

                var assessmentReport = await assessmentService.GenerateReportAsync(
                    context,
                    taskDefinition.TargetRegion);

                var reportId = DataAccess.SaveReport(
                    taskDefinition.TenantId,
                    taskDefinition.UserObjectId,
                    assessmentReport.ReportFilePath);
                File.Delete(assessmentReport.ReportFilePath);

                TelemetryHelper.LogInformation(
                    FormattableString.Invariant($"Saved report {reportId} to database and storage account."),
                    telemetryContext);

                DataAccess.UpdateTaskStatusCompleted(
                    taskDefinition.Id,
                    reportId);

                TokenStore.Instance.RemoveTokenWrapperByTaskId(taskDefinition.Id);

                TelemetryHelper.LogInformation(
                    @"Updated task status to completed.",
                    telemetryContext);

                TelemetryHelper.WriteEvent(
                    TelemetryEventNames.TaskEnd,
                    telemetryContext);

                TelemetryHelper.LogVerbose(@"Telemetry flush.", telemetryContext);
                TelemetryHelper.Flush();

                var flushWaitingTime = TimeSpan.FromSeconds(60);
                TelemetryHelper.LogVerbose(
                    FormattableString.Invariant($"Waiting flush for {flushWaitingTime}."),
                    telemetryContext);
                Thread.Sleep(flushWaitingTime);
            }

            private static TelemetryContext GetTelementryContext(
                AssessmentTask taskDefinition)
            {
                return new TelemetryContext
                {
                    TenantId = taskDefinition.TenantId,
                    UserObjectId = taskDefinition.UserObjectId,
                    Properties = new Dictionary<string, string>
                    {
                        { "TaskId", taskDefinition.Id.ToString(CultureInfo.InvariantCulture) },
                        { "SubscriptionId", taskDefinition.SubscriptionId },
                        { "TargetRegion", taskDefinition.TargetRegion },
                    }
                };
            }
        }
    }
}