using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BankManagementSystem.CustomerPages;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.Windows;

public partial class CustomerShellWindow : Window
{
    private readonly string _accountNumber;

    public CustomerShellWindow(string accountNumber)
    {
        InitializeComponent();
        _accountNumber = accountNumber;
        CustomerNameText.Text = DatabaseHelper.GetCustomerName(accountNumber);
        AccountNumberText.Text = accountNumber;
        Loaded += CustomerShellWindow_Loaded;
        NavigateToPage("Dashboard");
    }

    private void CustomerShellWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Sidebar slide-in animation
        var sidebarOpacity = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var sidebarSlide = new DoubleAnimation(-240, 0, TimeSpan.FromSeconds(0.5))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        SidebarBorder.BeginAnimation(FrameworkElement.OpacityProperty, sidebarOpacity);
        SidebarBorder.RenderTransform.BeginAnimation(TranslateTransform.XProperty, sidebarSlide);
    }

    private void ContentFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
    {
        // Content frame fade-in on navigation
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        ContentFrame.BeginAnimation(FrameworkElement.OpacityProperty, fadeIn);
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

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string tag)
            NavigateToPage(tag);
    }

    private void NavigateToPage(string page)
    {
        PageTitle.Text = page switch
        {
            "Dashboard" => "Dashboard",
            "Deposit" => "Deposit",
            "Withdraw" => "Withdraw",
            "Transfer" => "Transfer",
            "Transactions" => "Transaction History",
            "Loan" => "Loan Request",
            "Settings" => "Settings",
            _ => page
        };

        ContentFrame.Content = page switch
        {
            "Dashboard" => new DashboardPage(_accountNumber),
            "Deposit" => new DepositPage(_accountNumber),
            "Withdraw" => new WithdrawPage(_accountNumber),
            "Transfer" => new TransferPage(_accountNumber),
            "Transactions" => new TransactionHistoryPage(_accountNumber),
            "Loan" => new LoanRequestPage(_accountNumber),
            "Settings" => new SettingsPage(_accountNumber),
            _ => new DashboardPage(_accountNumber)
        };
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var win = new WelcomeWindow();
        win.Show();
        Close();
    }
}
