using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.CustomerPages;

public partial class SettingsPage : Page
{
    private readonly string _accountNumber;

    public SettingsPage(string accountNumber)
    {
        InitializeComponent();
        _accountNumber = accountNumber;
        Loaded += Page_Loaded;
        LoadProfile();
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

    private void LoadProfile()
    {
        var dt = DatabaseHelper.GetCustomerProfile(_accountNumber);
        if (dt.Rows.Count > 0)
        {
            var row = dt.Rows[0];
            ProfileAccountNumber.Text = row["AccountNumber"]?.ToString() ?? "";
            ProfileFullName.Text = row["FullName"]?.ToString() ?? "";
            ProfileEmail.Text = row["Email"]?.ToString() ?? "";
            ProfilePhone.Text = row["Phone"]?.ToString() ?? "";
            ProfileCity.Text = row["City"]?.ToString() ?? "";
            ProfileAccountType.Text = row["AccountType"]?.ToString() ?? "";
        }
    }

    private void ChangePin_Click(object sender, RoutedEventArgs e)
    {
        MessageText.Visibility = Visibility.Collapsed;

        var oldPin = OldPinBox.Password.Trim();
        var newPin = NewPinBox.Password.Trim();
        var confirmPin = ConfirmPinBox.Password.Trim();

        if (string.IsNullOrWhiteSpace(oldPin))
        { ShowMessage("Please enter your current PIN.", false); return; }
        if (string.IsNullOrWhiteSpace(newPin) || newPin.Length != 4)
        { ShowMessage("New PIN must be exactly 4 digits.", false); return; }
        if (newPin != confirmPin)
        { ShowMessage("New PINs do not match.", false); return; }
        if (oldPin == newPin)
        { ShowMessage("New PIN must be different from current PIN.", false); return; }

        var (success, message) = DatabaseHelper.ChangePin(_accountNumber, oldPin, newPin);
        if (success)
        {
            ShowMessage("PIN changed successfully!", true);
            OldPinBox.Clear();
            NewPinBox.Clear();
            ConfirmPinBox.Clear();
        }
        else
        {
            ShowMessage(message, false);
        }
    }

    private void ShowMessage(string text, bool success)
    {
        MessageText.Text = text;
        MessageText.Foreground = success ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A")!) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")!);
        MessageText.Visibility = Visibility.Visible;
    }
}
