using System.Windows;
using System.Windows.Controls;
using System.Linq;
using ClaimManagementSystem.Services;

namespace ClaimManagementSystem
{
    public partial class OverviewPage : Page
    {
        private DataService _dataService = new DataService();

        public OverviewPage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            var user = MainWindow.CurrentUser;
            if (user == null) return;

            txtWelcome.Text = $"Welcome back, {user.FullName}!\n{user.Department} - {user.Role}";

            var claims = _dataService.GetClaimsByUser(user.Email);

            txtTotalClaims.Text = claims.Count.ToString();
            txtPending.Text = claims.Count(c => c.Status == Models.ClaimStatus.Submitted ||
                                              c.Status == Models.ClaimStatus.UnderReview).ToString();
            txtApproved.Text = claims.Count(c => c.Status == Models.ClaimStatus.Approved ||
                                               c.Status == Models.ClaimStatus.Paid).ToString();
            txtTotalAmount.Text = $"R{claims.Sum(c => c.Amount):N0}";
        }

        private void BtnNewClaim_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToSubmitClaim(sender, e);
        }

        private void BtnViewClaims_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToMyClaims(sender, e);
        }
    }
}