// -----------------------------------------------------------------------
// <copyright file="AssessmentTask.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.CCME.Assessment.Hosts.DAL.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("AssessmentTasks")]
    public class AssessmentTask : EntityBase
    {
        [Column(TypeName = "nvarchar")]
        [MaxLength(50)]
        [Required]
        public string SubscriptionId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [Required]
        public string SubscriptionName { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(50)]
        [Required]
        public string TargetRegion { get; set; }

        [Column(TypeName = "int")]
        [Required]
        public TaskStatus Status { get; set; }

        [Column(TypeName = "datetime")]
        [Required]
        public DateTime CreatedTime { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CompletedTime { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string FailedReason { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Exception { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(50)]
        public string ReportId { get; set; }

        public bool IsDone => this.Status == TaskStatus.Completed || this.Status == TaskStatus.Failed;

        public enum TaskStatus
        {
            NotStarted,
            Processing,
            Completed,
            Failed
        }
    }
}