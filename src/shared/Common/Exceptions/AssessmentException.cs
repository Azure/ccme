// -----------------------------------------------------------------------
// <copyright file="AssessmentException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Azure.CCME.Assessment
{
    public class AssessmentException : Exception
    {
        public AssessmentException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
