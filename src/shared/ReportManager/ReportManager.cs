// -----------------------------------------------------------------------
// <copyright file="ReportManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Managers.ReportGenerators;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public sealed class ReportManager : IReportManager
    {
        private readonly IAssessmentContext context;

        public ReportManager(IAssessmentContext context)
        {
            this.context = context;
        }

        public async Task<AssessmentReport> ProcessAsync(
            ServiceParityResult serviceParityResult,
            CostEstimationResult costEstimationResult)
        {
            IReportGenerator reportGenerator = new PdfReportGenerator();

            AssessmentReport report = await reportGenerator.ProcessAsync(
                this.context,
                serviceParityResult,
                costEstimationResult);

            return report;
        }
    }
}
