// -----------------------------------------------------------------------
// <copyright file="TelemetryContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Web;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Diagnostics
{
    internal sealed class TelemetryContext
    {
        public string TenantId { get; set; }

        public string UserObjectId { get; set; }

        public string SessionId => HttpContext.Current?.Session?.SessionID;

        public string ThreadId => Thread.CurrentThread.ManagedThreadId.ToString();

        public IDictionary<string, string> Properties { get; set; }
    }
}