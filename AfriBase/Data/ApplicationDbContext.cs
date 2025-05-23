using AfriBase.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AfriBase.Data
{
    /// <summary>
    /// Entity Framework Core database context for the AfriBase application
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Constructor for the application database context
        /// </summary>
        /// <param name="options">The DbContext options</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet for artifacts
        /// </summary>
        public DbSet<Artifact> Artifacts { get; set; }

        /// <summary>
        /// DbSet for bids
        /// </summary>
        public DbSet<Bid> Bids { get; set; }

        /// <summary>
        /// Configures the database model
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Artifact entity
            modelBuilder.Entity<Artifact>()
                .HasOne(a => a.Owner)
                .WithMany(u => u.OwnedArtifacts)
                .HasForeignKey(a => a.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Bid entity
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Artifact)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.ArtifactId)
                .OnDelete(DeleteBehavior.Cascade);

            // Set default schema
            modelBuilder.HasDefaultSchema("dbo");

            // Configure decimal precision
            modelBuilder.Entity<Artifact>()
                .Property(a => a.EstimatedValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Bid>()
                .Property(b => b.BidAmount)
                .HasPrecision(18, 2);
        }
    }
}