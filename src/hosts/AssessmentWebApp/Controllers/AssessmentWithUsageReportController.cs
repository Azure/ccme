// -----------------------------------------------------------------------
// <copyright file="AssessmentWithUsageReportController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics;
using Microsoft.Azure.CCME.Assessment.Hosts.Models;
using Microsoft.Azure.CCME.Assessment.Services;
using Microsoft.Azure.CCME.Assessment.Utilities;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Controllers
{
    public class AssessmentWithUsageReportController : BaseController
    {
        public ActionResult Index()
        {
            return this.View(new AssessmentWithUsageReportModel
            {
                TargetRegions = Constants.TargetRegions
            });
        }

        [HttpPost]
        public async Task<ActionResult> Run(AssessmentWithUsageReportModel model)
        {
            try
            {
                var inputFileName = model.UsageReportFile.FileName;
                var outputFileName = $"{Path.GetFileNameWithoutExtension(inputFileName)}-AssessmentReport.pdf";

                string inputContent;
                using (var reader = new StreamReader(model.UsageReportFile.InputStream))
                {
                    inputContent = await reader.ReadToEndAsync();
                }

                var context = AssessmentHelper.GetEnvironmentContext(
                    TelemetryHelper.CreateTelemetryManager(new TelemetryContext
                    {
                        Properties = new Dictionary<string, string>
                        {
                            { "UsageReportFileName", model.UsageReportFile.FileName },
                            { "TargetRegion", model.SelectedTargetRegion }
                        }
                    }),
                    UsageFileHelper.Parse(inputContent));

                var assessmentService = new AssessmentService();
                var assessmentReport = await assessmentService.GenerateReportAsync(context, model.SelectedTargetRegion);

                var outputContent = System.IO.File.ReadAllBytes(assessmentReport.ReportFilePath);
                System.IO.File.Delete(assessmentReport.ReportFilePath);

                return this.File(outputContent, "application/pdf", outputFileName);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Assessment with usage report failed: {ex}");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}