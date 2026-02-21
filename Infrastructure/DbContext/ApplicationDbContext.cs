using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DbContext
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ======================== DbSets ========================

        // Waste Management
        public DbSet<WasteReport> WasteReports { get; set; }
        public DbSet<WasteReportWaste> WasteReportWastes { get; set; }
        public DbSet<WasteType> WasteTypes { get; set; }

        // Collection & Points
        public DbSet<CitizenPoint> CitizenPoints { get; set; }
        public DbSet<CollectionProof> CollectionProofs { get; set; }
        public DbSet<CollectionRequest> CollectionRequests { get; set; }
        public DbSet<CollectorAssignment> CollectorAssignments { get; set; }
        public DbSet<CollectorProfile> CollectorProfiles { get; set; }
        public DbSet<PointRule> PointRules { get; set; }

        // Enterprise & Services
        public DbSet<RecyclingEnterprise> RecyclingEnterprises { get; set; }
        public DbSet<EnterpriseServiceArea> EnterpriseServiceAreas { get; set; }
        public DbSet<EnterpriseWasteCapability> EnterpriseWasteCapabilities { get; set; }

        // Statistics & Complaints
        public DbSet<RecyclingStatistic> RecyclingStatistics { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<DisputeResolution> DisputeResolutions { get; set; }

        // Audit
        public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }

        // ======================== Model Configuration ========================

        //DBSet
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Đổi tên bảng Identity
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            // ======================== WasteReport Configuration ========================

            builder.Entity<WasteReport>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Latitude)
                    .HasPrecision(10, 7);

                entity.Property(e => e.Longitude)
                    .HasPrecision(10, 7);

                entity.HasOne(e => e.Citizen)
                    .WithMany()
                    .HasForeignKey(e => e.CitizenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Wastes)
                    .WithOne(w => w.WasteReport)
                    .HasForeignKey(w => w.WasteReportId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================== WasteReportWaste Configuration ========================

            builder.Entity<WasteReportWaste>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Lưu ImageUrls dạng CSV
                entity.Property(e => e.ImageUrls)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    );

                entity.HasOne(e => e.WasteType)
                    .WithMany()
                    .HasForeignKey(e => e.WasteTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.WasteReport)
                    .WithMany(r => r.Wastes)
                    .HasForeignKey(e => e.WasteReportId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================== WasteType Configuration ========================

            builder.Entity<WasteType>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.BusinessCode)
                    .IsUnique();

                entity.Property(e => e.Name)
                    .HasConversion<string>();
            });

            // ======================== CitizenPoint Configuration ========================

            builder.Entity<CitizenPoint>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Citizen)
                    .WithMany()
                    .HasForeignKey(e => e.CitizenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Report)
                    .WithMany()
                    .HasForeignKey(e => e.ReportId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================== CollectionRequest Configuration ========================

            builder.Entity<CollectionRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Report)
                    .WithMany()
                    .HasForeignKey(e => e.ReportId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Enterprise)
                    .WithMany()
                    .HasForeignKey(e => e.EnterpriseId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Citizen và WasteType không có FK, chỉ là navigation
                entity.Ignore(e => e.Citizen);
                entity.Ignore(e => e.WasteType);
            });

            // ======================== CollectorAssignment Configuration ========================

            builder.Entity<CollectorAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Request)
                    .WithMany()
                    .HasForeignKey(e => e.RequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Collector)
                    .WithMany()
                    .HasForeignKey(e => e.CollectorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // CollectionRequest là duplicate, ignore
                entity.Ignore(e => e.CollectionRequest);
            });

            // ======================== CollectorProfile Configuration ========================

            builder.Entity<CollectorProfile>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Collector)
                    .WithMany()
                    .HasForeignKey(e => e.CollectorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Enterprise)
                    .WithMany()
                    .HasForeignKey(e => e.EnterpriseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================== CollectionProof Configuration ========================

            builder.Entity<CollectionProof>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Assignment)
                    .WithMany()
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // CollectionRequest không có FK, ignore
                entity.Ignore(e => e.CollectionRequest);
            });

            // ======================== PointRule Configuration ========================

            builder.Entity<PointRule>(entity =>
            {
                entity.HasKey(e => e.Id);

                // RuleId là Id rồi, không cần property riêng
                entity.Ignore(e => e.RuleId);

                entity.HasOne(e => e.Enterprise)
                    .WithMany()
                    .HasForeignKey(e => e.EnterpriseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.WasteType)
                    .WithMany()
                    .HasForeignKey(e => e.WasteTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================== RecyclingEnterprise Configuration ========================

            builder.Entity<RecyclingEnterprise>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Representative không có FK, ignore
                entity.Ignore(e => e.Representative);
            });

            // ======================== EnterpriseServiceArea Configuration ========================

            builder.Entity<EnterpriseServiceArea>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Enterprise)
                    .WithMany()
                    .HasForeignKey(e => e.EnterpriseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================== EnterpriseWasteCapability Configuration ========================

            builder.Entity<EnterpriseWasteCapability>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DailyCapacityKg)
    .HasPrecision(18, 2);
                entity.HasOne(e => e.Enterprise)
                    .WithMany()
                    .HasForeignKey(e => e.EnterpriseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.WasteType)
                    .WithMany()
                    .HasForeignKey(e => e.WasteTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================== RecyclingStatistic Configuration ========================

            builder.Entity<RecyclingStatistic>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Enterprise)
                    .WithMany()
                    .HasForeignKey(e => e.EnterpriseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.WasteType)
                    .WithMany()
                    .HasForeignKey(e => e.WasteTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================== Complaint Configuration ========================

            builder.Entity<Complaint>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Citizen)
                    .WithMany()
                    .HasForeignKey(e => e.CitizenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Report)
                    .WithMany()
                    .HasForeignKey(e => e.ReportId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Resolutions)
                    .WithOne(r => r.Complaint)
                    .HasForeignKey(r => r.ComplaintId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Complainant và CollectionRequest là duplicate, ignore
                entity.Ignore(e => e.Complainant);
                entity.Ignore(e => e.CollectionRequest);
            });

            // ======================== DisputeResolution Configuration ========================

            builder.Entity<DisputeResolution>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Complaint)
                    .WithMany(c => c.Resolutions)
                    .HasForeignKey(e => e.ComplaintId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Admin)
                    .WithMany()
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Handler là duplicate của Admin, ignore
                entity.Ignore(e => e.Handler);
            });

            // ======================== SystemAuditLog Configuration ========================

            builder.Entity<SystemAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedBy);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}