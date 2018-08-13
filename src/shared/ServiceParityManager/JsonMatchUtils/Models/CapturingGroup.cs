// -----------------------------------------------------------------------
// <copyright file="CapturingGroup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Models
{
    [DebuggerDisplay("{Key} = {Value}")]
    internal sealed class CapturingGroup
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string Path { get; set; }
    }
}