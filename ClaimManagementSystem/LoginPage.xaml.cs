using ClaimManagementSystem.Models;
using ClaimManagementSystem.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClaimManagementSystem
{
    public partial class LoginPage : Page
    {
        private readonly MainWindow mainWindow;
        public LoginPage(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;

            // Sync data between UserStore and DataService
            SyncUserData();

            // Show debug info
            ShowDebugInfo();

            // Auto-fill last login
            AutoFillLastLogin();
        }

        private void ShowDebugInfo()
        {
            string debugInfo = UserStore.GetDebugInfo();
            Console.WriteLine("=== ALL USERS ===");
            foreach (var user in UserStore.GetUsers())
            {
                Console.WriteLine($"{user.Email} - {user.Role} - Active: {user.IsActive}");
            }
            Console.WriteLine("=== END DEBUG INFO ===");
        }

        private void SyncUserData()
        {
            try
            {
                Console.WriteLine("=== SYNCING USER DATA ===");
                var dataService = new DataService();
                var userStoreUsers = UserStore.GetUsers();

                foreach (var user in userStoreUsers)
                {
                    Console.WriteLine($"Checking user: {user.Email} - {user.Role}");

                    // Check if user exists in DataService
                    var dsUser = dataService.GetUserByEmail(user.Email);
                    if (dsUser == null)
                    {
                        Console.WriteLine($"Adding {user.Email} to DataService");
                        dataService.AddUser(user);
                    }
                }
                Console.WriteLine("=== SYNC COMPLETE ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync error: {ex.Message}");
            }
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            Console.WriteLine($"=== LOGIN ATTEMPT: {email} ===");

            // Validate inputs
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter your email address.", "Login Failed",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your password.", "Login Failed",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TEMPORARY: Try multiple authentication sources
            User authenticatedUser = null;

            // 1. Try default hardcoded users first
            authenticatedUser = TryDefaultUsers(email, password);

            // 2. If not found, try UserStore
            if (authenticatedUser == null)
            {
                authenticatedUser = UserStore.GetUserByEmail(email);
                if (authenticatedUser != null && authenticatedUser.Password == password)
                {
                    Console.WriteLine($"Authenticated via UserStore: {authenticatedUser.FullName}");
                }
                else
                {
                    authenticatedUser = null;
                }
            }

            // 3. If still not found, try DataService
            if (authenticatedUser == null)
            {
                var dataService = new DataService();
                authenticatedUser = dataService.GetUserByEmail(email);
                if (authenticatedUser != null && authenticatedUser.Password == password)
                {
                    Console.WriteLine($"Authenticated via DataService: {authenticatedUser.FullName}");
                }
                else
                {
                    authenticatedUser = null;
                }
            }

            if (authenticatedUser != null)
            {
                LoginSuccess(authenticatedUser);
            }
            else
            {
                MessageBox.Show($"Login failed for: {email}\n\nPlease try one of these test accounts:\n\n" +
                               "HR Manager: hr@university.com / hr123\n" +
                               "Admin: admin@university.com / admin123\n" +
                               "Lecturer: lecturer@university.com / lecturer123",
                               "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CleanUpUserData()
        {
            try
            {
                Console.WriteLine("=== CLEANING UP USER DATA ===");

                // Get current users and remove duplicates
                var currentUsers = UserStore.GetUsers();
                var uniqueUsers = currentUsers
                    .GroupBy(u => u.Email.ToLower())
                    .Select(g => g.First())
                    .ToList();

                Console.WriteLine($"Before cleanup: {currentUsers.Count} users");
                Console.WriteLine($"After cleanup: {uniqueUsers.Count} users");

                // Save the cleaned list back to UserStore
                // Since UserStore doesn't have a clear method, we'll recreate the file
                var dataFilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ClaimManagementSystem", "users.json");

                if (File.Exists(dataFilePath))
                {
                    File.Delete(dataFilePath);
                    Console.WriteLine("Deleted corrupted users file");
                }

                // Recreate with default users
                UserStore.GetUsers(); // This will trigger recreation of default users

                Console.WriteLine("=== CLEANUP COMPLETE ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup error: {ex.Message}");
            }
        }

        private User TryDefaultUsers(string email, string password)
        {
            var defaultUsers = new[]
            {
        new { Email = "hr@university.com", Password = "hr123", Name = "HR Manager", Role = UserRole.HRManager },
        new { Email = "admin@university.com", Password = "admin123", Name = "Admin User", Role = UserRole.AcademicManager },
        new { Email = "lecturer@university.com", Password = "lecturer123", Name = "John Lecturer", Role = UserRole.Lecturer },
        new { Email = "coordinator@university.com", Password = "coord123", Name = "Sarah Coordinator", Role = UserRole.ProgramCoordinator }
    };

            var match = defaultUsers.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (match != null)
            {
                return new User
                {
                    Email = match.Email,
                    FullName = match.Name,
                    Password = match.Password,
                    Role = match.Role,
                    Department = match.Role == UserRole.HRManager ? "HR" : "Computer Science",
                    IsActive = true
                };
            }

            return null;
        }

        private void LoginSuccess(User user)
        {
            RememberLastLogin(user.Email);
            mainWindow.NavigateAfterLogin(user);
        }

        private void TextBlock_SignUp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.MainFrame.Navigate(new RegisterPage(mainWindow));
        }

        private User AuthenticateUser(string email, string password)
        {
            Console.WriteLine($"=== AUTHENTICATING: {email} ===");

            // Try UserStore first
            var userStoreUser = UserStore.GetUserByEmail(email);
            if (userStoreUser != null && userStoreUser.Password == password && userStoreUser.IsActive)
            {
                Console.WriteLine($"Found in UserStore: {userStoreUser.FullName} - {userStoreUser.Role}");
                return userStoreUser;
            }

            // Try DataService as backup
            var dataService = new DataService();
            var dataServiceUser = dataService.GetUserByEmail(email);
            if (dataServiceUser != null && dataServiceUser.Password == password && dataServiceUser.IsActive)
            {
                Console.WriteLine($"Found in DataService: {dataServiceUser.FullName} - {dataServiceUser.Role}");

                // Ensure this user exists in UserStore too
                if (userStoreUser == null)
                {
                    Console.WriteLine($"Adding user to UserStore: {dataServiceUser.Email}");
                    UserStore.AddUser(dataServiceUser);
                }

                return dataServiceUser;
            }

            Console.WriteLine($"User not found or invalid credentials: {email}");
            return null;
        }

        private void RememberLastLogin(string email)
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appFolder = Path.Combine(appData, "ClaimManagementSystem");
                Directory.CreateDirectory(appFolder);

                var lastLoginFile = Path.Combine(appFolder, "lastlogin.txt");
                File.WriteAllText(lastLoginFile, email);
            }
            catch
            {
                // Silently fail
            }
        }


        private void AutoFillLastLogin()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var lastLoginFile = Path.Combine(appData, "ClaimManagementSystem", "lastlogin.txt");

                if (File.Exists(lastLoginFile))
                {
                    var lastEmail = File.ReadAllText(lastLoginFile).Trim();
                    if (!string.IsNullOrEmpty(lastEmail))
                    {
                        txtEmail.Text = lastEmail;
                        txtPassword.Focus();
                    }
                }
            }
            catch
            {
                // Silently fail
            }
        }
    }
}