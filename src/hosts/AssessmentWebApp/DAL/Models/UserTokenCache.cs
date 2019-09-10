// -----------------------------------------------------------------------
// <copyright file="UserTokenCache.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Hosts.DAL.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("UserTokenCaches")]
    internal class UserTokenCache : EntityBase
    {
        [Column(TypeName = "varbinary(max)")]
        [Required]
        public byte[] CacheBytes { get; set; }

        [Column(TypeName = "datetime")]
        [Required]
        public DateTime LastModifedTime { get; set; }
    }
}