using ClaimManagementSystem.Models;
using ClaimManagementSystem.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClaimManagementSystem
{
    public partial class RegisterPage : Page
    {
        private MainWindow mainWindow;

        public RegisterPage(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
        }

        private void BtnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            UserRole role = cmbRole.SelectedIndex switch
            {
                1 => UserRole.Lecturer,
                2 => UserRole.ProgramCoordinator,
                3 => UserRole.AcademicManager,
                4 => UserRole.HRManager, // ADD THIS CASE
                _ => throw new InvalidOperationException("Please select a valid role")
            };

            if (ValidateRegistration())
            {
                try
                {
                    // Create new user
                    var newUser = new User
                    {
                        FullName = txtFullName.Text.Trim(),
                        Email = txtEmail.Text.Trim().ToLower(),
                        Role = GetSelectedRole(),
                        Department = GetSelectedDepartment(),
                        Password = txtPassword.Password
                    };

                    // Save user
                    UserStore.AddUser(newUser);

                    MessageBox.Show("Account created successfully! You can now sign in.", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    // Navigate back to login
                    mainWindow.MainFrame.Navigate(new LoginPage(mainWindow));
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Registration Failed",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateRegistration()
        {
            // Validate Full Name
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || txtFullName.Text == "Enter your full name")
            {
                MessageBox.Show("Please enter your full name.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return false;
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || txtEmail.Text == "Enter your email")
            {
                MessageBox.Show("Please enter your email address.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            // Validate Role
            if (cmbRole.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select your role.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbRole.Focus();
                return false;
            }

            // Validate Department
            if (cmbDepartment.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select your department.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbDepartment.Focus();
                return false;
            }

            // Validate Password
            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Please enter a password.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return false;
            }

            if (txtPassword.Password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return false;
            }

            // Validate Confirm Password
            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Passwords do not match. Please confirm your password.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtConfirmPassword.Focus();
                return false;
            }

            return true;
        }

        private UserRole GetSelectedRole()
        {
            if (cmbRole.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString() switch
                {
                    "Lecturer" => UserRole.Lecturer,
                    "Program Coordinator" => UserRole.ProgramCoordinator,
                    "Academic Manager" => UserRole.AcademicManager,
                    _ => UserRole.Lecturer
                };
            }
            return UserRole.Lecturer;
        }

        private string GetSelectedDepartment()
        {
            if (cmbDepartment.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString();
            }
            return "Unknown Department";
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == "Enter your full name" || textBox.Text == "Enter your email")
                {
                    textBox.Text = "";
                    textBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (textBox == txtFullName)
                        textBox.Text = "Enter your full name";
                    else if (textBox == txtEmail)
                        textBox.Text = "Enter your email";

                    textBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
            }
        }

        private void TextBlock_SignIn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.MainFrame.Navigate(new LoginPage(mainWindow));
        }
    }
}