using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.Windows;

public partial class CustomerLoginWindow : Window
{
    private string _pinValue = string.Empty;

    public CustomerLoginWindow()
    {
        InitializeComponent();
        Loaded += CustomerLoginWindow_Loaded;
    }

    private void CustomerLoginWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var story = (Storyboard)FindResource("CardEntranceAnimation");
        story.Begin(this);
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void Close_Click(object sender, RoutedEventArgs e)
        => Application.Current.Shutdown();

    private void ShowPin_Changed(object sender, RoutedEventArgs e)
    {
        // Note: WPF PasswordBox doesn't natively support showing characters.
        // The checkbox toggling is handled by reading/writing the Password property.
        // For a simple approach, we just note the user's preference.
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        var accountNumber = AccountBox.Text.Trim();
        var pin = PinBox.Password.Trim();

        if (string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(pin))
        {
            ErrorText.Text = "Please enter both account number and PIN.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        var (success, message) = DatabaseHelper.ValidateCustomerLogin(accountNumber, pin);
        if (success)
        {
            var shell = new CustomerShellWindow(accountNumber);
            shell.Show();
            Close();
        }
        else
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        var win = new WelcomeWindow();
        win.Show();
        Close();
    }
}
