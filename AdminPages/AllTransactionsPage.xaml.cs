using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.AdminPages;

public partial class AllTransactionsPage : Page
{
    public AllTransactionsPage()
    {
        InitializeComponent();
        Loaded += Page_Loaded;
        TransactionsGrid.ItemsSource = DatabaseHelper.GetAllTransactions(500).DefaultView;
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
}
