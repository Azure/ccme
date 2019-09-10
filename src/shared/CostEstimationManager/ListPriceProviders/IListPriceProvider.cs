// -----------------------------------------------------------------------
// <copyright file="IListPriceProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.CCME.Assessment.Managers.ListPriceProviders
{
    public interface IListPriceProvider
    {
        Task<IEnumerable<ListPriceMeter>> GetMetersAsync(
            IEnumerable<string> subscriptionIds,
            ISet<string> meterIds);
    }
}
