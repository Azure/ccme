// -----------------------------------------------------------------------
// <copyright file="IAssessmentService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Environments;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Services
{
    public interface IAssessmentService
    {
        Task<AssessmentReport> GenerateReportAsync(
            IAssessmentContext context,
            string targetRegion);
    }
}
