// -----------------------------------------------------------------------
// <copyright file="IAssessmentContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.CCME.Assessment.Managers;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Environments
{
    public interface IAssessmentContext
    {
        ParsedUsageReport UsageReport { get; }

        IEnumerable<string> SubscriptionNames { get; }

        IEnumerable<string> SubscriptionIds { get; }

        IEnumerable<string> ResourceGroupNames { get; }

        string ARMAccessToken { get; }

        string ARMBaseUri { get; }

        string ResourceCachePath { get; }

        IConfigManager ConfigManager { get; }

        ITelemetryManager TelemetryManager { get; }
    }
}
