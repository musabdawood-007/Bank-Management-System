using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.AdminPages;

public partial class AdminDashboardPage : Page
{
    public AdminDashboardPage()
    {
        InitializeComponent();
        Loaded += Page_Loaded;
        LoadData();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Page entrance animation
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

        // Staggered stat card animations
        var statCards = new[] { StatCard1, StatCard2, StatCard3, StatCard4, StatCard5 };
        for (int i = 0; i < statCards.Length; i++)
        {
            var delay = TimeSpan.FromMilliseconds(i * 100 + 200);
            var cardFade = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4))
            {
                BeginTime = delay,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var cardScaleX = new DoubleAnimation(0.9, 1, TimeSpan.FromSeconds(0.4))
            {
                BeginTime = delay,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var cardScaleY = new DoubleAnimation(0.9, 1, TimeSpan.FromSeconds(0.4))
            {
                BeginTime = delay,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            statCards[i].BeginAnimation(FrameworkElement.OpacityProperty, cardFade);
            ((ScaleTransform)statCards[i].RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, cardScaleX);
            ((ScaleTransform)statCards[i].RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, cardScaleY);
        }
    }

    private void LoadData()
    {
        TotalCustomersText.Text = DatabaseHelper.GetTotalCustomers().ToString();
        TotalDepositsText.Text = $"PKR {DatabaseHelper.GetTotalDeposits():N0}";
        PendingLoansText.Text = DatabaseHelper.GetLoanCountByStatus("Pending").ToString();
        FraudAlertsText.Text = DatabaseHelper.GetOpenFraudFlags().ToString();
        TotalTransactionsText.Text = DatabaseHelper.GetTotalTransactions().ToString();

        RecentGrid.ItemsSource = DatabaseHelper.GetAllTransactions(20).DefaultView;
    }
}
