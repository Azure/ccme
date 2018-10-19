// -----------------------------------------------------------------------
// <copyright file="ResourceException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Azure.CCME.Assessment.Exceptions
{
    public class ResourceException : Exception
    {
        public ResourceException(string message, Exception ex)
           : base(message, ex)
        {
        }
    }
}
