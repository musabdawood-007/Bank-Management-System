using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.CustomerPages;

public partial class LoanRequestPage : Page
{
    private readonly string _accountNumber;

    public LoanRequestPage(string accountNumber)
    {
        InitializeComponent();
        _accountNumber = accountNumber;
        Loaded += Page_Loaded;
        LoadLoans();
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

    private void LoadLoans()
    {
        LoansGrid.ItemsSource = DatabaseHelper.GetMyLoans(_accountNumber).DefaultView;
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        MessageText.Visibility = Visibility.Collapsed;

        var loanType = (LoanTypeCombo.SelectedItem as ComboBoxItem)?.Content as string;
        if (string.IsNullOrWhiteSpace(loanType))
        { ShowMessage("Please select a loan type.", false); return; }

        if (!decimal.TryParse(LoanAmountBox.Text.Trim(), out var amount) || amount <= 0)
        { ShowMessage("Please enter a valid loan amount.", false); return; }

        var tenureStr = (TenureCombo.SelectedItem as ComboBoxItem)?.Content as string;
        if (string.IsNullOrWhiteSpace(tenureStr) || !int.TryParse(tenureStr, out var tenure))
        { ShowMessage("Please select a loan tenure.", false); return; }

        var (success, message) = DatabaseHelper.SubmitLoanRequest(_accountNumber, amount, loanType, tenure);
        if (success)
        {
            ShowMessage("Loan application submitted successfully!", true);
            LoanTypeCombo.SelectedIndex = -1;
            LoanAmountBox.Clear();
            TenureCombo.SelectedIndex = -1;
            LoadLoans();
        }
        else
        {
            ShowMessage(message, false);
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        LoanTypeCombo.SelectedIndex = -1;
        LoanAmountBox.Clear();
        TenureCombo.SelectedIndex = -1;
        MessageText.Visibility = Visibility.Collapsed;
    }

    private void ShowMessage(string text, bool success)
    {
        MessageText.Text = text;
        MessageText.Foreground = success ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A")!) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")!);
        MessageText.Visibility = Visibility.Visible;
    }
}
