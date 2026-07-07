using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BankManagementSystem.Helpers;
using System.Collections.Generic;

namespace BankManagementSystem.Windows;

public partial class CustomerRegistrationWindow : Window
{
    private readonly Dictionary<string, string[]> _cityAreas = new()
    {
        ["Karachi"] = new[] { "Clifton", "DHA", "Gulshan-e-Iqbal", "Korangi", "North Nazimabad", "Saddar", "Bahadurabad", "PECHS", "Landhi", "Malir", "SITE", "Liaquatabad" },
        ["Lahore"] = new[] { "Gulberg", "DHA", "Model Town", "Johar Town", "Cantt", "Garden Town", "Faisal Town", "Iqbal Town", "Samanabad", "Baghbanpura" },
        ["Islamabad"] = new[] { "F-Sector", "G-Sector", "I-Sector", "H-Sector", "Bahria Town", "DHA", "Blue Area", "Saidpur", "Margalla Town" },
        ["Rawalpindi"] = new[] { "Saddar", "Cantt", "Westridge", "R.A. Bazaar", "Commercial Market", "Bahria Town", "DHA" },
        ["Faisalabad"] = new[] { "Ghulam Muhammad Abad", "Madina Town", "Samanabad", "D-Type Colony", "Peoples Colony", "Civil Lines" },
        ["Peshawar"] = new[] { "University Town", "Cantt", "Hayatabad", "Saddar", "City Center", "Dalazak Road" },
        ["Multan"] = new[] { "Cantt", "Shah Rukn-e-Alam", "Bosan Road", "Gulgasht", "Nishtar Road", "Old Shujabad Road" },
        ["Quetta"] = new[] { "Cantt", "Jinnah Road", "Sariab Road", "Zarghoon Road", "Airport Road", "Brewery Road" },
        ["Sialkot"] = new[] { "Cantt", "Sadar Bazaar", "Kashmir Road", "Paris Road", "Wazirabad Road", "Sambrial" },
        ["Gujranwala"] = new[] { "Cantt", "Model Town", "Satellite Town", "Civil Lines", "Trust Plaza Road" },
        ["Hyderabad"] = new[] { "Latifabad", "Qasimabad", "Cantt", "Saddar", "Hirabad", "Resham Gali" },
        ["Abbottabad"] = new[] { "Cantt", "Supply Bazaar", "Mandian", "Nawanshehr", "Karak" },
        ["Bahawalpur"] = new[] { "Cantt", "Model Town", "Satellite Town", "Ahmedpur East Road", "Circular Road" },
        ["Sargodha"] = new[] { "Cantt", "Satellite Town", "University Road", "Sillanwali Road", "Lahore Road" },
        ["Sukkur"] = new[] { "Cantt", "New Sukkur", "Old Sukkur", "Shikarpur Road", "Airport Road" }
    };

    public CustomerRegistrationWindow()
    {
        InitializeComponent();
        Loaded += CustomerRegistrationWindow_Loaded;
        LoadCities();
    }

    private void CustomerRegistrationWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var story = (Storyboard)FindResource("EntranceAnimation");
        story.Begin(this);
    }

    private void LoadCities()
    {
        CityCombo.Items.Clear();
        foreach (var city in _cityAreas.Keys)
            CityCombo.Items.Add(city);
    }

    private void CityCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AreaCombo.Items.Clear();
        if (CityCombo.SelectedItem is string city && _cityAreas.ContainsKey(city))
        {
            foreach (var area in _cityAreas[city])
                AreaCombo.Items.Add(area);
            AreaCombo.IsEnabled = true;
            AreaCombo.SelectedIndex = 0;
        }
        else
        {
            AreaCombo.IsEnabled = false;
        }
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

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        SuccessPanel.Visibility = Visibility.Collapsed;

        var fullName = FullNameBox.Text.Trim();
        var email = EmailBox.Text.Trim();
        var phone = PhoneBox.Text.Trim();
        var cnic = CnicBox.Text.Trim();
        var dob = DobPicker.SelectedDate;
        var city = CityCombo.SelectedItem as string;
        var area = AreaCombo.SelectedItem as string;
        var accountType = (AccountTypeCombo.SelectedItem as ComboBoxItem)?.Content as string;
        var pin = PinBox.Password.Trim();
        var confirmPin = ConfirmPinBox.Password.Trim();

        // Validation
        if (string.IsNullOrWhiteSpace(fullName))
        { ShowError("Full name is required."); return; }
        if (string.IsNullOrWhiteSpace(email))
        { ShowError("Email is required."); return; }
        if (string.IsNullOrWhiteSpace(phone))
        { ShowError("Phone number is required."); return; }
        if (string.IsNullOrWhiteSpace(cnic) || cnic.Length < 13)
        { ShowError("Valid CNIC is required (13 digits)."); return; }
        if (dob == null)
        { ShowError("Date of birth is required."); return; }
        if (string.IsNullOrWhiteSpace(city))
        { ShowError("City is required."); return; }
        if (string.IsNullOrWhiteSpace(area))
        { ShowError("Area is required."); return; }
        if (string.IsNullOrWhiteSpace(accountType))
        { ShowError("Account type is required."); return; }
        if (string.IsNullOrWhiteSpace(pin) || pin.Length != 4)
        { ShowError("PIN must be exactly 4 digits."); return; }
        if (pin != confirmPin)
        { ShowError("PINs do not match."); return; }

        var result = DatabaseHelper.RegisterCustomer(fullName, email, phone, cnic, dob.Value, city, area, accountType, pin);

        if (result.StartsWith("Error:"))
        {
            ShowError(result.Substring(6));
        }
        else
        {
            AccountNumberText.Text = $"Your Account Number: {result}";
            SuccessPanel.Visibility = Visibility.Visible;
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        FullNameBox.Clear();
        EmailBox.Clear();
        PhoneBox.Clear();
        CnicBox.Clear();
        DobPicker.SelectedDate = null;
        CityCombo.SelectedIndex = -1;
        AreaCombo.Items.Clear();
        AreaCombo.IsEnabled = false;
        AccountTypeCombo.SelectedIndex = -1;
        PinBox.Clear();
        ConfirmPinBox.Clear();
        ErrorText.Visibility = Visibility.Collapsed;
        SuccessPanel.Visibility = Visibility.Collapsed;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        var win = new WelcomeWindow();
        win.Show();
        Close();
    }
}
