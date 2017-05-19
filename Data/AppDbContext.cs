using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ReactSpa.Data
{
    public class AppDbContext : IdentityDbContext<UserInfo>
    {
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<CheckRecord> CheckRecord { get; set; }
        public DbSet<LeavesReview> LeavesReview { get; set; }
        public DbSet<UserDeputy> UserDeputy { get; set; }
        public DbSet<UserSupervisor> UserSupervisor { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserInfo>()
                .Ignore(c => c.AccessFailedCount)
                .Ignore(c => c.EmailConfirmed)
                .Ignore(c => c.LockoutEnabled)
                .Ignore(c => c.LockoutEnd)
                .Ignore(c => c.PasswordHash)
                .Ignore(c => c.PhoneNumberConfirmed)
                .Ignore(c => c.TwoFactorEnabled)
                .Ignore(c => c.NormalizedEmail)
                .Ignore(c => c.NormalizedUserName);

            builder.Entity<UserInfo>().Property(s => s.UserName);
            builder.Entity<UserInfo>().HasAlternateKey(s => s.Email);
            builder.Entity<UserInfo>().Property(s => s.AnnualLeaves).HasColumnType("decimal(5, 2)").HasDefaultValue("0");
            builder.Entity<UserInfo>()
                .Property(s => s.FamilyCareLeaves)
                .HasColumnType("decimal(5, 2)")
                .HasDefaultValue("7");
            builder.Entity<UserInfo>().Property(s => s.SickLeaves).HasColumnType("decimal(5, 2)").HasDefaultValue("30");
            builder.Entity<UserInfo>().Property(s => s.JobTitle).IsRequired(false);
            builder.Entity<UserInfo>()
                .HasMany(s => s.CheckRecords)
                .WithOne(s => s.UserInfo)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserInfo>()
                .HasMany(s => s.Deputy)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<UserInfo>()
                .HasMany(s => s.Supervisor)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CheckRecord>().Property(s => s.CheckInTime).IsRequired(false);
            builder.Entity<CheckRecord>().Property(s => s.CheckOutTime).IsRequired(false);
            builder.Entity<CheckRecord>().Property(s => s.OffReason).IsRequired(false);
            builder.Entity<CheckRecord>().Property(s => s.OffType).IsRequired(false);
            builder.Entity<CheckRecord>().Property(s => s.OffTimeStart).IsRequired(false);
            builder.Entity<CheckRecord>().Property(s => s.OffTimeEnd).IsRequired(false);
            builder.Entity<CheckRecord>().Property(s => s.GeoLocation1).IsRequired(false);
            builder.Entity<CheckRecord>().Property(s => s.GeoLocation2).IsRequired(false);
            builder.Entity<CheckRecord>().Property(s => s.Comment).IsRequired(false);
            builder.Entity<CheckRecord>().Property(s => s.OffEndDate).IsRequired(false);
            builder.Entity<CheckRecord>()
                .HasMany(s => s.LeavesReview)
                .WithOne(s => s.Record)
                .HasForeignKey(s => s.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserInfo>().ToTable("User");
            builder.Entity<IdentityUserToken<String>>().ToTable("UserToken");
            builder.Entity<IdentityUserRole<String>>().ToTable("UserRole");
            builder.Entity<IdentityUserClaim<String>>()
                .ToTable("UserClaim")
                .Property(s => s.Id)
                .HasColumnName("ClaimId");
            builder.Entity<IdentityUserLogin<String>>().ToTable("UserLogin");
            builder.Entity<IdentityRoleClaim<String>>().ToTable("RoleClaim");
            builder.Entity<IdentityRole>().ToTable("Role").Property(s => s.Id).HasColumnName("RoleId");
        }
    }

    public class UserInfo : IdentityUser
    {
        private string _UserName;
        private DateTime? _DateOfEmployment;

        public override string UserName
        {
            get { return _UserName; }
            set { _UserName = Regex.Replace(value, @"\s+", ""); }
        }

        [Column(TypeName = "Date")]
        public DateTime? DateOfEmployment
        {
            get
            {
                if (_DateOfEmployment == null)
                    return DateTime.Today;
                return _DateOfEmployment;
            }
            set { _DateOfEmployment = value; }
        }

        public string JobTitle { get; set; }

        public decimal AnnualLeaves { get; set; }

        public decimal SickLeaves { get; set; }

        public decimal FamilyCareLeaves { get; set; }

        public virtual ICollection<CheckRecord> CheckRecords { get; set; }

        public virtual ICollection<UserDeputy> Deputy { get; set; }

        public virtual ICollection<UserSupervisor> Supervisor { get; set; }
    }


    [Table(("LeavesReview"))]
    public class LeavesReview
    {
        private string _Id;

        [Key]
        public string Id
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_Id))
                    return Guid.NewGuid().ToString();
                return _Id;
            }
            set { _Id = value; }
        }

        public string RecordId { get; set; }

        public string ReviewerId { get; set; }

        public virtual CheckRecord Record { get; set; }
    }

    [Table("CheckRecord")]
    public class CheckRecord
    {
        private string _Id;
        private DateTime _CheckedDate;
        private string _StatusOfApproval;

        [Key]
        public string Id
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_Id))
                    return Guid.NewGuid().ToString();
                return _Id;
            }
            set { _Id = value; }
        }

        public string UserId { get; set; }

        [Column(TypeName = "Date")]
        public DateTime CheckedDate
        {
            get
            {
                if (_CheckedDate == DateTime.MinValue)
                    _CheckedDate = DateTime.Today;
                return _CheckedDate;
            }
            set { _CheckedDate = value; }
        }

        public TimeSpan? CheckInTime { get; set; }

        public TimeSpan? CheckOutTime { get; set; }

        public string GeoLocation1 { get; set; }

        public string GeoLocation2 { get; set; }

        public string OffType { get; set; }

        public TimeSpan? OffTimeStart { get; set; }

        public TimeSpan? OffTimeEnd { get; set; }

        [Column(TypeName = "Date")]
        public DateTime? OffEndDate { get; set; }

        public string OffReason { get; set; }

        public string StatusOfApproval
        {
            get { return StatusOfApprovalEnum.PENDING(); }
            set { _StatusOfApproval = value; }
        }

        public string Comment { get; set; }

        public virtual UserInfo UserInfo { get; set; }

        public virtual ICollection<LeavesReview> LeavesReview { get; set; }
    }

    [Table("UserDeputy")]
    public class UserDeputy
    {
        private string _Id;

        [Key]
        public string Id
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_Id))
                    return Guid.NewGuid().ToString();
                return _Id;
            }
            set { _Id = value; }
        }

        public string UserId { get; set; }

        public string DeputyId { get; set; }

        public string DeputyName { get; set; }

        public virtual UserInfo User { get; set; }
    }

    [Table("UserSupervisor")]
    public class UserSupervisor
    {
        private string _Id;

        [Key]
        public string Id
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_Id))
                    return Guid.NewGuid().ToString();
                return _Id;
            }
            set { _Id = value; }
        }

        public string UserId { get; set; }

        public string SupervisorId { get; set; }

        public string SupervisorName { get; set; }

        public virtual UserInfo User { get; set; }
    }

    public class StatusOfApprovalEnum
    {
        public static string PENDING()
        {
            return "審核中";
        }

        public static string APPROVED()
        {
            return "已核准";
        }

        public static string REJECTED()
        {
            return "遭駁回";
        }
    }

    public class TypeEnum
    {
        public static string CHECK_IN()
        {
            return "打卡";
        }

        public static string PERSONAL_LEAVE()
        {
            return "事假";
        }

        public static string SICK_LEAVE()
        {
            return "病假";
        }

        public static string FUNERAL_LEAVE()
        {
            return "喪假";
        }

        public static string OFFICIAL_LEAVE()
        {
            return "公假";
        }

        public static string MARRIAGE_LEAVE()
        {
            return "婚假";
        }

        public static string FAMILY_CARE_LEAVE()
        {
            return "家庭照顧假";
        }

        public static string MATERNITY_LEAVE()
        {
            return "陪產假";
        }

        public static string COMPENSATORY_LEAVE()
        {
            return "補休";
        }

        public static string ANNUAL_LEAVE()
        {
            return "特休";
        }
    }
}