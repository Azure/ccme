// -----------------------------------------------------------------------
// <copyright file="ObjectEqualityComparer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Azure.CCME.Assessment.Managers.JsonMatchUtils.Helpers
{
    internal static class ObjectEqualityComparer
    {
        public static bool Equals(object expected, object captured, bool ignoreCase)
        {
            if (expected is string && captured is string)
            {
                return (expected as string) == "*" ||
                    string.Equals(
                    expected as string,
                    captured as string,
                    ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }
            else
            {
                return object.Equals(expected, captured);
            }
        }
    }
}