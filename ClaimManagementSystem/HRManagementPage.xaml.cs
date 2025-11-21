using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClaimManagementSystem.Models;
using ClaimManagementSystem.Services;

namespace ClaimManagementSystem
{
    public partial class HRManagementPage : Page
    {
        private DataService _dataService = new DataService();

        public HRManagementPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Load lecturers
                var lecturers = _dataService.GetLecturers();
                dgLecturers.ItemsSource = lecturers;

                // Load approved claims for payment
                var approvedClaims = _dataService.GetClaimsForPayment();
                dgApprovedClaims.ItemsSource = approvedClaims;

                // Load statistics
                var stats = _dataService.GetStatistics();
                txtTotalUsers.Text = $"Total Users: {stats.totalUsers}";
                txtTotalClaims.Text = $"Total Claims: {stats.totalClaims}";
                txtPendingPayments.Text = $"Pending Payments: {stats.pendingPayments}";
                txtTotalAmount.Text = $"Total Amount: R{stats.totalAmount:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            var fromDate = dpFromDate.SelectedDate;
            var toDate = dpToDate.SelectedDate;

            if (fromDate == null || toDate == null)
            {
                MessageBox.Show("Please select both from and to dates.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (fromDate > toDate)
            {
                MessageBox.Show("From date cannot be after to date.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // In a real application, this would generate a detailed report
            var claimsInPeriod = _dataService.GetAllClaims()
                .Where(c => c.SubmittedDate >= fromDate && c.SubmittedDate <= toDate)
                .ToList();

            MessageBox.Show($"Generated payment report for period {fromDate.Value:MM/dd/yyyy} to {toDate.Value:MM/dd/yyyy}\n" +
                          $"Claims found: {claimsInPeriod.Count}\n" +
                          $"Total amount: R{claimsInPeriod.Sum(c => c.Amount):N2}",
                          "Report Generated", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportClaims_Click(object sender, RoutedEventArgs e)
        {
            var approvedClaims = _dataService.GetClaimsForPayment();

            if (!approvedClaims.Any())
            {
                MessageBox.Show("No approved claims available for export.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Simulate CSV export
            string csvContent = "Lecturer,Title,Amount,ApprovedDate\n";
            foreach (var claim in approvedClaims)
            {
                csvContent += $"{claim.UserEmail},{claim.Title},R{claim.Amount:N2},{claim.ApprovedDate:MM/dd/yyyy}\n";
            }

            // In real application, save to file
            MessageBox.Show($"Exported {approvedClaims.Count} approved claims for payment processing.\n\n" +
                          "Sample CSV content:\n" + csvContent.Substring(0, Math.Min(200, csvContent.Length)) + "...",
                          "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAddLecturer_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This would open a form to add new lecturer.\n" +
                          "Implementation would include:\n" +
                          "- User registration form\n" +
                          "- Email validation\n" +
                          "- Department assignment\n" +
                          "- Automatic account creation",
                          "Add New Lecturer", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDeactivateUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = dgLecturers.SelectedItem as User;
            if (selectedUser != null)
            {
                var result = MessageBox.Show($"Are you sure you want to deactivate {selectedUser.FullName}?",
                                           "Confirm Deactivation",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    selectedUser.IsActive = false;
                    _dataService.UpdateUser(selectedUser);
                    LoadData(); // Refresh data
                    MessageBox.Show($"User {selectedUser.FullName} has been deactivated.",
                                  "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a user to deactivate.", "Selection Required",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnProcessPayments_Click(object sender, RoutedEventArgs e)
        {
            var selectedClaims = dgApprovedClaims.SelectedItems.Cast<Claim>().ToList();

            if (!selectedClaims.Any())
            {
                MessageBox.Show("Please select claims to process for payment.", "Selection Required",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var totalAmount = selectedClaims.Sum(c => c.Amount);
            var result = MessageBox.Show($"Process payments for {selectedClaims.Count} claims?\nTotal amount: R{totalAmount:N2}",
                                       "Confirm Payment Processing",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var claim in selectedClaims)
                {
                    claim.Status = ClaimStatus.Paid;
                    claim.PaidDate = DateTime.Now;
                    _dataService.UpdateClaim(claim);
                }

                MessageBox.Show($"Successfully processed {selectedClaims.Count} payments.\nTotal paid: R{totalAmount:N2}",
                              "Payments Processed", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData(); // Refresh data
            }
        }
    }
}