// -----------------------------------------------------------------------
// <copyright file="StringExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;

namespace Microsoft.Azure.CCME.Assessment.Extensions
{
    /// <summary>
    /// Defines the string extension methods.
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Formats the string in culture invariant mode.
        /// </summary>
        /// <param name="format">The string format.</param>
        /// <param name="args">The string format arguments.</param>
        /// <returns>The formatted string in culture invariant mode.</returns>
        public static string FormatInvariant(
            this string format,
            params object[] args)
            => string.Format(CultureInfo.InvariantCulture, format, args);

        /// <summary>
        /// Compare strings using ordinal (binary) sort rules and ignoring the
        /// case of the strings being compared.
        /// </summary>
        /// <param name="value1">The first string to compare, or null.</param>
        /// <param name="value2">The second string to compare, or null.</param>
        /// <returns>
        /// true if the two string values are equal in ignoring case mode;
        /// otherwise, false.
        /// </returns>
        public static bool EqualsOic(this string value1, string value2)
            => string.Equals(
                value1,
                value2,
                StringComparison.OrdinalIgnoreCase);
    }
}
