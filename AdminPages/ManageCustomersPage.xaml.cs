using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;

namespace BankManagementSystem.AdminPages;

public partial class ManageCustomersPage : Page
{
    public ManageCustomersPage()
    {
        InitializeComponent();
        Loaded += Page_Loaded;
        LoadCustomers();
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

    private void LoadCustomers(string search = "")
    {
        CustomersGrid.ItemsSource = DatabaseHelper.GetAllCustomers(search).DefaultView;
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        LoadCustomers(SearchBox.Text.Trim());
    }

    private string? GetSelectedAccountNumber()
    {
        if (CustomersGrid.SelectedItem is DataRowView row)
            return row["AccountNumber"]?.ToString();
        return null;
    }

    private void CustomersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Selection tracking
    }

    private void Activate_Click(object sender, RoutedEventArgs e)
    {
        var account = GetSelectedAccountNumber();
        if (account == null)
        { MessageBox.Show("Please select a customer first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        var result = MessageBox.Show($"Activate account {account}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            DatabaseHelper.UpdateAccountStatus(account, "Active");
            LoadCustomers(SearchBox.Text.Trim());
            MessageBox.Show("Account activated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Freeze_Click(object sender, RoutedEventArgs e)
    {
        var account = GetSelectedAccountNumber();
        if (account == null)
        { MessageBox.Show("Please select a customer first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        var result = MessageBox.Show($"Freeze account {account}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            DatabaseHelper.UpdateAccountStatus(account, "Frozen");
            LoadCustomers(SearchBox.Text.Trim());
            MessageBox.Show("Account frozen.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        var account = GetSelectedAccountNumber();
        if (account == null)
        { MessageBox.Show("Please select a customer first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

        var result = MessageBox.Show($"Close account {account}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            DatabaseHelper.UpdateAccountStatus(account, "Closed");
            LoadCustomers(SearchBox.Text.Trim());
            MessageBox.Show("Account closed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
