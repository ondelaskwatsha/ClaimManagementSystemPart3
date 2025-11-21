using System;
using System.Windows;
using System.Windows.Controls;
using ClaimManagementSystem.Models;
using ClaimManagementSystem.Services;

namespace ClaimManagementSystem
{
    public partial class MainWindow : Window
    {
        public static User? CurrentUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            NavigateToLogin();
        }

        public void NavigateToLogin()
        {
            MainFrame.Navigate(new LoginPage(this));
            UpdateNavigationVisibility(false);
        }

        public void NavigateAfterLogin(User user)
        {
            CurrentUser = user;

            // DEBUG: Show user info
            Console.WriteLine($"=== NAVIGATING AFTER LOGIN ===");
            Console.WriteLine($"User: {user.FullName}");
            Console.WriteLine($"Role: {user.Role}");
            Console.WriteLine($"Email: {user.Email}");

            UpdateNavigationVisibility(true);

            // Role-based navigation and access control
            switch (user.Role)
            {
                case UserRole.Lecturer:
                    Console.WriteLine("Navigating to LECTURER view");
                    MainFrame.Navigate(new SubmitClaimPage());
                    btnManageClaims.Visibility = Visibility.Collapsed;
                    btnHRManagement.Visibility = Visibility.Collapsed;
                    break;
                case UserRole.ProgramCoordinator:
                case UserRole.AcademicManager:
                    Console.WriteLine("Navigating to COORDINATOR/MANAGER view");
                    MainFrame.Navigate(new ManageClaimsPage(user.Role));
                    btnHRManagement.Visibility = Visibility.Collapsed;
                    break;
                case UserRole.HRManager:
                    Console.WriteLine("Navigating to HR MANAGER view");
                    MainFrame.Navigate(new HRManagementPage());
                    btnSubmitClaim.Visibility = Visibility.Collapsed;
                    btnMyClaims.Visibility = Visibility.Collapsed;
                    btnManageClaims.Visibility = Visibility.Collapsed;
                    break;
                default:
                    Console.WriteLine("Unknown role, defaulting to lecturer view");
                    MainFrame.Navigate(new SubmitClaimPage());
                    break;
            }

            txtWelcome.Text = $"Welcome, {user.FullName} ({user.Role})";
        }

        private void UpdateNavigationVisibility(bool isLoggedIn)
        {
            btnOverview.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            btnSubmitClaim.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            btnMyClaims.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            btnManageClaims.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            btnHRManagement.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            btnLogout.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            txtWelcome.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnManageClaims_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser == null || CurrentUser.Role == UserRole.Lecturer)
            {
                MessageBox.Show("Access denied. Lecturers cannot manage claims.", "Unauthorized",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainFrame.Navigate(new ManageClaimsPage(CurrentUser.Role));
        }

        private void BtnHRManagement_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser == null || CurrentUser.Role != UserRole.HRManager)
            {
                MessageBox.Show("Access denied. Only HR Managers can access this section.", "Unauthorized",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainFrame.Navigate(new HRManagementPage());
        }

        // Navigation methods
        public void NavigateToOverview(object sender, RoutedEventArgs e) => MainFrame.Navigate(new OverviewPage());
        public void NavigateToSubmitClaim(object sender, RoutedEventArgs e) => MainFrame.Navigate(new SubmitClaimPage());
        public void NavigateToMyClaims(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MyClaimsPage());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser = null;
            NavigateToLogin();
        }
    }
}