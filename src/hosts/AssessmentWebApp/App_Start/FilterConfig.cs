// -----------------------------------------------------------------------
// <copyright file="FilterConfig.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Web.Mvc;

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    internal static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            var replyUriScheme = new Uri(ConfigHelper.ReplyUri).Scheme;
            if (string.Equals(replyUriScheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                filters.Add(new RequireHstsAttribute());
            }
        }
    }
}
