// -----------------------------------------------------------------------
// <copyright file="AssessmentDbContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Data.Entity;

using Microsoft.Azure.CCME.Assessment.Hosts.DAL.Models;

namespace Microsoft.Azure.CCME.Assessment.Hosts.DAL
{
    internal sealed class AssessmentDbContext : DbContext
    {
        public AssessmentDbContext(string connectionString) : base(connectionString)
        {
        }

        public DbSet<UserTokenCache> UserTokenCaches { get; set; }

        public DbSet<AssessmentTask> AssessmentTasks { get; set; }

        public DbSet<AssessmentReport> AssessmentReports { get; set; }

        public sealed class Initializer : CreateDatabaseIfNotExists<AssessmentDbContext>
        {
        }
    }
}