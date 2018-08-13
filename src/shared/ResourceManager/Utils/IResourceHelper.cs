// -----------------------------------------------------------------------
// <copyright file="IResourceHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    internal interface IResourceHelper
    {
        Task<IEnumerable<ResourceModel>> GetResourcesAsync(
            string resourceGroupName,
            IReadOnlyDictionary<string, ResourceModel> prefetchedResources,
            IEnumerable<string> detailedResourceTypes = null);
    }
}