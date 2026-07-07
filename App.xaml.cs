using BankManagementSystem.Helpers;

namespace BankManagementSystem;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DatabaseHelper.ConnectionString = "Server=.;Database=BankManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";
    }
}
