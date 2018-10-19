// -----------------------------------------------------------------------
// <copyright file="MigrationController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Mvc;
using Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Controllers
{
    public class MigrationController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            TelemetryHelper.LogVerbose(@"MigrationController::Index");

            return this.View();
        }
    }
}