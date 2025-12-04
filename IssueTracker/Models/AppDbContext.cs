using IssueTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Issue> Issues { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One Issue has many Comments; a Comment belongs to one Issue
            modelBuilder.Entity<Comment>()
                .HasOne<Issue>()                     // principal
                .WithMany()                          // many comments
                .HasForeignKey(c => c.IssueId)       // FK property
                .OnDelete(DeleteBehavior.Cascade);   // delete comments when issue is deleted
        }
    }
}
