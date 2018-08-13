// -----------------------------------------------------------------------
// <copyright file="CsvFileHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.CCME.Assessment.Utilities
{
    public static class CsvFileHelper
    {
        private const string CsvLinePattern = @"(?<=,|^)(?:""(?<item>([^""]*(?:""""[^""]*)*))""|(?<item>[^"",]*))";
        private static readonly Regex Regex = new Regex(CsvLinePattern);

        public static IEnumerable<string[]> Parse(string content)
        {
            var lines = content.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            return lines.Select(line => ParseLine(line));
        }

        private static string[] ParseLine(string line)
        {
            return Regex.Matches(line)
                .OfType<Match>()
                .Select(m => m.Groups["item"].Value.Replace(@"""""", @""""))
                .ToArray();
        }

        public static T DeserializeLine<T>(string[] line) where T : new()
        {
            var obj = new T();

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var attribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                try
                {
                    var value = Convert.ChangeType(line[attribute.Order], propertyInfo.PropertyType);
                    propertyInfo.SetValue(obj, value);
                }
                catch
                {
                    // Nothing to do
                }
            }

            return obj;
        }
    }
}