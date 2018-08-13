// -----------------------------------------------------------------------
// <copyright file="ReportIndexModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

using Microsoft.Azure.CCME.Assessment.Hosts.DAL.Models;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Models
{
    public class ReportIndexModel
    {
        public ReportIndexModel()
        {
            this.AssessmentTasks = new AssessmentTask[0];
        }

        public IEnumerable<AssessmentTask> AssessmentTasks { get; set; }
    }
}