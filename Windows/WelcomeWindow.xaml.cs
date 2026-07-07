using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace BankManagementSystem.Windows;

public partial class WelcomeWindow : Window
{
    public WelcomeWindow()
    {
        InitializeComponent();
        Loaded += WelcomeWindow_Loaded;
    }

    private void WelcomeWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Play staggered entrance animations
        ((Storyboard)FindResource("IconEntrance")).Begin(this);
        ((Storyboard)FindResource("TitleEntrance")).Begin(this);
        ((Storyboard)FindResource("SubtitleEntrance")).Begin(this);
        ((Storyboard)FindResource("ButtonsEntrance")).Begin(this);
        ((Storyboard)FindResource("RegisterEntrance")).Begin(this);

        // Start continuous animations after a short delay
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = System.TimeSpan.FromSeconds(1.2) };
        timer.Tick += (s, args) =>
        {
            timer.Stop();
            ((Storyboard)FindResource("FloatAnimation")).Begin(this);
            ((Storyboard)FindResource("TitleShimmerAnimation")).Begin(this);
        };
        timer.Start();
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

    private void CustomerLogin_Click(object sender, RoutedEventArgs e)
    {
        var win = new CustomerLoginWindow();
        win.Show();
        Close();
    }

    private void AdminLogin_Click(object sender, RoutedEventArgs e)
    {
        var win = new AdminLoginWindow();
        win.Show();
        Close();
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        var win = new CustomerRegistrationWindow();
        win.Show();
        Close();
    }
}
