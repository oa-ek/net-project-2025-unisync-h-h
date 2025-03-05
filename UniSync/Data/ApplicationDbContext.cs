using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using UniSync.Models.Entity;

namespace UniSync.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure many-to-many relationship between Article and Tag
            modelBuilder.Entity<Article>()
                .HasMany(a => a.Tags)
                .WithMany(t => t.Articles)
                .UsingEntity(j => j.ToTable("ArticleTags"));

            // Configure many-to-many relationship between Project and Category
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Categories)
                .WithMany(c => c.Projects)
                .UsingEntity(j => j.ToTable("ProjectCategories"));

            // Fix the property types to match the navigation properties
            modelBuilder.Entity<Article>()
                .Property(a => a.UserId)
                .HasConversion<int>();

            modelBuilder.Entity<Article>()
                .Property(a => a.SpecialtyId)
                .HasConversion<int?>();

            modelBuilder.Entity<Comment>()
                .Property(c => c.UserId)
                .HasConversion<int>();

            modelBuilder.Entity<Project>()
                .Property(p => p.SubjectId)
                .HasConversion<int>();

            modelBuilder.Entity<Subject>()
                .Property(s => s.UserId)
                .HasConversion<int>();

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.ProjectId)
                .HasConversion<int>();

            modelBuilder.Entity<User>()
                .Property(u => u.SpecialtyId)
                .HasConversion<int?>();
        }
    }
}