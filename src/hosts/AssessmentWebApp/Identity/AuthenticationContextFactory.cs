// -----------------------------------------------------------------------
// <copyright file="AuthenticationContextFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Identity
{
    internal static class AuthenticationContextFactory
    {
        public static AuthenticationContext CreateNew(string tenantId, string userObjectId)
        {
            return new AuthenticationContext(
                $"{ConfigHelper.AuthenticationEndpoint}{tenantId}",
                new DbTokenCache(
                    tenantId,
                    userObjectId));
        }
    }
}