// -----------------------------------------------------------------------
// <copyright file="AssessmentWithUsageReportModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Models
{
    public class AssessmentWithUsageReportModel
    {
        public IDictionary<string, string> TargetRegions { get; set; }

        public List<SelectListItem> TargetRegionListItems
        {
            get
            {
                return this.TargetRegions.Select(pair => new SelectListItem
                {
                    Text = pair.Value,
                    Value = pair.Key
                }).ToList();
            }
        }

        public string SelectedTargetRegion { get; set; }

        public HttpPostedFileBase UsageReportFile { get; set; }
    }
}