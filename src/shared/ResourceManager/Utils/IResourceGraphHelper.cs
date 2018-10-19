// -----------------------------------------------------------------------
// <copyright file="IResourceGraphHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CCME.Assessment.Models;

namespace Microsoft.Azure.CCME.Assessment.Managers.Utils
{
    internal interface IResourceGraphHelper
    {
        Task<IReadOnlyDictionary<string, ResourceModel>> GetResourcesAsync(IEnumerable<string> subscriptionIds);
    }
}
