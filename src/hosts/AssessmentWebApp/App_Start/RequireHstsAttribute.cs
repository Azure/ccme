// -----------------------------------------------------------------------
// <copyright file="RequireHstsAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Web.Mvc;

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    internal class RequireHstsAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsSecureConnection)
            {
                filterContext.HttpContext.Response.AppendHeader("Strict-Transport-Security", "max-age=31536000");
            }
            else
            {
                this.HandleNonHttpsRequest(filterContext);
            }
        }
    }
}
