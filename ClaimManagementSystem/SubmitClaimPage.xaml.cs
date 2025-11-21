using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ClaimManagementSystem.Models;
using ClaimManagementSystem.Services;

namespace ClaimManagementSystem
{
    public partial class SubmitClaimPage : Page
    {
        private const decimal DefaultHourlyRate = 130.00m;
        private List<string> selectedFiles = new List<string>();
        private DataService _dataService = new DataService();

        // Control references to avoid null issues
        private TextBox? _txtHours;
        private TextBox? _txtHourlyRate;
        private TextBox? _txtTotalAmount;

        public SubmitClaimPage()
        {
            try
            {
                InitializeComponent();
                InitializeControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing page: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure controls are initialized after page is loaded
            InitializeControls();
        }

        private void InitializeControls()
        {
            try
            {
                // Store control references
                _txtHours = txtHours;
                _txtHourlyRate = txtHourlyRate;
                _txtTotalAmount = txtTotalAmount;

                // Set default date to current month
                if (dpMonthYear != null)
                    dpMonthYear.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                // Set default hourly rate
                if (_txtHourlyRate != null)
                    _txtHourlyRate.Text = DefaultHourlyRate.ToString("N2");

                // Set placeholder text
                if (txtTitle != null)
                {
                    txtTitle.Text = "e.g., November 2024 Teaching Hours";
                    txtTitle.GotFocus += (s, e) =>
                    {
                        if (txtTitle.Text == "e.g., November 2024 Teaching Hours")
                            txtTitle.Text = "";
                    };
                    txtTitle.LostFocus += (s, e) =>
                    {
                        if (string.IsNullOrWhiteSpace(txtTitle.Text))
                            txtTitle.Text = "e.g., November 2024 Teaching Hours";
                    };
                }

                if (txtDescription != null)
                {
                    txtDescription.Text = "Describe the work performed, courses taught, etc.";
                    txtDescription.GotFocus += (s, e) =>
                    {
                        if (txtDescription.Text == "Describe the work performed, courses taught, etc.")
                            txtDescription.Text = "";
                    };
                    txtDescription.LostFocus += (s, e) =>
                    {
                        if (string.IsNullOrWhiteSpace(txtDescription.Text))
                            txtDescription.Text = "Describe the work performed, courses taught, etc.";
                    };
                }

                // Calculate initial amount
                CalculateTotalAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing controls: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateTitleRealTime();
        }

        private void ValidateTitleRealTime()
        {
            if (txtTitle == null) return;

            string title = txtTitle.Text;

            if (string.IsNullOrWhiteSpace(title) || title == "e.g., November 2024 Teaching Hours")
            {
                txtTitle.BorderBrush = System.Windows.Media.Brushes.Red;
                txtTitle.ToolTip = "Title is required";
            }
            else if (title.Length < 5)
            {
                txtTitle.BorderBrush = System.Windows.Media.Brushes.Orange;
                txtTitle.ToolTip = "Title should be at least 5 characters";
            }
            else if (title.Length > 100)
            {
                txtTitle.BorderBrush = System.Windows.Media.Brushes.Orange;
                txtTitle.ToolTip = "Title should be less than 100 characters";
            }
            else
            {
                txtTitle.BorderBrush = System.Windows.Media.Brushes.Green;
                txtTitle.ToolTip = "Title is valid";
            }
        }

        private void txtHours_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotalAmount();
        }

        private void txtHourlyRate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotalAmount();
        }

        private void CalculateTotalAmount()
        {
            try
            {
                if (_txtHours == null || _txtHourlyRate == null || _txtTotalAmount == null)
                {
                    // Controls not initialized yet
                    return;
                }

                string hoursText = _txtHours.Text ?? "0";
                string rateText = _txtHourlyRate.Text ?? DefaultHourlyRate.ToString("N2");

                // Remove any non-numeric characters except decimal point
                hoursText = new string(hoursText.Where(c => char.IsDigit(c) || c == '.').ToArray());
                rateText = new string(rateText.Where(c => char.IsDigit(c) || c == '.').ToArray());

                if (decimal.TryParse(hoursText, out decimal hours) &&
                    decimal.TryParse(rateText, out decimal rate))
                {
                    if (hours > 0 && rate > 0)
                    {
                        decimal total = hours * rate;
                        _txtTotalAmount.Text = $"R{total:N2}";

                        // Validate inputs
                        ValidateHours(hours);
                        ValidateRate(rate);
                    }
                    else
                    {
                        _txtTotalAmount.Text = "R0.00";
                    }
                }
                else
                {
                    _txtTotalAmount.Text = "R0.00";
                }
            }
            catch (Exception ex)
            {
                // Silently handle calculation errors
                if (_txtTotalAmount != null)
                    _txtTotalAmount.Text = "R0.00";
            }
        }

        private void ValidateHours(decimal hours)
        {
            if (_txtHours == null) return;

            if (hours > 744) // Max hours in a month
            {
                ShowValidationError(_txtHours, "Hours cannot exceed 744 per month");
            }
            else if (hours < 0)
            {
                ShowValidationError(_txtHours, "Hours cannot be negative");
            }
            else
            {
                ClearValidationError(_txtHours);
            }
        }

