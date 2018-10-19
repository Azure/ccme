// -----------------------------------------------------------------------
// <copyright file="DataAccess.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Microsoft.Azure.CCME.Assessment.Hosts.DAL.Models;

namespace Microsoft.Azure.CCME.Assessment.Hosts.DAL
{
    internal static class DataAccess
    {
        private static readonly string DeploymentId = ConfigHelper.DeploymentId;

        public static void InitDatabase() => Database.SetInitializer(new AssessmentDbContext.Initializer());

        public static int CreateNewTask(
            string tenantId,
            string userObjectId,
            string subscriptionId,
            string subscriptionName,
            string targetRegion)
        {
            using (var db = new AssessmentDbContext(ConfigHelper.DatabaseConnectionString))
            {
                var task = new AssessmentTask
                {
                    DeploymentId = DeploymentId,
                    TenantId = tenantId,
                    UserObjectId = userObjectId,
                    SubscriptionId = subscriptionId,
                    SubscriptionName = subscriptionName,
                    TargetRegion = targetRegion,
                    Status = AssessmentTask.TaskStatus.NotStarted,
                    CreatedTime = DateTime.UtcNow
                };

                db.AssessmentTasks.Add(task);
                db.SaveChanges();

                return task.Id;
            }
        }

        public static IEnumerable<AssessmentTask> ListTasks(
            string tenantId,
            string userObjectId)
        {
            using (var db = new AssessmentDbContext(ConfigHelper.DatabaseConnectionString))
            {
                return db.AssessmentTasks.Where(
                    t => t.DeploymentId.Equals(DeploymentId)
                        && t.TenantId.Equals(tenantId)
                        && t.UserObjectId.Equals(userObjectId))
                        .ToList();
            }
        }

        public static IEnumerable<AssessmentTask> ListAllNotStartedTasks()
        {
            using (var db = new AssessmentDbContext(ConfigHelper.DatabaseConnectionString))
            {
                var tasks = db.AssessmentTasks.Where(
                    t => t.DeploymentId.Equals(DeploymentId)
                        && t.Status == AssessmentTask.TaskStatus.NotStarted)
                        .ToList();

                if (tasks.Count() < 2)
                {
                    return tasks;
                }

                var result = new List<AssessmentTask>();

                var groups = tasks.GroupBy(
                    t => $"{t.TenantId}|{t.UserObjectId}|{t.SubscriptionId}|{t.TargetRegion}");

                foreach (var group in groups)
                {
                    if (group.Count() == 1)
                    {
                        result.Add(group.First());
                        continue;
                    }

                    var orderedTasks = group.OrderByDescending(t => t.CreatedTime);

                    result.Add(orderedTasks.First());

                    foreach (var task in orderedTasks.Skip(1))
                    {
                        task.Status = AssessmentTask.TaskStatus.Failed;
                        task.CompletedTime = DateTime.UtcNow;
                        task.FailedReason = @"Abandoned.";
                        db.Entry(task).State = EntityState.Modified;
                    }
                }

                db.SaveChanges();

                return result.OrderBy(t => t.CreatedTime);
            }
        }

        public static void UpdateTaskStatusProcessing(int taskId)
        {
            using (var db = new AssessmentDbContext(ConfigHelper.DatabaseConnectionString))
            {
                var task = db.AssessmentTasks
                    .Where(t => t.Id.Equals(taskId))
                    .FirstOrDefault();

                if (task != null)
                {
                    task.Status = AssessmentTask.TaskStatus.Processing;

                    db.Entry(task).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public static void UpdateTaskStatusCompleted(
            int taskId,
            string reportId)
        {
            using (var db = new AssessmentDbContext(ConfigHelper.DatabaseConnectionString))
            {
                var task = db.AssessmentTasks
                    .Where(t => t.Id.Equals(taskId))
                    .FirstOrDefault();

                if (task != null)
                {
                    task.Status = AssessmentTask.TaskStatus.Completed;
                    task.CompletedTime = DateTime.UtcNow;
                    task.ReportId = reportId;

                    db.Entry(task).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public static void UpdateTaskStatusFailed(
            int taskId,
            string failedReason,
            Exception ex)
        {
            using (var db = new AssessmentDbContext(ConfigHelper.DatabaseConnectionString))
            {
                var task = db.AssessmentTasks
                    .Where(t => t.Id.Equals(taskId))
                    .FirstOrDefault();

                if (task != null)
                {
                    task.Status = AssessmentTask.TaskStatus.Failed;
                    task.CompletedTime = DateTime.UtcNow;
                    task.FailedReason = failedReason;
                    task.Exception = ex.ToString();

                    db.Entry(task).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public static string SaveReport(
            string tenantId,
            string userObjectId,
            string localFilePath)
        {
            var reportId = Guid.NewGuid().ToString();
            var fileName = StorageAccess.UploadFile(localFilePath);

            var report = new AssessmentReport
            {
                DeploymentId = DeploymentId,
                TenantId = tenantId,
                UserObjectId = userObjectId,
                ReportId = reportId,
                FileName = fileName
            };

            using (var db = new AssessmentDbContext(ConfigHelper.DatabaseConnectionString))
            {
                db.AssessmentReports.Add(report);
                db.SaveChanges();
            }

            return reportId;
        }

        public static string GetReportFileName(
            string tenantId,
            string userObjectId,
            string reportId)
        {
            using (var db = new AssessmentDbContext(ConfigHelper.DatabaseConnectionString))
            {
                return db.AssessmentReports
                    .Where(r => r.DeploymentId.Equals(DeploymentId)
                        && r.TenantId.Equals(tenantId)
                        && r.UserObjectId.Equals(userObjectId)
                        && r.ReportId.Equals(reportId))
                    .Select(r => r.FileName)
                    .FirstOrDefault();
            }
        }

        public static string TryRemoveAssessmentTaskById(int id)
        {
            using (var db = new AssessmentDbContext(ConfigHelper.DatabaseConnectionString))
            {
                // Try to get task
                var task = db.AssessmentTasks.Where(r => r.Id == id).FirstOrDefault();

                if (task == null)
                {
                    return null;
                }

                // Remove task
                db.AssessmentTasks.Remove(task);

                AssessmentReport report = null;

                // Report id exists
                if (!string.IsNullOrWhiteSpace(task.ReportId))
                {
                    // Try to get report
                    report = db.AssessmentReports.Where(r => r.ReportId == task.ReportId).FirstOrDefault();

                    // Report exists
                    if (report != null)
                    {
                        db.AssessmentReports.Remove(report);
                    }
                }

                db.SaveChanges();

                return report?.FileName;
            }
        }
    }
}