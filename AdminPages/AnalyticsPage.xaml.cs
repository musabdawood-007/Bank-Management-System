using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.AdminPages;

public partial class AnalyticsPage : Page
{
    public AnalyticsPage()
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
        var totalBalance = DatabaseHelper.GetTotalBalance();
        var totalDeposits = DatabaseHelper.GetTotalDeposits();
        var totalWithdrawals = DatabaseHelper.GetTotalWithdrawals();
        var loansDisbursed = DatabaseHelper.GetTotalLoansDisbursed();
        var activeLoans = DatabaseHelper.GetActiveLoanCount();
        var activeAccounts = DatabaseHelper.GetTotalAccounts();
        var totalTransactions = DatabaseHelper.GetTotalTransactions();

        TotalBalanceText.Text = $"PKR {totalBalance:N0}";
        TotalDepositsText.Text = $"PKR {totalDeposits:N0}";
        TotalWithdrawalsText.Text = $"PKR {totalWithdrawals:N0}";
        LoansDisbursedText.Text = $"PKR {loansDisbursed:N0}";
        ActiveLoansText.Text = activeLoans.ToString();
        ActiveAccountsText.Text = activeAccounts.ToString();
        TotalTransactionsText.Text = totalTransactions.ToString();

        SummaryText.Text = $"The banking system currently holds a total balance of PKR {totalBalance:N0} across {activeAccounts} active accounts. " +
                           $"Total deposits amount to PKR {totalDeposits:N0}, while total withdrawals stand at PKR {totalWithdrawals:N0}. " +
                           $"A total of PKR {loansDisbursed:N0} has been disbursed across {activeLoans} active loans. " +
                           $"The system has processed {totalTransactions} transactions to date.";
    }
}
