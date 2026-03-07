using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Text.Json;

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
        public DbSet<WasteImage> WasteImages { get; set; } // ✅ NEW

        // Collection & Points
        public DbSet<CitizenPoint> CitizenPoints { get; set; }
        public DbSet<CitizenPointHistory> CitizenPointHistories { get; set; }
        public DbSet<CollectionProof> CollectionProofs { get; set; }
        public DbSet<CollectionRequest> CollectionRequests { get; set; }
        public DbSet<CollectorAssignment> CollectorAssignments { get; set; }
        public DbSet<CollectorProfile> CollectorProfiles { get; set; }
        public DbSet<PointRule> PointRules { get; set; }

        // Enterprise & Services
        public DbSet<RecyclingEnterprise> RecyclingEnterprises { get; set; }
        public DbSet<EnterpriseServiceArea> EnterpriseServiceAreas { get; set; }
        public DbSet<EnterpriseWasteCapability> EnterpriseWasteCapabilities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Ward> Wards { get; set; }

        // Statistics & Complaints
        public DbSet<RecyclingStatistic> RecyclingStatistics { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<DisputeResolution> DisputeResolutions { get; set; }

        // Audit
        public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }

        // Refresh Token
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ======================== Identity Rename ========================

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

                entity.HasIndex(e => e.RegionCode);
            });

            // ======================== District Configuration ========================

            builder.Entity<District>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ProvinceCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Code).IsUnique();

                entity.HasMany(e => e.Wards)
                    .WithOne(w => w.District)
                    .HasForeignKey(w => w.DistrictId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================== Ward Configuration ========================

            builder.Entity<Ward>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(e => e.District)
                    .WithMany(d => d.Wards)
                    .HasForeignKey(e => e.DistrictId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.DistrictId, e.Code })
                    .IsUnique();
            }); 
            // ======================== WasteReportWaste Configuration ========================

            builder.Entity<WasteReportWaste>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.EstimatedWeightKg)
                    .HasPrecision(10, 2);

                entity.HasOne(e => e.WasteType)
                    .WithMany()
                    .HasForeignKey(e => e.WasteTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.WasteReport)
                    .WithMany(r => r.Wastes)
                    .HasForeignKey(e => e.WasteReportId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Images)
                    .WithOne(i => i.WasteReportWaste)
                    .HasForeignKey(i => i.WasteReportWasteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================== WasteImage Configuration ========================

            builder.Entity<WasteImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ImageUrl)
                    .IsRequired();

                entity.Property(e => e.ImageType)
                    .HasConversion<string>();

                entity.HasOne(e => e.WasteReportWaste)
                    .WithMany(w => w.Images)
                    .HasForeignKey(e => e.WasteReportWasteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================== WasteType Configuration ========================

            builder.Entity<WasteType>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Category)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // Optional: nếu bạn muốn Name là unique
                entity.HasIndex(e => e.Name)
                    .IsUnique();
            });

            // ======================== CitizenPoint Configuration ========================

            builder.Entity<CitizenPoint>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Citizen)
                    .WithMany()
                    .HasForeignKey(e => e.CitizenId)
                    .OnDelete(DeleteBehavior.Restrict);

                // mỗi citizen chỉ có 1 record điểm
                entity.HasIndex(e => e.CitizenId)
                    .IsUnique();
            });

            // ======================== CollectionRequest Configuration ========================

            builder.Entity<CollectionRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.WasteReportWaste)
                    .WithMany()
                    .HasForeignKey(e => e.WasteReportWasteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Enterprise)
                    .WithMany(e => e.CollectionRequests)
                    .HasForeignKey(e => e.EnterpriseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasMany(e => e.Assignments)
                    .WithOne(a => a.Request)
                    .HasForeignKey(a => a.RequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                // MVP: 1 món rác -> 1 request
                entity.HasIndex(e => e.WasteReportWasteId).IsUnique();

                // dashboard enterprise
                entity.HasIndex(e => new { e.EnterpriseId, e.Status });
            });

            // ======================== CollectorAssignment Configuration ========================

            builder.Entity<CollectorAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Request)
                    .WithMany(r => r.Assignments)
                    .HasForeignKey(e => e.RequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Collector)
                    .WithMany(c => c.Assignments)
                    .HasForeignKey(e => e.CollectorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasMany(e => e.Proofs)
                    .WithOne(p => p.Assignment)
                    .HasForeignKey(p => p.AssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.CollectorId, e.Status });

                // MVP: không re-assign
                entity.HasIndex(e => e.RequestId).IsUnique();

                entity.Property(e => e.CollectedNote).HasMaxLength(1000);
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
                    .WithMany(e => e.Collectors)
                    .HasForeignKey(e => e.EnterpriseId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Optional nhưng nên có: unique để 1 user chỉ có 1 profile collector
                entity.HasIndex(e => e.CollectorId).IsUnique();
            });

            // ======================== CollectionProof Configuration ========================

            builder.Entity<CollectionProof>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ImageUrl).IsRequired();
                entity.Property(e => e.PublicId).IsRequired();

                entity.Property(e => e.ReviewStatus)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasOne(e => e.Assignment)
                    .WithMany(a => a.Proofs)
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.AssignmentId);
                entity.HasIndex(e => e.PublicId);
                entity.HasIndex(e => e.ReviewStatus);
            });

            // ======================== PointRule Configuration ========================

            builder.Entity<PointRule>(entity =>
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

                entity.HasIndex(e => new { e.EnterpriseId, e.WasteTypeId })
                    .IsUnique();
            });

            // ======================== RecyclingEnterprise Configuration ========================

            builder.Entity<RecyclingEnterprise>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.TaxCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LegalRepresentative)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.RepresentativePosition)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.ApprovalStatus)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(e => e.OperationalStatus)
                    .HasConversion<string>()
                    .IsRequired();

                // Owner account
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // 1 User = 1 Enterprise
                entity.HasIndex(e => e.UserId)
                    .IsUnique();
            });

            // ======================== EnterpriseServiceArea Configuration ========================

            builder.Entity<EnterpriseServiceArea>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Enterprise)
                    .WithMany(e => e.ServiceAreas)
                    .HasForeignKey(e => e.EnterpriseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.District)
                    .WithMany()
                    .HasForeignKey(e => e.DistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Ward)
                    .WithMany()
                    .HasForeignKey(e => e.WardId)
                    .OnDelete(DeleteBehavior.Restrict);

                // 1 enterprise không được duplicate district + ward
                entity.HasIndex(e => new { e.EnterpriseId, e.DistrictId, e.WardId })
                    .IsUnique();
            });

            // ======================== EnterpriseWasteCapability Configuration ========================

            builder.Entity<EnterpriseWasteCapability>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DailyCapacityKg).HasPrecision(18, 2);

                entity.HasOne(e => e.Enterprise)
                    .WithMany(e => e.WasteCapabilities)
                    .HasForeignKey(e => e.EnterpriseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.WasteType)
                    .WithMany()
                    .HasForeignKey(e => e.WasteTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.EnterpriseId, e.WasteTypeId }).IsUnique();
                entity.HasIndex(e => e.WasteTypeId);
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
                // Complainant (ApplicationUser)
                entity.HasOne(c => c.Complainant)
                    .WithMany()
                    .HasForeignKey(c => c.ComplainantId)
                    .OnDelete(DeleteBehavior.Restrict);

                // WasteReport
                entity.HasOne(c => c.Report)
                    .WithMany()
                    .HasForeignKey(c => c.ReportId)
                    .OnDelete(DeleteBehavior.Cascade);

                // CollectionRequest (optional)
                entity.HasOne(c => c.CollectionRequest)
                    .WithMany()
                    .HasForeignKey(c => c.CollectionRequestId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Resolutions
                entity.HasMany(c => c.Resolutions)
                    .WithOne(r => r.Complaint)
                    .HasForeignKey(r => r.ComplaintId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.Content)
                    .IsRequired()
                    .HasMaxLength(1000);
            });

            // ======================== DisputeResolution Configuration ========================

            builder.Entity<DisputeResolution>(entity =>
            {
                entity.HasOne(r => r.Complaint)
                    .WithMany(c => c.Resolutions)
                    .HasForeignKey(r => r.ComplaintId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Handler)
                    .WithMany()
                    .HasForeignKey(r => r.HandlerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(r => r.ResolutionNote)
                    .IsRequired()
                    .HasMaxLength(1000);
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

            // ======================== CitizenPointHistory Configuration ========================

            builder.Entity<CitizenPointHistory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Points)
                    .IsRequired();

                entity.Property(e => e.Reason)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasOne(e => e.Citizen)
                    .WithMany()
                    .HasForeignKey(e => e.CitizenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.WasteReport)
                    .WithMany()
                    .HasForeignKey(e => e.WasteReportId)
                    .OnDelete(DeleteBehavior.SetNull);

                // leaderboard query
                entity.HasIndex(e => e.CitizenId);
                entity.HasIndex(e => e.CreatedTime);
                entity.HasIndex(e => e.WasteReportId);
            });
        }
    }
}