using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.CustomerPages;

public partial class DepositPage : Page
{
    private readonly string _accountNumber;

    public DepositPage(string accountNumber)
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

    private void Deposit_Click(object sender, RoutedEventArgs e)
    {
        MessageText.Visibility = Visibility.Collapsed;

        if (!decimal.TryParse(AmountBox.Text.Trim(), out var amount) || amount <= 0)
        {
            ShowMessage("Please enter a valid amount greater than 0.", false);
            return;
        }

        var success = DatabaseHelper.Deposit(_accountNumber, amount, DescriptionBox.Text.Trim());
        if (success)
        {
            ShowMessage($"Successfully deposited PKR {amount:N0}.", true);
            AmountBox.Clear();
            DescriptionBox.Clear();
            LoadBalance();
        }
        else
        {
            ShowMessage("Deposit failed. Please try again.", false);
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
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
