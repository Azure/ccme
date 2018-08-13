// -----------------------------------------------------------------------
// <copyright file="Options.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using CommandLine;

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    internal class Options
    {
        [Option('l', HelpText = "Target location name, e.g. ChinaNorth or ChinaEast2", Required = true)]
        public string TargetLocation { get; set; }

        [Option('t', HelpText = "Tenant ID", Default = "Common")]
        public string TenantId { get; set; }

        [Option('u', HelpText = "Full path of the usage report (v2) file")]
        public string UsageReportPath { get; set; }

        [Option('m', HelpText = "Source management group names")]
        public IEnumerable<string> ManagementGroupNames { get; set; }

        [Option('n', HelpText = "Source subscription names")]
        public IEnumerable<string> SubscriptionNames { get; set; }

        [Option('s', HelpText = "Source subscription IDs")]
        public IEnumerable<string> SubscriptionIds { get; set; }

        [Option('g', HelpText = "Source resource group names")]
        public IEnumerable<string> ResourceGroupNames { get; set; }

        [Option('r', HelpText = "Full path of the resources detail cache file")]
        public string ResourceCachePath { get; set; }

        [Option('e', HelpText = "Name of the azure environment, e.g. AzureGlobalCloud or AzureChinaCloud")]
        public string AzureEnvironmentName { get; set; }
    }
}
