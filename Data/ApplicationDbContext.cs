using Microsoft.EntityFrameworkCore;
using JCAM_CONNECT.Models;

namespace JCAM_CONNECT.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<MemberProfile> MemberProfiles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Testimony> Testimonies { get; set; }
        public DbSet<TestimonyDocument> TestimonyDocuments { get; set; }
        public DbSet<PrayerRequest> PrayerRequests { get; set; }
        public DbSet<PrayerResponse> PrayerResponses { get; set; }
        public DbSet<CounsellingSession> CounsellingSessions { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Devotional> Devotionals { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<BaptismRequest> BaptismRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User -> MemberProfile (One-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.MemberProfile)
                .WithOne(m => m.User)
                .HasForeignKey<MemberProfile>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Testimony (One-to-Many)
            modelBuilder.Entity<Testimony>()
                .HasOne(t => t.User)
                .WithMany(u => u.Testimonies)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Testimony -> TestimonyDocument (One-to-Many)
            modelBuilder.Entity<TestimonyDocument>()
                .HasOne(d => d.Testimony)
                .WithMany(t => t.Documents)
                .HasForeignKey(d => d.TestimonyId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> PrayerRequest (One-to-Many)
            modelBuilder.Entity<PrayerRequest>()
                .HasOne(p => p.User)
                .WithMany(u => u.PrayerRequests)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PrayerRequest -> PrayerResponse (One-to-Many)
            modelBuilder.Entity<PrayerResponse>()
                .HasOne(r => r.PrayerRequest)
                .WithMany(p => p.Responses)
                .HasForeignKey(r => r.PrayerRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> PrayerResponse (One-to-Many)
            modelBuilder.Entity<PrayerResponse>()
                .HasOne(r => r.Responder)
                .WithMany(u => u.PrayerResponses)
                .HasForeignKey(r => r.ResponderId)
                .OnDelete(DeleteBehavior.Restrict);

            // MemberProfile -> Attendance (One-to-Many)
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Member)
                .WithMany(m => m.Attendances)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event -> Attendance (One-to-Many)
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Event)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> CounsellingSession (as Member)
            modelBuilder.Entity<CounsellingSession>()
                .HasOne(c => c.Member)
                .WithMany(u => u.CounsellingSessionsAsMember)
                .HasForeignKey(c => c.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> CounsellingSession (as Pastor)
            modelBuilder.Entity<CounsellingSession>()
                .HasOne(c => c.Pastor)
                .WithMany(u => u.CounsellingSessionsAsPastor)
                .HasForeignKey(c => c.PastorId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Devotional (as Author)
            modelBuilder.Entity<Devotional>()
                .HasOne(d => d.Author)
                .WithMany(u => u.Devotionals)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Add unique indexes
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Event>().HasIndex(e => e.EventDate);

            // Performance indexes
            modelBuilder.Entity<Testimony>().HasIndex(t => t.IsApproved);
            modelBuilder.Entity<PrayerRequest>().HasIndex(p => p.SubmittedAt);
            modelBuilder.Entity<PrayerRequest>().HasIndex(p => p.IsPrayedFor);

            // Add this to OnModelCreating
            modelBuilder.Entity<Donation>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Add this to onModelCreating
            modelBuilder.Entity<BaptismRequest>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

           
        }

    }
}