// -----------------------------------------------------------------------
// <copyright file="IPageExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Rest.Azure;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    internal static class IPageExtension
    {
        public static async Task<T> ForEachAsync<T>(
            this IPage<T> page,
            Func<IPage<T>, Task<IPage<T>>> nextFuncAsync,
            Func<T, bool> action)
        {
            while (true)
            {
                foreach (var item in page)
                {
                    if (!action(item))
                    {
                        return item;
                    }
                }

                if (page.NextPageLink == null)
                {
                    return default(T);
                }

                page = await nextFuncAsync(page);
            }
        }

        public static async Task<IEnumerable<T>> GetAllAsync<T>(
            this IPage<T> page,
            Func<IPage<T>, Task<IPage<T>>> nextFuncAsync)
        {
            var output = new List<T>();

            while (true)
            {
                output.AddRange(page);

                if (page.NextPageLink == null)
                {
                    break;
                }

                page = await nextFuncAsync(page);
            }

            return output;
        }
    }
}