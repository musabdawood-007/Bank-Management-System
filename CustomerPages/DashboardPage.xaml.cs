using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.CustomerPages;

public partial class DashboardPage : Page
{
    private readonly string _accountNumber;

    public DashboardPage(string accountNumber)
    {
        InitializeComponent();
        _accountNumber = accountNumber;
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
        var statCards = new[] { StatCard1, StatCard2, StatCard3, StatCard4 };
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

    public void RefreshData() => LoadData();

    private void LoadData()
    {
        BalanceText.Text = $"PKR {DatabaseHelper.GetBalance(_accountNumber):N0}";
        AccountTypeText.Text = DatabaseHelper.GetAccountType(_accountNumber);
        StatusText.Text = DatabaseHelper.GetAccountStatus(_accountNumber);

        var transactions = DatabaseHelper.GetTransactionHistory(_accountNumber, 100);
        TransactionCountText.Text = transactions.Rows.Count.ToString();
        TransactionsGrid.ItemsSource = transactions.DefaultView;

        LoansGrid.ItemsSource = DatabaseHelper.GetMyLoans(_accountNumber).DefaultView;
    }
}
