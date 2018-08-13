// -----------------------------------------------------------------------
// <copyright file="ReportController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.CCME.Assessment.Hosts.Controllers;
using Microsoft.Azure.CCME.Assessment.Hosts.DAL;
using Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics;
using Microsoft.Azure.CCME.Assessment.Hosts.Identity;
using Microsoft.Azure.CCME.Assessment.Hosts.Models;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Views.Report
{
    [Authorize]
    public class ReportController : BaseController
    {
        public ActionResult Index()
        {
            var owinContext = this.HttpContext.GetOwinContext();
            var user = owinContext.GetUser();
            var tenantId = user.GetTenantId();
            var userObjectId = user.GetUserObjectId();

            var telemetryContext = new TelemetryContext
            {
                TenantId = tenantId,
                UserObjectId = userObjectId
            };

            TelemetryHelper.LogVerbose(
                @"ReportController::Index",
                telemetryContext);

            TelemetryHelper.LogInformation(
                @"Listing assessment tasks.",
                telemetryContext);

            var tasks = DataAccess.ListTasks(tenantId, userObjectId);

            TelemetryHelper.LogInformation(
                $"Got {tasks.Count()} assessment tasks.",
                telemetryContext);

            var model = new ReportIndexModel
            {
                AssessmentTasks = tasks
            };

            TelemetryHelper.LogVerbose(
                $"ReportController::Index::view with model: {JsonConvert.SerializeObject(model, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })}",
                telemetryContext);

            return this.View(model);
        }

        public ActionResult Download(string reportId)
        {
            var owinContext = this.HttpContext.GetOwinContext();
            var user = owinContext.GetUser();
            var tenantId = user.GetTenantId();
            var userObjectId = user.GetUserObjectId();

            var telemetryContext = new TelemetryContext
            {
                TenantId = tenantId,
                UserObjectId = userObjectId
            };

            TelemetryHelper.LogVerbose(
                @"ReportController::Download",
                telemetryContext);

            var fileName = DataAccess.GetReportFileName(tenantId, userObjectId, reportId);

            TelemetryHelper.LogInformation(
                $"Got report file name {fileName} for report {reportId}.",
                telemetryContext);

            var stream = new MemoryStream();
            StorageAccess.DownloadFile(fileName, stream);
            stream.Position = 0;

            TelemetryHelper.LogInformation(
                $"Got file stream with size {stream.Length} for report {reportId}",
                telemetryContext);

            return this.File(stream, @"application/pdf", $"{reportId}.pdf");
        }

        public ActionResult Remove(int id)
        {
            var owinContext = this.HttpContext.GetOwinContext();
            var user = owinContext.GetUser();
            var tenantId = user.GetTenantId();
            var userObjectId = user.GetUserObjectId();

            var telemetryContext = new TelemetryContext
            {
                TenantId = tenantId,
                UserObjectId = userObjectId
            };

            TelemetryHelper.LogVerbose(@"ReportController::Remove", telemetryContext);

            // Remove the info of task and report in the database
            var fileName = DataAccess.TryRemoveAssessmentTaskById(id);

            // Report file exists in the blob
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                TelemetryHelper.LogInformation(
                    $"Remove report file with name {fileName}.",
                    telemetryContext);

                // Remove file saved in the blob
                StorageAccess.RemoveFile(fileName);
            }

            TelemetryHelper.LogInformation(
                $"Report file with Id {id}, name {fileName} has been successfully removed.",
                telemetryContext);

            return this.Redirect(this.Request.UrlReferrer.ToString());
        }
    }
}