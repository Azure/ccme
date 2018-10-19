// -----------------------------------------------------------------------
// <copyright file="OwinContextExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Security.Claims;
using Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Identity
{
    internal static class OwinContextExtensions
    {
        private const string TenantIdEnvKey = @"tenantId";

        public static ClaimsPrincipal GetUser(this IOwinContext context)
             => context.Authentication.User;

        public static void Challenge(
            this IOwinContext context,
            string redirectUrl,
            string tenantId = null)
        {
            if (tenantId != null)
            {
                context.Environment.Add(TenantIdEnvKey, tenantId);
            }

            TelemetryHelper.WriteEvent(
                TelemetryEventNames.AuthChallenge,
                new TelemetryContext { TenantId = tenantId });

            context.Authentication.Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public static string GetTenantIdEnvValue(this IOwinContext context)
        {
            string tenantId = null;

            if (context.Environment.TryGetValue(TenantIdEnvKey, out object obj))
            {
                tenantId = Convert.ToString(obj, CultureInfo.InvariantCulture);
            }

            return tenantId;
        }
    }
}