# Bank Management System (BMS)

A WPF desktop banking application built with **.NET 8 (C#)** and **Microsoft SQL Server**, designed as an OOP + Databases project that simulates real-world banking workflows for both customers and administrators. All data access goes through parameterized stored procedures, and every admin action is recorded in a **SHA-256-signed audit log** for tamper evidence.

---

## Features

### Customer Portal
- Secure login with account number + 4-digit PIN
- Dashboard showing current balance and recent activity
- Deposit and withdraw funds (with balance / status validation)
- Transfer money between accounts
- Apply for loans (amount, purpose, tenure)
- View full transaction history with reference numbers
- Change PIN, view profile

### Admin Portal
- Admin login with email + password
- Dashboard with KPIs (total customers, deposits, withdrawals, pending loans, fraud alerts)
- Manage customers — search, freeze / activate accounts
- Approve or reject loan applications (with approved amount + interest rate)
- Monitor and resolve fraud alerts
- Browse all transactions across the bank
- Review the tamper-evident audit log

### Security
- All database access goes through parameterized stored procedures — no inline SQL, no injection surface
- 4-digit PIN for customer accounts, password-protected admin accounts
- Account status flags (`Active` / `Frozen` / `Closed`) are checked before every withdraw / transfer
- Audit log entries are signed with **SHA-256** over `ActionType | Description | Timestamp`

---

## Tech Stack

| Layer       | Technology                                    |
|-------------|-----------------------------------------------|
| UI          | WPF (.NET 8, XAML, storyboards / animations)  |
| Language    | C# 12 (file-scoped namespaces, records)       |
| Data access | ADO.NET via `Microsoft.Data.SqlClient`        |
| Database    | Microsoft SQL Server 2019+ (SSMS 22)          |
| Pattern     | Static `DatabaseHelper` + stored procedures   |

---

## Prerequisites

1. **Windows 10 / 11 (x64)** — required for WPF
2. **.NET 8 SDK** — <https://dotnet.microsoft.com/download>
3. **SQL Server 2019 or newer** (Express edition is fine)
4. **SQL Server Management Studio (SSMS) 22** — <https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms>
5. **Visual Studio 2022 17.8+** with the *".NET desktop development"* workload (or just the .NET SDK + your favorite editor)

---

## Setup

### 1. Clone the repo

```bash
git clone https://github.com/<your-username>/OOP_DB_Project_BMS.git
cd OOP_DB_Project_BMS
```

### 2. Create the database with SSMS 22

1. Launch **SQL Server Management Studio 22**.
2. Connect to your local instance:
   - **Server type:** `Database Engine`
   - **Server name:** `.` (or `localhost\SQLEXPRESS` if you installed SQL Express)
   - **Authentication:** `Windows Authentication`
3. Once connected, press **Ctrl + N** (or click *New Query* in the toolbar).
4. In SSMS, go to **File → Open → File…** and select `SQL/database_schema.sql` from this repo.
5. Make sure the query window is the active tab, then press **F5** (or click **Execute**).
6. Watch the **Messages** tab — you should see `BankManagementDB created successfully.`
7. Refresh the **Databases** node in Object Explorer; `BankManagementDB` should appear.

### 3. Verify the connection string

Open `App.xaml.cs` and confirm the connection string matches your SQL instance:

```csharp
DatabaseHelper.ConnectionString =
    "Server=.;Database=BankManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";
```

If you are using **SQL Express**, change `Server=.` to `Server=.\SQLEXPRESS`.

### 4. Build and run

From the command line:

```bash
dotnet build
dotnet run --project BankManagementSystem.csproj
```

Or from Visual Studio: open `BankManagementSystem.csproj` and press **F5**.

### 5. Default credentials

| Role     | Login                 | Password / PIN |
|----------|-----------------------|----------------|
| Admin    | `admin@bms.local`     | `Admin@123`    |
| Customer | (register your own)   | 4-digit PIN    |

The first customer must register through the Welcome window's **Register** button. After registration, the system returns the auto-generated `PKR…` account number, which you then use to log in.

---

## Project Structure

```
BankManagementSystem/
├── App.xaml / App.xaml.cs          # Application entry + connection string
├── BankManagementSystem.csproj     # Project file (.NET 8, WPF)
├── Styles/
│   └── Theme.xaml                  # Shared colors, brushes, control styles
├── Windows/                        # Top-level windows
│   ├── WelcomeWindow.xaml          # Landing screen with login / register
│   ├── AdminLoginWindow.xaml
│   ├── CustomerLoginWindow.xaml
│   ├── CustomerRegistrationWindow.xaml
│   ├── AdminShellWindow.xaml       # Admin navigation shell
│   └── CustomerShellWindow.xaml    # Customer navigation shell
├── AdminPages/                     # Pages rendered inside AdminShellWindow
│   ├── AdminDashboardPage.xaml
│   ├── ManageCustomersPage.xaml
│   ├── AllTransactionsPage.xaml
│   ├── LoanManagementPage.xaml
│   ├── FraudAlertsPage.xaml
│   ├── AuditLogPage.xaml
│   └── AnalyticsPage.xaml
├── CustomerPages/                  # Pages rendered inside CustomerShellWindow
│   ├── DashboardPage.xaml
│   ├── DepositPage.xaml
│   ├── WithdrawPage.xaml
│   ├── TransferPage.xaml
│   ├── TransactionHistoryPage.xaml
│   ├── LoanRequestPage.xaml
│   └── SettingsPage.xaml
├── Helpers/
│   └── DatabaseHelper.cs           # All ADO.NET calls + stored proc wrappers
└── SQL/
    └── database_schema.sql         # Tables, stored procedures, seed admin
```

---

## Database Schema (overview)

| Table              | Purpose                                                       |
|--------------------|---------------------------------------------------------------|
| `Admins`           | Admin users (email, password hash, active flag)               |
| `Customers`        | Customer profile (name, CNIC, DOB, city, area)                |
| `Accounts`         | Bank accounts (auto-generated `PKR…` number, balance, PIN, status) |
| `Transactions`     | Every deposit / withdrawal / transfer with a reference number |
| `LoanApplications` | Customer loan requests + admin decision fields                |
| `FraudAlerts`      | Suspicious activity alerts (Open / Resolved)                  |
| `AuditLog`         | Tamper-evident log signed with SHA-256                         |

**Stored procedures** exposed by the database:
`sp_RegisterCustomer`, `sp_Deposit`, `sp_Withdraw`, `sp_Transfer`, `sp_ApplyForLoan`, `sp_ApproveLoan`, `sp_RejectLoan`.

---

## Building a Windows installer (optional)

A ready-to-use **Inno Setup 6** script is included as `installer.iss`. Workflow:

```bash
# 1. Publish the app (framework-dependent, smaller installer)
dotnet publish BankManagementSystem.csproj -c Release -r win-x64 --self-contained false -o publish
```

```text
2. Open installer.iss in Inno Setup Studio 6.
3. Press Ctrl+F9 (Compile).
4. The installer will appear in InstallerOutput\BankManagementSystem_Setup_v1.0.0.exe
```

The installer bundles the app, ships the SQL schema script under `SQL\`, creates Start Menu + Desktop shortcuts, and prompts the user to install the .NET 8 Desktop Runtime if it is missing.

---

## License

Submitted as academic coursework for OOP + Databases. Free to use for learning purposes.
