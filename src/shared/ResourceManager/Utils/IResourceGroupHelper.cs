// -----------------------------------------------------------------------
// <copyright file="IResourceGroupHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    internal interface IResourceGroupHelper
    {
        Task<IEnumerable<string>> GetAllResourceGroupNamesAsync(string subscriptionId);
    }
}