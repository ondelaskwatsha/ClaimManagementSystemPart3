using System.Linq;
using System.Windows.Controls;
using ClaimManagementSystem.Services;

namespace ClaimManagementSystem
{
    public partial class MyClaimsPage : Page
    {
        private DataService _dataService = new DataService();

        public MyClaimsPage()
        {
            InitializeComponent();
            LoadClaims();
        }

        private void LoadClaims()
        {
            if (MainWindow.CurrentUser == null) return;

            var claims = _dataService.GetClaimsByUser(MainWindow.CurrentUser.Email);
            dgClaims.ItemsSource = claims;

            // Update summary
            txtSummary.Text = $"Total Claims: {claims.Count} | " +
                            $"Total Hours: {claims.Sum(c => c.Hours)}h | " +
                            $"Total Amount: R{claims.Sum(c => c.Amount):N0}";
        }
    }
}