// -----------------------------------------------------------------------
// <copyright file="ResourceId.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    internal sealed class ResourceId : IComparable<ResourceId>
    {
        // TODO: move to resource manager
        private const char Separator = '/';
        private const int SubIdIndex = 1;
        private const int ResourceGroupIndex = 3;
        private const int ProviderIndex = 5;
        private const int ResourceTypeIndex = 6;
        private const int ResourceNameIndex = 7;
        private const int SubResourceTypeIndex = 8;
        private const int SubResourceNameIndex = 9;
        private const int SubSubResourceTypeIndex = 10;
        private const int SubSubResourceNameIndex = 11;
        private const int ResourcePartsCount = 8;
        private const int SubResourcePartsCount = 10;
        private const int SubSubResourcePartsCount = 12;

        private readonly string[] parts;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">ARM resource ID</param>
        public ResourceId(string id)
        {
            this.Id = id.TrimStart('/').ToLower(); // IMPORTANT!
            this.parts = this.Id.Split(Separator);
            if (this.parts.Length != ResourcePartsCount && this.parts.Length != SubResourcePartsCount && this.parts.Length != SubSubResourcePartsCount)
            {
                throw new ArgumentException(nameof(id));
            }
        }

        public string Id { get; }
        public string SubscriptionId => this.parts[SubIdIndex];
        public string ResourceGroup => this.parts[ResourceGroupIndex].ToLower();
        public string Provider => this.parts[ProviderIndex];
        public string ResourceType => this.parts[ResourceTypeIndex];
        public string ResourceName => this.parts[ResourceNameIndex];
        public bool IsSubResource => this.parts.Length == SubResourcePartsCount;
        public bool IsSubSubResource => this.parts.Length == SubSubResourcePartsCount;

        public string ParentId => this.IsSubResource ? string.Join(Separator.ToString(), this.parts, 0, ResourcePartsCount) : null;

        public string SubResourceName => this.IsSubResource ? this.parts[SubResourceNameIndex] : null;

        public string SubResourceType => this.IsSubResource ? this.parts[SubResourceTypeIndex] : null;

        public string SubSubResourceName => this.IsSubSubResource ? this.parts[SubSubResourceNameIndex] : null;

        public string SubSubResourceType => this.IsSubSubResource ? this.parts[SubSubResourceTypeIndex] : null;

        public int CompareTo(ResourceId other)
        {
            // Id property should already be all lower case
            return string.Compare(this.Id, other.Id, StringComparison.Ordinal);
        }
    }
}