        private void ValidateRate(decimal rate)
        {
            if (_txtHourlyRate == null) return;

            if (rate > 1000)
            {
                ShowValidationError(_txtHourlyRate, "Hourly rate seems unusually high");
            }
            else if (rate < 50)
            {
                ShowValidationError(_txtHourlyRate, "Hourly rate seems unusually low");
            }
            else
            {
                ClearValidationError(_txtHourlyRate);
            }
        }

        private void ShowValidationError(Control control, string message)
        {
            control.BorderBrush = System.Windows.Media.Brushes.Red;
            control.ToolTip = message;
        }

        private void ClearValidationError(Control control)
        {
            control.BorderBrush = System.Windows.Media.Brushes.Gray;
            control.ToolTip = null;
        }

        private void BtnChooseFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Filter = "Supported Files|*.pdf;*.doc;*.docx;*.jpg;*.jpeg;*.png",
                    Title = "Select Supporting Documents"
                };

                if (dialog.ShowDialog() == true)
                {
                    selectedFiles = dialog.FileNames.ToList();
                    if (lstFiles != null)
                    {
                        lstFiles.Items.Clear();
                        foreach (var file in selectedFiles)
                        {
                            lstFiles.Items.Add(Path.GetFileName(file));
                        }
                    }
                    MessageBox.Show($"{selectedFiles.Count} file(s) selected.", "Files Uploaded",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting files: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSaveDraft_Click(object sender, RoutedEventArgs e)
        {
            if (SaveClaim(ClaimStatus.Draft))
            {
                MessageBox.Show("Claim saved as draft successfully!", "Success",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
        }

        private void BtnSubmitClaim_Click(object sender, RoutedEventArgs e)
        {
            if (SaveClaim(ClaimStatus.Submitted))
            {
                MessageBox.Show("Claim submitted successfully!", "Success",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();

                // Navigate to My Claims page
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.NavigateToMyClaims(sender, e);
            }
        }

        private bool SaveClaim(ClaimStatus status)
        {
            if (!ValidateInput()) return false;

            try
            {
                var claim = new Claim
                {
                    Title = txtTitle?.Text ?? "Untitled Claim",
                    MonthYear = dpMonthYear?.SelectedDate?.ToString("MMM yyyy") ?? "Unknown",
                    Hours = decimal.Parse(_txtHours?.Text ?? "0"),
                    HourlyRate = decimal.Parse(_txtHourlyRate?.Text ?? DefaultHourlyRate.ToString("N2")),
                    Amount = decimal.Parse((_txtTotalAmount?.Text ?? "R0.00").Replace("R", "").Replace(",", "")),
                    Description = txtDescription?.Text ?? "",
                    Status = status,
                    SubmittedDate = status == ClaimStatus.Draft ? null : DateTime.Now,
                    UserEmail = MainWindow.CurrentUser?.Email ?? "unknown@university.com",
                    FilePaths = string.Join(";", selectedFiles)
                };

                // Save files to local folder (simulated)
                if (selectedFiles.Any())
                {
                    string claimFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UploadedFiles", Guid.NewGuid().ToString());
                    Directory.CreateDirectory(claimFolder);

                    foreach (var file in selectedFiles)
                    {
                        string fileName = Path.GetFileName(file);
                        string destPath = Path.Combine(claimFolder, fileName);
                        // File.Copy(file, destPath, overwrite: true); // Uncomment in real application
                    }

                    claim.FilePaths = claimFolder;
                }

                _dataService.AddClaim(claim);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving claim: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool ValidateInput()
        {
            // Validate Title
            if (txtTitle == null || string.IsNullOrWhiteSpace(txtTitle.Text) || txtTitle.Text == "e.g., November 2024 Teaching Hours")
            {
                MessageBox.Show("Please enter a claim title.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTitle?.Focus();
                return false;
            }

            // Validate Date
            if (dpMonthYear == null || dpMonthYear.SelectedDate == null)
            {
                MessageBox.Show("Please select a month and year.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                dpMonthYear?.Focus();
                return false;
            }

            // Validate Hours
            if (_txtHours == null || !decimal.TryParse(_txtHours.Text, out decimal hours) || hours <= 0)
            {
                MessageBox.Show("Please enter valid hours worked.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                _txtHours?.Focus();
                return false;
            }

            // Validate Hourly Rate
            if (_txtHourlyRate == null || !decimal.TryParse(_txtHourlyRate.Text, out decimal rate) || rate <= 0)
            {
                MessageBox.Show("Please enter a valid hourly rate.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                _txtHourlyRate?.Focus();
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            if (txtTitle != null)
                txtTitle.Text = "e.g., November 2024 Teaching Hours";

            if (dpMonthYear != null)
                dpMonthYear.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (_txtHours != null)
                _txtHours.Text = "0";

            if (_txtHourlyRate != null)
                _txtHourlyRate.Text = DefaultHourlyRate.ToString("N2");

            if (_txtTotalAmount != null)
                _txtTotalAmount.Text = "R0.00";

            if (txtDescription != null)
                txtDescription.Text = "Describe the work performed, courses taught, etc.";

            selectedFiles.Clear();
            if (lstFiles != null)
                lstFiles.Items.Clear();

            // Reset borders
            if (txtTitle != null) ClearValidationError(txtTitle);
            if (_txtHours != null) ClearValidationError(_txtHours);
            if (_txtHourlyRate != null) ClearValidationError(_txtHourlyRate);
        }
    }
}