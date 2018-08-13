// -----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.Azure.CCME.Assessment.Hosts.DAL;
using Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics;
using Microsoft.Azure.CCME.Assessment.Hosts.Tasks;

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            TelemetryHelper.WriteEvent(TelemetryEventNames.WebAppStart);
            TelemetryHelper.LogInformation("Application_Start");

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //IdentityModelEventSource.ShowPII = true;
            DataAccess.InitDatabase();
            TaskSchedulerJob.Start();
        }

        protected void Application_End()
        {
            TaskSchedulerJob.Stop();

            TelemetryHelper.WriteEvent(TelemetryEventNames.WebAppEnd);
            TelemetryHelper.LogInformation($"Application_End, shutdown reason = {HostingEnvironment.ShutdownReason}");
            TelemetryHelper.Flush();
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var error = this.Server.GetLastError();

            TelemetryHelper.LogWarning("Application_Error", error);

            if (error != null)
            {
                var errorMessage = $"{error.GetType().Name}: {error.Message}";
                Trace.TraceError($"{error.GetType().Name}: {error.Message}\n{error.StackTrace}");
                this.Response.Redirect($"?errormessage={Uri.EscapeDataString(errorMessage)}");
            }
        }
    }
}
