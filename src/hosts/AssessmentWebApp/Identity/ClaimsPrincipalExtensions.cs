﻿// -----------------------------------------------------------------------
// <copyright file="ClaimsPrincipalExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using System.Security.Claims;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Identity
{
    internal static class ClaimsPrincipalExtensions
    {
        public const string UserObjectIdClaimName =
           @"http://schemas.microsoft.com/identity/claims/objectidentifier";

        public const string TenantIdClaimName =
            @"http://schemas.microsoft.com/identity/claims/tenantid";

        public static string GetUserObjectId(this ClaimsPrincipal user)
            => user.FindFirst(UserObjectIdClaimName).Value;

        public static string GetTenantId(this ClaimsPrincipal user)
            => user.FindFirst(TenantIdClaimName).Value;

        public static string GetAccessToken(this ClaimsPrincipal user)
        {
            var authContext = AuthenticationContextFactory.CreateNew(
                user.GetTenantId(),
                user.GetUserObjectId());

            return authContext.TokenCache.ReadItems()
                .Where(c => c.Resource == ConfigHelper.ResourceManagerEndpoint)
                .Select(t => t.AccessToken)
                .FirstOrDefault();
        }
    }
}