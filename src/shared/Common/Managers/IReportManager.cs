// -----------------------------------------------------------------------
// <copyright file="IReportManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    public interface IReportManager
    {
        Task<AssessmentReport> ProcessAsync(
            ServiceParityResult serviceParityResult,
            CostEstimationResult costEstimationResult);
    }
}
