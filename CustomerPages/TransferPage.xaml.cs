using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.CustomerPages;

public partial class TransferPage : Page
{
    private readonly string _accountNumber;

    public TransferPage(string accountNumber)
    {
        InitializeComponent();
        _accountNumber = accountNumber;
        Loaded += Page_Loaded;
        LoadBalance();
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

    private void LoadBalance()
    {
        BalanceText.Text = $"PKR {DatabaseHelper.GetBalance(_accountNumber):N0}";
    }

    private void Transfer_Click(object sender, RoutedEventArgs e)
    {
        MessageText.Visibility = Visibility.Collapsed;

        var targetAccount = TargetAccountBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(targetAccount))
        { ShowMessage("Please enter a target account number.", false); return; }
        if (targetAccount == _accountNumber)
        { ShowMessage("Cannot transfer to your own account.", false); return; }
        if (!decimal.TryParse(AmountBox.Text.Trim(), out var amount) || amount <= 0)
        { ShowMessage("Please enter a valid amount greater than 0.", false); return; }

        var (success, message) = DatabaseHelper.Transfer(_accountNumber, targetAccount, amount, DescriptionBox.Text.Trim());
        if (success)
        {
            ShowMessage($"Successfully transferred PKR {amount:N0} to {targetAccount}.", true);
            TargetAccountBox.Clear();
            AmountBox.Clear();
            DescriptionBox.Clear();
            LoadBalance();
        }
        else
        {
            ShowMessage(message, false);
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        TargetAccountBox.Clear();
        AmountBox.Clear();
        DescriptionBox.Clear();
        MessageText.Visibility = Visibility.Collapsed;
    }

    private void ShowMessage(string text, bool success)
    {
        MessageText.Text = text;
        MessageText.Foreground = success ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A")!) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")!);
        MessageText.Visibility = Visibility.Visible;
    }
}
