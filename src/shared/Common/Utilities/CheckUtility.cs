// -----------------------------------------------------------------------
// <copyright file="CheckUtility.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

using Microsoft.Azure.CCME.Assessment.Diagnostics;
using Microsoft.Azure.CCME.Assessment.Extensions;

namespace Microsoft.Azure.CCME.Assessment.Utilities
{
    /// <summary>
    /// Defines the parameter checking utility methods.
    /// </summary>
    public static class CheckUtility
    {
        /// <summary>
        /// Check the parameter value is not null.
        /// </summary>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the given parameter value is null.
        /// </exception>
        public static void NotNull(string paramName, object paramValue)
        {
            if (paramValue == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Check the parameter value is not null nor empty nor white space.
        /// </summary>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the given parameter value is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the given parameter value is empty or white space.
        /// </exception>
        public static void NotNullNorEmptyNorWhiteSpace(
            string paramName,
            string paramValue)
        {
            if (paramValue == null)
            {
                throw new ArgumentNullException(paramName);
            }
            else if (string.IsNullOrWhiteSpace(paramValue))
            {
                var messageFormat =
                    ExceptionMessages.ParamMustNotBeEmptyOrWhiteSpace;
                var exceptionMessage = messageFormat.FormatInvariant(paramName);
                throw new ArgumentException(exceptionMessage, paramName);
            }
        }

        /// <summary>
        /// Check the parameter value is a defined enum value.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the given parameter type is not an enum type.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the given parameter values is not a defined enum value.
        /// </exception>
        public static void IsDefinedEnumValue<TEnum>(
            string paramName,
            TEnum paramValue)
            where TEnum : struct
        {
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException(
                    ExceptionMessages.ParamTypeNotEnum);
            }

            if (!enumType.IsEnumDefined(paramValue))
            {
                var messageFormat = ExceptionMessages.ParamNotDefinedEnumValue;
                var exceptionMessage =
                    messageFormat.FormatInvariant(paramValue, paramName);
                throw new ArgumentException(exceptionMessage, paramName);
            }
        }
    }
}
