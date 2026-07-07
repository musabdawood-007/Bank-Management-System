using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace BankManagementSystem.Helpers;

public static class DatabaseHelper
{
    public static string ConnectionString { get; set; } = string.Empty;

    private static SqlConnection GetConnection() => new(ConnectionString);

    /// <summary>
    /// Generates a SHA-256 data signature for audit log integrity.
    /// Combines action type, description, and timestamp to create a tamper-evident hash.
    /// </summary>
    private static string GenerateDataSignature(string actionType, string description)
    {
        var input = $"{actionType}|{description}|{DateTime.UtcNow:O}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    public static string TestConnection()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            return "Connected successfully!";
        }
        catch (Exception ex)
        {
            return $"Connection failed: {ex.Message}";
        }
    }

    public static (bool Success, string Message) ValidateAdminLogin(string email, string password)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT AdminId, FullName FROM Admins WHERE Email = @Email AND PasswordHash = @Password AND IsActive = 1", conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Password", password);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return (true, reader["FullName"].ToString() ?? "Admin");
            return (false, "Invalid email or password.");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public static (bool Success, string Message) ValidateCustomerLogin(string accountNumber, string pin)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT c.FullName FROM Customers c
                INNER JOIN Accounts a ON c.CustomerId = a.CustomerId
                WHERE a.AccountNumber = @AccountNumber AND a.PIN = @PIN AND a.Status = 'Active'", conn);
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            cmd.Parameters.AddWithValue("@PIN", pin);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return (true, reader["FullName"].ToString() ?? "Customer");
            return (false, "Invalid account number or PIN.");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public static string RegisterCustomer(string fullName, string email, string phone, string cnic,
        DateTime dob, string city, string area, string accountType, string pin)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("sp_RegisterCustomer", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FullName", fullName);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Phone", phone);
            cmd.Parameters.AddWithValue("@CNIC", cnic);
            cmd.Parameters.AddWithValue("@DateOfBirth", dob);
            cmd.Parameters.AddWithValue("@City", city);
            cmd.Parameters.AddWithValue("@Area", area);
            cmd.Parameters.AddWithValue("@AccountType", accountType);
            cmd.Parameters.AddWithValue("@PIN", pin);
            var outputParam = new SqlParameter("@AccountNumber", SqlDbType.NVarChar, 20)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);
            cmd.ExecuteNonQuery();
            return outputParam.Value?.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public static string GetCustomerName(string accountNumber)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT c.FullName FROM Customers c
                INNER JOIN Accounts a ON c.CustomerId = a.CustomerId
                WHERE a.AccountNumber = @AccountNumber", conn);
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "Unknown";
        }
        catch { return "Unknown"; }
    }

    public static string GetAccountType(string accountNumber)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT AccountType FROM Accounts WHERE AccountNumber = @AccountNumber", conn);
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "N/A";
        }
        catch { return "N/A"; }
    }

    public static decimal GetBalance(string accountNumber)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT Balance FROM Accounts WHERE AccountNumber = @AccountNumber", conn);
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            var result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }
        catch { return 0; }
    }

    public static string GetAccountStatus(string accountNumber)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT Status FROM Accounts WHERE AccountNumber = @AccountNumber", conn);
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "Unknown";
        }
        catch { return "Unknown"; }
    }

    public static bool Deposit(string accountNumber, decimal amount, string description)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("sp_Deposit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch { return false; }
    }

    public static (bool Success, string Message) Withdraw(string accountNumber, decimal amount, string description)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("sp_Withdraw", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var success = Convert.ToBoolean(reader["Success"]);
                var message = reader["Message"].ToString() ?? "";
                return (success, message);
            }
            return (false, "Withdrawal failed.");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public static (bool Success, string Message) Transfer(string fromAccount, string toAccount, decimal amount, string description)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("sp_Transfer", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FromAccountNumber", fromAccount);
            cmd.Parameters.AddWithValue("@ToAccountNumber", toAccount);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var success = Convert.ToBoolean(reader["Success"]);
                var message = reader["Message"].ToString() ?? "";
                return (success, message);
            }
            return (false, "Transfer failed.");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public static (bool Success, string Message) SubmitLoanRequest(string accountNumber, decimal amount, string purpose, int tenureMonths)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("sp_ApplyForLoan", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            cmd.Parameters.AddWithValue("@LoanAmount", amount);
            cmd.Parameters.AddWithValue("@Purpose", purpose);
            cmd.Parameters.AddWithValue("@TenureMonths", tenureMonths);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var success = Convert.ToBoolean(reader["Success"]);
                var message = reader["Message"].ToString() ?? "";
                return (success, message);
            }
            return (false, "Loan application failed.");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public static DataTable GetTransactionHistory(string accountNumber, int count)
    {
        var dt = new DataTable();
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT TOP(@Count) TransactionId, TransactionType, Amount, Description,
                       TransactionDate, ReferenceNumber
                FROM Transactions
                WHERE AccountNumber = @AccountNumber
                ORDER BY TransactionDate DESC", conn);
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            cmd.Parameters.AddWithValue("@Count", count);
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        catch { /* return empty table */ }
        return dt;
    }

    public static int GetTotalCustomers()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM Customers", conn);
            return (int)cmd.ExecuteScalar()!;
        }
        catch { return 0; }
    }

    public static int GetTotalAccounts()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM Accounts", conn);
            return (int)cmd.ExecuteScalar()!;
        }
        catch { return 0; }
    }

    public static decimal GetTotalDeposits()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT ISNULL(SUM(Amount),0) FROM Transactions WHERE TransactionType = 'Deposit'", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar()!);
        }
        catch { return 0; }
    }

    public static decimal GetTotalWithdrawals()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT ISNULL(SUM(Amount),0) FROM Transactions WHERE TransactionType = 'Withdrawal'", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar()!);
        }
        catch { return 0; }
    }

    public static int GetTotalTransactions()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM Transactions", conn);
            return (int)cmd.ExecuteScalar()!;
        }
        catch { return 0; }
    }

    public static int GetLoanCountByStatus(string status)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM LoanApplications WHERE Status = @Status", conn);
            cmd.Parameters.AddWithValue("@Status", status);
            return (int)cmd.ExecuteScalar()!;
        }
        catch { return 0; }
    }

    public static int GetOpenFraudFlags()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM FraudAlerts WHERE Status = 'Open'", conn);
            return (int)cmd.ExecuteScalar()!;
        }
        catch { return 0; }
    }

    public static DataTable GetAllCustomers(string search)
    {
        var dt = new DataTable();
        try
        {
            using var conn = GetConnection();
            conn.Open();
            var sql = @"
                SELECT c.CustomerId, c.FullName, c.Email, c.Phone, c.CNIC, c.City,
                       a.AccountNumber, a.AccountType, a.Balance, a.Status, a.CreatedDate
                FROM Customers c
                INNER JOIN Accounts a ON c.CustomerId = a.CustomerId";
            if (!string.IsNullOrWhiteSpace(search))
                sql += " WHERE c.FullName LIKE @Search OR c.Email LIKE @Search OR a.AccountNumber LIKE @Search OR c.CNIC LIKE @Search";
            sql += " ORDER BY c.FullName";
            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@Search", $"%{search}%");
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        catch { /* return empty table */ }
        return dt;
    }

    public static void UpdateAccountStatus(string accountNumber, string status)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("UPDATE Accounts SET Status = @Status WHERE AccountNumber = @AccountNumber", conn);
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.ExecuteNonQuery();

            // Log to audit
            LogAudit("AccountStatusUpdate", $"Account {accountNumber} status changed to {status}");
        }
        catch { }
    }

    public static DataTable GetAllTransactions(int count)
    {
        var dt = new DataTable();
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT TOP(@Count) t.TransactionId, t.AccountNumber, t.TransactionType,
                       t.Amount, t.Description, t.TransactionDate, t.ReferenceNumber,
                       c.FullName AS CustomerName
                FROM Transactions t
                INNER JOIN Accounts a ON t.AccountNumber = a.AccountNumber
                INNER JOIN Customers c ON a.CustomerId = c.CustomerId
                ORDER BY t.TransactionDate DESC", conn);
            cmd.Parameters.AddWithValue("@Count", count);
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        catch { }
        return dt;
    }

    public static DataTable GetAllLoanApplications()
    {
        var dt = new DataTable();
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT la.LoanAppId, la.AccountNumber, c.FullName AS CustomerName,
                       la.LoanAmount, la.Purpose, la.TenureMonths, la.Status,
                       la.ApplicationDate, la.ApprovedAmount, la.InterestRate,
                       la.ApprovedDate, la.Notes
                FROM LoanApplications la
                INNER JOIN Accounts a ON la.AccountNumber = a.AccountNumber
                INNER JOIN Customers c ON a.CustomerId = c.CustomerId
                ORDER BY la.ApplicationDate DESC", conn);
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        catch { }
        return dt;
    }

    public static void ApproveLoan(int loanAppId, decimal approvedAmount, decimal interestRate, string notes)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("sp_ApproveLoan", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@LoanAppId", loanAppId);
            cmd.Parameters.AddWithValue("@ApprovedAmount", approvedAmount);
            cmd.Parameters.AddWithValue("@InterestRate", interestRate);
            cmd.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    public static void RejectLoan(int loanAppId, string notes)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("sp_RejectLoan", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@LoanAppId", loanAppId);
            cmd.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    public static DataTable GetFraudFlags()
    {
        var dt = new DataTable();
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT f.AlertId, f.AccountNumber, c.FullName AS CustomerName,
                       f.AlertType, f.Description, f.Status, f.CreatedDate,
                       f.ResolvedDate, f.ResolutionNotes
                FROM FraudAlerts f
                INNER JOIN Accounts a ON f.AccountNumber = a.AccountNumber
                INNER JOIN Customers c ON a.CustomerId = c.CustomerId
                ORDER BY f.CreatedDate DESC", conn);
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        catch { }
        return dt;
    }

    public static void ResolveFraudAlert(int alertId, string notes)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE FraudAlerts
                SET Status = 'Resolved', ResolvedDate = GETDATE(), ResolutionNotes = @Notes
                WHERE AlertId = @AlertId", conn);
            cmd.Parameters.AddWithValue("@AlertId", alertId);
            cmd.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);
            cmd.ExecuteNonQuery();
            LogAudit("FraudAlertResolved", $"Fraud alert {alertId} resolved");
        }
        catch { }
    }

    public static DataTable GetAuditLog(int count)
    {
        var dt = new DataTable();
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT TOP(@Count) LogId, ActionType, Description, PerformedBy, ActionDate, DataSignature
                FROM AuditLog
                ORDER BY ActionDate DESC", conn);
            cmd.Parameters.AddWithValue("@Count", count);
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        catch { }
        return dt;
    }

    public static (bool Success, string Message) ChangePin(string accountNumber, string oldPin, string newPin)
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            // Verify old PIN
            using var verifyCmd = new SqlCommand(
                "SELECT COUNT(*) FROM Accounts WHERE AccountNumber = @AccountNumber AND PIN = @OldPIN", conn);
            verifyCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            verifyCmd.Parameters.AddWithValue("@OldPIN", oldPin);
            var count = (int)verifyCmd.ExecuteScalar()!;
            if (count == 0)
                return (false, "Current PIN is incorrect.");

            // Update PIN
            using var updateCmd = new SqlCommand(
                "UPDATE Accounts SET PIN = @NewPIN WHERE AccountNumber = @AccountNumber", conn);
            updateCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            updateCmd.Parameters.AddWithValue("@NewPIN", newPin);
            updateCmd.ExecuteNonQuery();

            LogAudit("PINChange", $"PIN changed for account {accountNumber}");
            return (true, "PIN changed successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public static DataTable GetMyLoans(string accountNumber)
    {
        var dt = new DataTable();
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT LoanAppId, LoanAmount, Purpose, TenureMonths, Status,
                       ApplicationDate, ApprovedAmount, InterestRate, ApprovedDate
                FROM LoanApplications
                WHERE AccountNumber = @AccountNumber
                ORDER BY ApplicationDate DESC", conn);
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        catch { }
        return dt;
    }

    public static decimal GetTotalLoansDisbursed()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT ISNULL(SUM(ApprovedAmount),0) FROM LoanApplications WHERE Status = 'Approved'", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar()!);
        }
        catch { return 0; }
    }

    public static decimal GetTotalBalance()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT ISNULL(SUM(Balance),0) FROM Accounts WHERE Status = 'Active'", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar()!);
        }
        catch { return 0; }
    }

    public static int GetActiveLoanCount()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM LoanApplications WHERE Status = 'Approved'", conn);
            return (int)cmd.ExecuteScalar()!;
        }
        catch { return 0; }
    }

    public static DataTable GetCustomerProfile(string accountNumber)
    {
        var dt = new DataTable();
        try
        {
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT c.FullName, c.Email, c.Phone, c.CNIC, c.DateOfBirth,
                       c.City, c.Area, a.AccountNumber, a.AccountType, a.Balance, a.Status, a.CreatedDate
                FROM Customers c
                INNER JOIN Accounts a ON c.CustomerId = a.CustomerId
                WHERE a.AccountNumber = @AccountNumber", conn);
            cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        catch { }
        return dt;
    }

    private static void LogAudit(string actionType, string description)
    {
        try
        {
            var signature = GenerateDataSignature(actionType, description);
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO AuditLog (ActionType, Description, PerformedBy, ActionDate, DataSignature)
                VALUES (@ActionType, @Description, @PerformedBy, GETDATE(), @DataSignature)", conn);
            cmd.Parameters.AddWithValue("@ActionType", actionType);
            cmd.Parameters.AddWithValue("@Description", description);
            cmd.Parameters.AddWithValue("@PerformedBy", "System");
            cmd.Parameters.AddWithValue("@DataSignature", signature);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }
}
