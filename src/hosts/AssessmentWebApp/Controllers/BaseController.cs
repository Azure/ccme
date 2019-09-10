// -----------------------------------------------------------------------
// <copyright file="BaseController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Mvc;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {
            ViewBag.CloudEnvironment = ConfigHelper.AzureEnvironmentName;
        }
    }
}