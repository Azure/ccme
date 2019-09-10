// -----------------------------------------------------------------------
// <copyright file="TokenProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Utilities
{
    internal static class TokenProvider
    {
        private const string ClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
        private const string RedirectUrl = "urn:ietf:wg:oauth:2.0:oob";

        public static Task<AuthenticationResult> AcquireTokenAsync(
            string authenticationEndpoint,
            string tenantId,
            string tokenResource)
        {
            var context = new AuthenticationContext($"{authenticationEndpoint}{tenantId}");

            return context.AcquireTokenAsync(
                tokenResource,
                ClientId,
                new Uri(RedirectUrl),
                new PlatformParameters(PromptBehavior.Auto));
        }
    }
}