// -----------------------------------------------------------------------
// <copyright file="TelemetryEventNames.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics
{
    internal static class TelemetryEventNames
    {
        public const string WebAppStart = "WebApp_Start";

        public const string WebAppEnd = "WebApp_End";

        public const string AuthChallenge = "Auth_Challenge";

        public const string AuthSwitchTenant = "Auth_SwitchTenant";

        public const string AuthSignedIn = "Auth_SignedIn";

        public const string AuthSignedOut = "Auth_SignedOut";

        public const string TaskSchedulerStart = "TaskScheduler_Start";

        public const string TaskSchedulerEnd = "TaskScheduler_End";

        public const string TaskStart = "Task_Start";

        public const string TaskEnd = "Task_End";
    }
}