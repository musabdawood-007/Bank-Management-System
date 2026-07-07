using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BankManagementSystem.AdminPages;

namespace BankManagementSystem.Windows;

public partial class AdminShellWindow : Window
{
    public AdminShellWindow(string adminName)
    {
        InitializeComponent();
        AdminNameText.Text = adminName;
        Loaded += AdminShellWindow_Loaded;
        NavigateToPage("Dashboard");
    }

    private void AdminShellWindow_Loaded(object sender, RoutedEventArgs e)
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
            "Customers" => "Manage Customers",
            "Transactions" => "All Transactions",
            "Loans" => "Loan Management",
            "Fraud" => "Fraud Alerts",
            "Audit" => "Audit Log",
            "Analytics" => "Analytics",
            _ => page
        };

        ContentFrame.Content = page switch
        {
            "Dashboard" => new AdminDashboardPage(),
            "Customers" => new ManageCustomersPage(),
            "Transactions" => new AllTransactionsPage(),
            "Loans" => new LoanManagementPage(),
            "Fraud" => new FraudAlertsPage(),
            "Audit" => new AuditLogPage(),
            "Analytics" => new AnalyticsPage(),
            _ => new AdminDashboardPage()
        };
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var win = new WelcomeWindow();
        win.Show();
        Close();
    }
}
