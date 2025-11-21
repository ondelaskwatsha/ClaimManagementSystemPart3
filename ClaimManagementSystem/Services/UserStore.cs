using ClaimManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace ClaimManagementSystem.Services
{
    public static class UserStore
    {
        private static readonly string DataFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClaimManagementSystem", "users.json");

        private static List<User> _users = new List<User>();

        static UserStore()
        {
            LoadUsers();

            // If no users exist, create default ones
            if (!_users.Any())
            {
                CreateDefaultUsers();
            }
        }

        private static void LoadUsers()
        {
            try
            {
                if (File.Exists(DataFilePath))
                {
                    string json = File.ReadAllText(DataFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                _users = new List<User>();
            }
        }

        private static void SaveUsers()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DataFilePath));
                string json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(DataFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving users: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void CreateDefaultUsers()
        {
            _users = new List<User>
            {
                new User
                {
                    FullName = "Admin User",
                    Email = "admin@university.com",
                    Password = "admin123",
                    Role = UserRole.AcademicManager,
                    Department = "Administration"
                },
                new User
                {
                    FullName = "HR Manager",
                    Email = "hr@university.com",
                    Password = "hr123",
                    Role = UserRole.HRManager,
                    Department = "HR"
                },
                new User
                {
                    FullName = "John Lecturer",
                    Email = "lecturer@university.com",
                    Password = "lecturer123",
                    Role = UserRole.Lecturer,
                    Department = "Computer Science"
                },
                new User
                {
                    FullName = "Sarah Coordinator",
                    Email = "coordinator@university.com",
                    Password = "coord123",
                    Role = UserRole.ProgramCoordinator,
                    Department = "Computer Science"
                }
            };
            SaveUsers();
        }

        public static List<User> GetUsers()
        {
            return _users.ToList(); // Return a copy
        }

        public static User GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public static void AddUser(User user)
        {
            if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            _users.Add(user);
            SaveUsers();
        }

        // Debug method to see what's happening
        public static string GetDebugInfo()
        {
            return $"Total users: {_users.Count}\n" +
                   $"Users: {string.Join(", ", _users.Select(u => u.Email))}\n" +
                   $"File path: {DataFilePath}\n" +
                   $"File exists: {File.Exists(DataFilePath)}";
        }
    }
}