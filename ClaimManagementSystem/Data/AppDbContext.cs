using ClaimManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ClaimManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Claim> Claims { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=claims.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed default data
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Email = "admin@university.com",
                    FullName = "Admin User",
                    Password = "admin123",
                    Role = UserRole.AcademicManager,
                    Department = "Administration",
                    IsActive = true
                },
                new User
                {
                    Email = "hr@university.com",
                    FullName = "HR Manager",
                    Password = "hr123",
                    Role = UserRole.HRManager,
                    Department = "HR",
                    IsActive = true
                }
            );
        }
    }
}