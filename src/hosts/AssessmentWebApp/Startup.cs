// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics;
using Microsoft.Azure.CCME.Assessment.Hosts.Identity;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(Microsoft.Azure.CCME.Assessment.Hosts.Startup))]

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    public class Startup
    {
        private string issuerPrefix;

        public void Configuration(IAppBuilder app)
        {
            this.issuerPrefix = GetIssuerPrefixAsync().GetAwaiter().GetResult();

            app.SetDefaultSignInAsAuthenticationType(
                CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = ConfigHelper.ApplicationId,
                    Authority = $"{ConfigHelper.AuthenticationEndpoint}common",
                    RedirectUri = ConfigHelper.ReplyUri,
                    PostLogoutRedirectUri = ConfigHelper.ReplyUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = this.OnAuthenticationFailed,
                        AuthorizationCodeReceived = this.OnAuthorizationCodeReceived,
                        RedirectToIdentityProvider = this.OnRedirectToIdentityProvider,
                        SecurityTokenValidated = this.OnSecurityTokenValidated,
                    },
                    TokenValidationParameters = new IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                    }
                });
        }

        private async Task OnAuthorizationCodeReceived(
            AuthorizationCodeReceivedNotification context)
        {
            var user = new ClaimsPrincipal(context.AuthenticationTicket.Identity);

            var telemetryContext = new TelemetryContext
            {
                TenantId = user.GetTenantId(),
                UserObjectId = user.GetUserObjectId()
            };

            TelemetryHelper.LogVerbose(@"OnAuthorizationCodeReceived", telemetryContext);

            var authContext = AuthenticationContextFactory.CreateNew(user.GetTenantId(), user.GetUserObjectId());
            var result = await authContext.AcquireTokenByAuthorizationCodeAsync(
                context.Code,
                new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)),
                new ClientCredential(ConfigHelper.ApplicationId, ConfigHelper.ApplicationSecret),
                ConfigHelper.ResourceManagerEndpoint);

            TelemetryHelper.WriteEvent(
                TelemetryEventNames.AuthSignedIn,
                telemetryContext);
        }

        private Task OnAuthenticationFailed(
            AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            TelemetryHelper.LogError(@"Authentication Failed.", context.Exception);

            context.HandleResponse();
            context.Response.Redirect("/?errormessage=" + context.Exception.Message);
            return Task.FromResult(0);
        }

        private async Task OnRedirectToIdentityProvider(
            RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            var tenantId = context.OwinContext.GetTenantIdEnvValue();

            TelemetryHelper.LogVerbose(
                @"OnRedirectToIdentityProvider",
                new TelemetryContext
                {
                    TenantId = tenantId
                });

            if (tenantId != null)
            {
                var previousIssuerAddress = new Uri(context.ProtocolMessage.IssuerAddress);

                var segments = previousIssuerAddress.Segments;
                segments[1] = $"{tenantId}/";

                context.ProtocolMessage.IssuerAddress =
                    new Uri(
                        previousIssuerAddress,
                        string.Join(string.Empty, segments)).AbsoluteUri;
            }

            context.ProtocolMessage.PostLogoutRedirectUri =
                new UrlHelper(HttpContext.Current.Request.RequestContext)
                    .Action("Index", "Home", null, HttpContext.Current.Request.Url.Scheme);

            context.ProtocolMessage.Prompt = "select_account";
            context.ProtocolMessage.Resource = ConfigHelper.ResourceManagerEndpoint;

            await Task.CompletedTask;
        }

        private async Task OnSecurityTokenValidated(
            SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            var issuer = context.AuthenticationTicket.Identity.FindFirst("iss").Value;
            if (!issuer.StartsWith(issuerPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new System.IdentityModel.Tokens.SecurityTokenValidationException();
            }

            await Task.CompletedTask;
        }

        private async Task<string> GetIssuerPrefixAsync()
        {
            try
            {
                var metadataUri = new UriBuilder(ConfigHelper.AuthenticationEndpoint)
                {
                    Path = "common/FederationMetadata/2007-06/FederationMetadata.xml"
                }.Uri;

                using (var client = new HttpClient())
                {
                    var content = await client.GetStringAsync(metadataUri);

                    var document = XDocument.Parse(content);
                    var issuer = document.Root.Attribute("entityID").Value;
                    var uri = new Uri(issuer);
                    return new UriBuilder(uri.Scheme, uri.Host).ToString();
                }
            }
            catch
            {
                switch (ConfigHelper.AzureEnvironmentName)
                {
                    case "AzureChinaCloud":
                        return "https://sts.chinacloudapi.cn/";

                    case "AzureGlobalCloud":
                        return "https://sts.windows.net/";

                    default:
                        throw new ArgumentOutOfRangeException($"Unexpected azure environment name {ConfigHelper.AzureEnvironmentName}");
                }
            }
        }
    }
}
