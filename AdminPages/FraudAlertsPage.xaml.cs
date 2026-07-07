using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.AdminPages;

public partial class FraudAlertsPage : Page
{
    private int _selectedAlertId;

    public FraudAlertsPage()
    {
        InitializeComponent();
        Loaded += Page_Loaded;
        LoadData();
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

    private void LoadData()
    {
        FraudGrid.ItemsSource = DatabaseHelper.GetFraudFlags().DefaultView;
    }

    private void FraudGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FraudGrid.SelectedItem is DataRowView row)
        {
            _selectedAlertId = Convert.ToInt32(row["AlertId"]);
            var status = row["Status"]?.ToString() ?? "";

            if (status == "Open")
            {
                ResolvePanel.Visibility = Visibility.Visible;
                DetailType.Text = row["AlertType"]?.ToString() ?? "";
                DetailDescription.Text = row["Description"]?.ToString() ?? "";
                ResolveNotesBox.Text = "";
            }
            else
            {
                ResolvePanel.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            ResolvePanel.Visibility = Visibility.Collapsed;
        }
    }

    private void Resolve_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            $"Resolve fraud alert #{_selectedAlertId}?",
            "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            DatabaseHelper.ResolveFraudAlert(_selectedAlertId, ResolveNotesBox.Text.Trim());
            LoadData();
            ResolvePanel.Visibility = Visibility.Collapsed;
            MessageBox.Show("Alert resolved.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        ResolvePanel.Visibility = Visibility.Collapsed;
        FraudGrid.UnselectAll();
    }
}
