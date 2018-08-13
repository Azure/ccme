// -----------------------------------------------------------------------
// <copyright file="IReportGenerator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.ReportGenerators
{
    public interface IReportGenerator
    {
        Task<AssessmentReport> ProcessAsync(
            IAssessmentContext context,
            ServiceParityResult serviceParityResult,
            CostEstimationResult costEstimationResult);
    }
}
