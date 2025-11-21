// Services/DataService.cs - ENHANCED
using System;
using System.Collections.Generic;
using System.Linq;
using ClaimManagementSystem.Models;

namespace ClaimManagementSystem.Services
{
    public class DataService
    {
        private static List<Claim> _claims = new List<Claim>();
        private static List<User> _users => UserStore.GetUsers(); // Use UserStore as source of truth

        static DataService()
        {
            InitializeSampleData();
        }

        private static void InitializeSampleData()
        {
            // Clear existing data
            _users.Clear();
            _claims.Clear();

            // Add sample users
            _users.AddRange(new List<User>
            {
                new User
                {
                    FullName = "Admin User",
                    Email = "admin@university.com",
                    Password = "admin123",
                    Role = UserRole.AcademicManager,
                    Department = "Administration",
                    IsActive = true
                },
                new User
                {
                    FullName = "HR Manager",
                    Email = "hr@university.com",
                    Password = "hr123",
                    Role = UserRole.HRManager,
                    Department = "HR",
                    IsActive = true
                },
                new User
                {
                    FullName = "John Lecturer",
                    Email = "lecturer@university.com",
                    Password = "lecturer123",
                    Role = UserRole.Lecturer,
                    Department = "Computer Science",
                    IsActive = true
                },
                new User
                {
                    FullName = "Sarah Coordinator",
                    Email = "coordinator@university.com",
                    Password = "coord123",
                    Role = UserRole.ProgramCoordinator,
                    Department = "Computer Science",
                    IsActive = true
                }
            });

            // Add sample claims
            _claims.AddRange(new List<Claim>
            {
                new Claim
                {
                    Title = "September 2024 Teaching Hours",
                    MonthYear = "Sep 2024",
                    Hours = 40,
                    HourlyRate = 130,
                    Amount = 5200,
                    Status = ClaimStatus.Approved,
                    UserEmail = "lecturer@university.com",
                    SubmittedDate = DateTime.Now.AddDays(-30),
                    ApprovedDate = DateTime.Now.AddDays(-20),
                    Description = "Regular teaching hours for September",
                    ReviewedBy = "admin@university.com",
                    ApprovedBy = "admin@university.com",
                    FilePaths = ""
                },
                new Claim
                {
                    Title = "October 2024 Teaching Hours",
                    MonthYear = "Oct 2024",
                    Hours = 45,
                    HourlyRate = 130,
                    Amount = 5850,
                    Status = ClaimStatus.UnderReview,
                    UserEmail = "lecturer@university.com",
                    SubmittedDate = DateTime.Now.AddDays(-10),
                    Description = "Regular teaching hours for October",
                    ReviewedBy = "coordinator@university.com",
                    FilePaths = ""
                },
                new Claim
                {
                    Title = "November 2024 Teaching Hours",
                    MonthYear = "Nov 2024",
                    Hours = 35,
                    HourlyRate = 130,
                    Amount = 4550,
                    Status = ClaimStatus.Draft,
                    UserEmail = "lecturer@university.com",
                    Description = "Regular teaching hours for November",
                    FilePaths = ""
                }
            });
        }

        // Claim methods
        public List<Claim> GetClaimsByUser(string email)
        {
            return _claims.Where(c => c.UserEmail == email)
                         .OrderByDescending(c => c.SubmittedDate)
                         .ToList();
        }

        public List<Claim> GetAllClaims()
        {
            return _claims.OrderByDescending(c => c.SubmittedDate).ToList();
        }

        public List<Claim> GetPendingClaims()
        {
            return _claims.Where(c => c.Status == ClaimStatus.Submitted ||
                                     c.Status == ClaimStatus.UnderReview)
                         .OrderBy(c => c.SubmittedDate)
                         .ToList();
        }

        public List<Claim> GetClaimsForPayment()
        {
            return _claims.Where(c => c.Status == ClaimStatus.Approved)
                         .OrderBy(c => c.ApprovedDate)
                         .ToList();
        }

        public void AddClaim(Claim claim)
        {
            _claims.Add(claim);
        }

        public void UpdateClaim(Claim updatedClaim)
        {
            var existingClaim = _claims.FirstOrDefault(c => c.Id == updatedClaim.Id);
            if (existingClaim != null)
            {
                _claims.Remove(existingClaim);
                _claims.Add(updatedClaim);
            }
        }

        public Claim? GetClaimById(string id)
        {
            return _claims.FirstOrDefault(c => c.Id == id);
        }

        // User methods
        public User? GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        }

        public List<User> GetAllUsers()
        {
            return _users.Where(u => u.IsActive).ToList();
        }

        public List<User> GetLecturers()
        {
            return _users.Where(u => u.Role == UserRole.Lecturer && u.IsActive).ToList();
        }

        public void AddUser(User user)
        {
            if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("User with this email already exists.");

            _users.Add(user);
        }

        public void UpdateUser(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Email == user.Email);
            if (existingUser != null)
            {
                _users.Remove(existingUser);
                _users.Add(user);
            }
        }

        // Statistics
        public (int totalUsers, int totalClaims, int pendingPayments, decimal totalAmount) GetStatistics()
        {
            var totalUsers = _users.Count(u => u.IsActive);
            var totalClaims = _claims.Count;
            var pendingPayments = _claims.Count(c => c.Status == ClaimStatus.Approved);
            var totalAmount = _claims.Where(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Paid)
                                   .Sum(c => c.Amount);

            return (totalUsers, totalClaims, pendingPayments, totalAmount);
        }

        // Reset data for testing
        public void ResetSampleData()
        {
            InitializeSampleData();
        }
    }
}