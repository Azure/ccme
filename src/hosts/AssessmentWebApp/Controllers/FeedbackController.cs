// -----------------------------------------------------------------------
// <copyright file="FeedbackController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Mvc;
using Microsoft.Azure.CCME.Assessment.Hosts.Controllers;

namespace Microsoft.Azure.Mrm.Assessment.Hosts.Controllers
{
    public class FeedbackController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return this.View();
        }
    }
}