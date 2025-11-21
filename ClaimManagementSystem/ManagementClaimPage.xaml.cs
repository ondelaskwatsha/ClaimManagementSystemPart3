using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClaimManagementSystem.Models;
using ClaimManagementSystem.Services;

namespace ClaimManagementSystem
{
    public partial class ManageClaimsPage : Page
    {
        private DataService _dataService = new DataService();
        private UserRole _userRole;

        public ManageClaimsPage(UserRole userRole)
        {
            InitializeComponent();
            _userRole = userRole;
            LoadAllClaims();
        }

        public ManageClaimsPage()
        {
            // Default constructor for XAML
            InitializeComponent();
            _dataService = new DataService();
            LoadAllClaims();
        }

        private void LoadAllClaims()
        {
            var allClaims = _dataService.GetAllClaims();
            dgAllClaims.ItemsSource = allClaims;

            // Update summary
            txtSummary.Text = $"Total Claims: {allClaims.Count} | " +
                            $"Total Hours: {allClaims.Sum(c => c.Hours)}h | " +
                            $"Total Amount: R{allClaims.Sum(c => c.Amount):N0}";
        }

        private void BtnReview_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var claim = button?.DataContext as Claim;
            if (claim != null)
            {
                MessageBox.Show($"Reviewing claim: {claim.Title}", "Review Claim",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var claim = button?.DataContext as Claim;
            if (claim != null && MainWindow.CurrentUser != null)
            {
                claim.Status = ClaimStatus.Approved;
                claim.ApprovedBy = MainWindow.CurrentUser.Email;
                claim.ApprovedDate = DateTime.Now;
                _dataService.UpdateClaim(claim);
                LoadAllClaims(); // Refresh the grid
                MessageBox.Show($"Claim '{claim.Title}' approved!", "Success",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Clear session
            MainWindow.CurrentUser = null;

            // Navigate back to login page
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.NavigateToLogin();
        }
    }
}