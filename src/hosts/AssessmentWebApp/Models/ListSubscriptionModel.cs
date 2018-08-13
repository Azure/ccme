// -----------------------------------------------------------------------
// <copyright file="ListSubscriptionModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.Mvc;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Models
{
    public class ListSubscriptionModel
    {
        public string TenantId { get; set; }

        public IDictionary<string, string> Subscriptions { get; set; }

        public List<SelectListItem> SubscriptionListItems =>
            ToListItems(this.Subscriptions);

        public string SelectedSubscriptionId { get; set; }

        public IDictionary<string, string> TargetRegions { get; set; }

        public List<SelectListItem> TargetRegionListItems =>
            ToListItems(this.TargetRegions);

        public string SelectedTargetRegion { get; set; }

        private static List<SelectListItem> ToListItems(
            IDictionary<string, string> dic)
        {
            var listItems = new List<SelectListItem>();

            if (dic != null)
            {
                foreach (var kvp in dic)
                {
                    listItems.Add(new SelectListItem
                    {
                        Text = kvp.Value,
                        Value = kvp.Key
                    });
                }
            }

            return listItems;
        }

        public bool AnyTask { get; set; }
    }
}