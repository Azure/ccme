// -----------------------------------------------------------------------
// <copyright file="AssessmentController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.CCME.Assessment.Hosts.DAL;
using Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics;
using Microsoft.Azure.CCME.Assessment.Hosts.Identity;
using Microsoft.Azure.CCME.Assessment.Hosts.Models;
using Microsoft.Azure.CCME.Assessment.Hosts.Tokens;
using Microsoft.Azure.CCME.Assessment.Managers;
using Newtonsoft.Json;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Controllers
{
    public class AssessmentController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> ListSubscription()
        {
            if (!this.Request.IsAuthenticated)
            {
                return this.View();
            }

            var owinContext = this.HttpContext.GetOwinContext();
            var user = owinContext.GetUser();
            var tenantId = user.GetTenantId();
            var userObjectId = user.GetUserObjectId();

            var telemetryContext = new TelemetryContext
            {
                TenantId = tenantId,
                UserObjectId = userObjectId
            };

            TelemetryHelper.LogVerbose(
                @"AssessmentController::ListSubscription",
                telemetryContext);

            var accessToken = user.Claims.SingleOrDefault(c => c.Type == Constants.TokenKey).Value;

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                owinContext.Challenge(
                    this.Url.Action("ListSubscription", "Assessment"));
            }

            TelemetryHelper.LogInformation(
                @"Listing subscriptions",
                telemetryContext);

            var telemetryManager = TelemetryHelper.CreateTelemetryManager(telemetryContext);
            var context = AssessmentHelper.GetEnvironmentContext(
                telemetryManager,
                accessToken,
                ConfigHelper.ResourceManagerEndpoint);

            var resourceManager = new ResourceManager(context);
            var subscriptions = await resourceManager.ListSubscriptionsAsync();
            var tasks = DataAccess.ListTasks(tenantId, userObjectId);
            var tenants = await Utilities.TenantNameHelper.ListTenants(accessToken, ConfigHelper.ResourceManagerEndpoint);

            TelemetryHelper.LogInformation(
                FormattableString.Invariant($"Got {subscriptions.Count} subscriptions"),
                telemetryContext);

            var model = new ListSubscriptionModel
            {
                TenantId = tenantId,
                Tenants = tenants,
                Subscriptions = subscriptions,
                TargetRegions = Constants.TargetRegions,
                AnyTask = tasks.Any(),
            };

            TelemetryHelper.LogVerbose(
                FormattableString.Invariant($"AssessmentController::ListSubscription::View with model: {JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })}"),
                telemetryContext);

            return this.View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public void SwitchTenant(ListSubscriptionModel model)
        {
            var owinContext = this.HttpContext.GetOwinContext();
            var user = owinContext.GetUser();
            var tenantId = user.GetTenantId();
            var userObjectId = user.GetUserObjectId();

            var telemetryContext = new TelemetryContext
            {
                TenantId = tenantId,
                UserObjectId = userObjectId
            };

            TelemetryHelper.LogVerbose(
                @"AssessmentController::SwitchTenant",
                telemetryContext);

            TelemetryHelper.WriteEvent(
                TelemetryEventNames.AuthSwitchTenant,
                telemetryContext);

            TelemetryHelper.LogInformation(
                FormattableString.Invariant($"Switch to tenant {model.TenantId}"),
                telemetryContext);

            owinContext.Challenge(
                this.Url.Action("ListSubscription", "Assessment"),
                model.TenantId);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNewTask(ListSubscriptionModel model)
        {
            var owinContext = this.HttpContext.GetOwinContext();
            var user = owinContext.GetUser();
            var tenantId = user.GetTenantId();
            var userObjectId = user.GetUserObjectId();

            var accessToken = user.Claims.SingleOrDefault(c => c.Type == Constants.TokenKey).Value;

            var telemetryContext = new TelemetryContext
            {
                TenantId = tenantId,
                UserObjectId = userObjectId
            };

            TelemetryHelper.LogVerbose(
                @"AssessmentController::CreateNewTask",
                telemetryContext);

            // TODO: check tenant id
            var taskId = DataAccess.CreateNewTask(
                tenantId,
                userObjectId,
                model.SelectedSubscriptionId,
                model.SelectedSubscriptionName,
                model.SelectedTargetRegion);

            TokenStore.Instance.AddToken(taskId, accessToken, userObjectId);

            TelemetryHelper.LogInformation(
                FormattableString.Invariant($"Created task with id {taskId} for subscription {model.SelectedSubscriptionId} and target region {model.SelectedTargetRegion}"),
                telemetryContext);

            return this.RedirectToAction("Index", "Report");
        }
    }
}