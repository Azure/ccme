// -----------------------------------------------------------------------
// <copyright file="EntityBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Hosts.DAL.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public abstract class EntityBase
    {
        [Key]
        [Column(TypeName = "int")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(50)]
        [Required]
        public string TenantId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(50)]
        [Required]
        public string UserObjectId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(50)]
        public string DeploymentId { get; set; }
    }
}