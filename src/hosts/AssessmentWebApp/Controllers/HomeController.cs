// -----------------------------------------------------------------------
// <copyright file="HomeController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Web;
using System.Web.Mvc;

using Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics;
using Microsoft.Azure.CCME.Assessment.Hosts.Identity;
using Microsoft.Azure.CCME.Assessment.Hosts.Tokens;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            TelemetryHelper.LogVerbose(@"HomeController::Index");
            return this.View();
        }

        /// <summary>
        /// Send an OpenID Connect sign-in request.
        /// Alternatively, you can just decorate the SignIn method with the [Authorize] attribute
        /// </summary>
        public void SignIn(string redirectUrl)
        {
            TelemetryHelper.LogVerbose(@"HomeController::SignIn");

            if (!this.Request.IsAuthenticated)
            {
                this.HttpContext.GetOwinContext().Challenge(redirectUrl);
            }
        }

        /// <summary>
        /// Send an OpenID Connect sign-out request.
        /// </summary>
        public void SignOut()
        {
            var user = this.HttpContext.GetOwinContext().GetUser();
            var tenantId = user.GetTenantId();
            var userObjectId = user.GetUserObjectId();

            var telemetryContext = new TelemetryContext
            {
                TenantId = tenantId,
                UserObjectId = userObjectId
            };

            TokenStore.Instance.RemoveTokenWrapperByUserObjectId(userObjectId);

            TelemetryHelper.LogVerbose(
                @"HomeController::SignOut",
                telemetryContext);

            TelemetryHelper.WriteEvent(
                TelemetryEventNames.AuthSignedOut,
                telemetryContext);

            this.HttpContext.GetOwinContext().Authentication.SignOut(
                    OpenIdConnectAuthenticationDefaults.AuthenticationType,
                    CookieAuthenticationDefaults.AuthenticationType);
        }

        /// <summary>
        /// Get link about privacy statement according to regions
        /// </summary>
        /// <returns>Redirect to the list price URL</returns>
        [HttpGet]
        public ActionResult PrivacyStatement()
        {
            switch (ConfigHelper.AzureEnvironmentName)
            {
                case "AzureGlobalCloud":
                    return this.Redirect("https://www.microsoft.com/en-us/TrustCenter/Privacy/default.aspx");
                case "AzureChinaCloud":
                    return this.Redirect("https://www.azure.cn/zh-cn/support/legal/privacy-statement/?l=zh-cn&amp;clcid=0x9");
                default:
                    return this.Redirect("https://www.microsoft.com/en-us/TrustCenter/Privacy/default.aspx");
            }
        }
    }
}