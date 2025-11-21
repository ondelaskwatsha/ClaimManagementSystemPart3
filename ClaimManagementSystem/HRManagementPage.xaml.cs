using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClaimManagementSystem.Models;
using ClaimManagementSystem.Services;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace ClaimManagementSystem
{
    public partial class HRManagementPage : Page
    {
        private DataService _dataService = new DataService();
        private List<Claim> _currentReportClaims = new List<Claim>();

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

            // Generate the report data
            _currentReportClaims = _dataService.GetAllClaims()
                .Where(c => c.SubmittedDate >= fromDate && c.SubmittedDate <= toDate)
                .ToList();

            // Enable download button if we have data
            btnDownloadReport.IsEnabled = _currentReportClaims.Any();

            // Show report summary
            var totalAmount = _currentReportClaims.Sum(c => c.Amount);
            var approvedAmount = _currentReportClaims.Where(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Paid).Sum(c => c.Amount);

            MessageBox.Show($"Generated payment report for period {fromDate.Value:MM/dd/yyyy} to {toDate.Value:MM/dd/yyyy}\n\n" +
                          $"Total Claims: {_currentReportClaims.Count}\n" +
                          $"Approved/Paid Claims: {_currentReportClaims.Count(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Paid)}\n" +
                          $"Total Amount: R{totalAmount:N2}\n" +
                          $"Approved Amount: R{approvedAmount:N2}\n\n" +
                          $"You can now download the detailed report as CSV.",
                          "Report Generated", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDownloadReport_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentReportClaims.Any())
            {
                MessageBox.Show("No report data available. Please generate a report first.", "No Data",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Create CSV content
                string csvContent = GenerateReportCsv(_currentReportClaims);

                // Ask user where to save the file
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    FileName = $"Payment_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    DefaultExt = ".csv",
                    Title = "Save Payment Report"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Save the file
                    File.WriteAllText(saveFileDialog.FileName, csvContent, Encoding.UTF8);

                    MessageBox.Show($"Report successfully downloaded!\n\n" +
                                  $"File: {Path.GetFileName(saveFileDialog.FileName)}\n" +
                                  $"Location: {Path.GetDirectoryName(saveFileDialog.FileName)}\n" +
                                  $"Claims: {_currentReportClaims.Count}",
                                  "Download Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading report: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateReportCsv(List<Claim> claims)
        {
            var csv = new StringBuilder();

            // Add header
            csv.AppendLine("Claim ID,Title,Month/Year,Lecturer Email,Hours,Hourly Rate,Amount,Status,Submitted Date,Approved Date,Description");

            // Add data rows
            foreach (var claim in claims.OrderBy(c => c.SubmittedDate))
            {
                csv.AppendLine($"\"{claim.Id}\"," +
                             $"\"{EscapeCsvField(claim.Title)}\"," +
                             $"\"{claim.MonthYear}\"," +
                             $"\"{claim.UserEmail}\"," +
                             $"\"{claim.Hours}\"," +
                             $"\"{claim.HourlyRate}\"," +
                             $"\"{claim.Amount:N2}\"," +
                             $"\"{claim.Status}\"," +
                             $"\"{claim.SubmittedDate:yyyy-MM-dd}\"," +
                             $"\"{claim.ApprovedDate:yyyy-MM-dd}\"," +
                             $"\"{EscapeCsvField(claim.Description)}\"");
            }

            // Add summary section
            csv.AppendLine();
            csv.AppendLine("SUMMARY");
            csv.AppendLine($"Total Claims,{claims.Count}");
            csv.AppendLine($"Draft Claims,{claims.Count(c => c.Status == ClaimStatus.Draft)}");
            csv.AppendLine($"Submitted Claims,{claims.Count(c => c.Status == ClaimStatus.Submitted)}");
            csv.AppendLine($"Under Review Claims,{claims.Count(c => c.Status == ClaimStatus.UnderReview)}");
            csv.AppendLine($"Approved Claims,{claims.Count(c => c.Status == ClaimStatus.Approved)}");
            csv.AppendLine($"Paid Claims,{claims.Count(c => c.Status == ClaimStatus.Paid)}");
            csv.AppendLine($"Rejected Claims,{claims.Count(c => c.Status == ClaimStatus.Rejected)}");
            csv.AppendLine($"Total Amount,R{claims.Sum(c => c.Amount):N2}");
            csv.AppendLine($"Approved Amount,R{claims.Where(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Paid).Sum(c => c.Amount):N2}");

            return csv.ToString();
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            // Escape quotes and handle fields that contain commas or quotes
            if (field.Contains("\"") || field.Contains(",") || field.Contains("\n") || field.Contains("\r"))
            {
                return field.Replace("\"", "\"\"");
            }
            return field;
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

            try
            {
                // Create CSV content for approved claims
                string csvContent = "Lecturer Email,Full Name,Title,Amount,Approved Date,Hours,Hourly Rate\n";
                foreach (var claim in approvedClaims)
                {
                    var user = _dataService.GetUserByEmail(claim.UserEmail);
                    var fullName = user?.FullName ?? "Unknown";

                    csvContent += $"\"{claim.UserEmail}\"," +
                                $"\"{fullName}\"," +
                                $"\"{EscapeCsvField(claim.Title)}\"," +
                                $"\"R{claim.Amount:N2}\"," +
                                $"\"{claim.ApprovedDate:yyyy-MM-dd}\"," +
                                $"\"{claim.Hours}\"," +
                                $"\"{claim.HourlyRate}\"\n";
                }

                // Ask user where to save the file
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    FileName = $"Approved_Claims_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    DefaultExt = ".csv",
                    Title = "Export Approved Claims"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, csvContent, Encoding.UTF8);

                    MessageBox.Show($"Exported {approvedClaims.Count} approved claims successfully!\n\n" +
                                  $"File: {Path.GetFileName(saveFileDialog.FileName)}",
                                  "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting claims: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // The rest of your existing methods remain the same...
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