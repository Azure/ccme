// -----------------------------------------------------------------------
// <copyright file="ResourceIdHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.CCME.Assessment.Utilities
{
    public static class ResourceIdHelper
    {
        private const string ResourceIdPattern = @"/subscriptions/(?<subscriptionId>[^/]+)/resourceGroups/(?<resourceGroupName>[^/]+)/providers/(?<name>.+)";
        private static readonly Regex Regex = new Regex(ResourceIdPattern, RegexOptions.IgnoreCase);

        public static string GetResourceType(string resourceId)
        {
            var match = Regex.Match(resourceId);
            if (!match.Success)
            {
                return null;
            }

            var parts = match.Groups["name"].Value.Split('/');
            var typeParts = Enumerable.Range(0, parts.Length)
                .Zip(parts, (idx, part) => idx == 0 || idx % 2 != 0 ? part : null)
                .Where(s => !string.IsNullOrWhiteSpace(s));

            return string.Join("/", typeParts);
        }
    }
}