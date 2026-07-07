using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.Windows;

public partial class AdminLoginWindow : Window
{
    public AdminLoginWindow()
    {
        InitializeComponent();
        Loaded += AdminLoginWindow_Loaded;
    }

    private void AdminLoginWindow_Loaded(object sender, RoutedEventArgs e)
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

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        var email = EmailBox.Text.Trim();
        var password = PasswordBox.Password.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorText.Text = "Please enter both email and password.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        var (success, message) = DatabaseHelper.ValidateAdminLogin(email, password);
        if (success)
        {
            var shell = new AdminShellWindow(message);
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
