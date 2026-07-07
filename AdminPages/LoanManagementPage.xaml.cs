using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.AdminPages;

public partial class LoanManagementPage : Page
{
    private int _selectedLoanAppId;

    public LoanManagementPage()
    {
        InitializeComponent();
        Loaded += Page_Loaded;
        LoadData();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var slideIn = new DoubleAnimation(20, 0, TimeSpan.FromSeconds(0.4))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        PageRoot.BeginAnimation(FrameworkElement.OpacityProperty, fadeIn);
        PageRoot.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideIn);
    }

    private void LoadData()
    {
        PendingText.Text = DatabaseHelper.GetLoanCountByStatus("Pending").ToString();
        ApprovedText.Text = DatabaseHelper.GetLoanCountByStatus("Approved").ToString();
        RejectedText.Text = DatabaseHelper.GetLoanCountByStatus("Rejected").ToString();
        LoansGrid.ItemsSource = DatabaseHelper.GetAllLoanApplications().DefaultView;
    }

    private void LoansGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LoansGrid.SelectedItem is DataRowView row)
        {
            _selectedLoanAppId = Convert.ToInt32(row["LoanAppId"]);
            DetailPanel.Visibility = Visibility.Visible;
            DetailCustomer.Text = row["CustomerName"]?.ToString() ?? "";
            DetailAmount.Text = $"PKR {Convert.ToDecimal(row["LoanAmount"]):N0}";
            DetailPurpose.Text = row["Purpose"]?.ToString() ?? "";
            var status = row["Status"]?.ToString() ?? "";

            DetailStatus.Text = status;

            if (status == "Pending")
            {
                ApproveSection.Visibility = Visibility.Visible;
                RejectSection.Visibility = Visibility.Visible;
                ApprovedAmountBox.Text = row["LoanAmount"]?.ToString() ?? "";
                InterestRateBox.Text = "12.5";
                ApproveNotesBox.Text = "";
                RejectNotesBox.Text = "";
            }
            else
            {
                ApproveSection.Visibility = Visibility.Collapsed;
                RejectSection.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            DetailPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void Approve_Click(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(ApprovedAmountBox.Text.Trim(), out var approvedAmount))
        { MessageBox.Show("Please enter a valid approved amount.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        if (!decimal.TryParse(InterestRateBox.Text.Trim(), out var interestRate))
        { MessageBox.Show("Please enter a valid interest rate.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        var result = MessageBox.Show(
            $"Approve loan #{_selectedLoanAppId} for PKR {approvedAmount:N0} at {interestRate}%?",
            "Confirm Approval", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            DatabaseHelper.ApproveLoan(_selectedLoanAppId, approvedAmount, interestRate, ApproveNotesBox.Text.Trim());
            LoadData();
            DetailPanel.Visibility = Visibility.Collapsed;
            MessageBox.Show("Loan approved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Reject_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            $"Reject loan #{_selectedLoanAppId}?",
            "Confirm Rejection", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            DatabaseHelper.RejectLoan(_selectedLoanAppId, RejectNotesBox.Text.Trim());
            LoadData();
            DetailPanel.Visibility = Visibility.Collapsed;
            MessageBox.Show("Loan rejected.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
