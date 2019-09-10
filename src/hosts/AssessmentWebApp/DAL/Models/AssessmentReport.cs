// -----------------------------------------------------------------------
// <copyright file="AssessmentReport.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Hosts.DAL.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("AssessmentReports")]
    public class AssessmentReport : EntityBase
    {
        [Column(TypeName = "nvarchar")]
        [MaxLength(50)]
        [Required]
        public string ReportId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(2048)]
        [Required]
        public string FileName { get; set; }
    }
}