-- ============================================================
-- 08_Verify.sql
-- Verification queries to confirm setup
-- ============================================================

USE BankManagementDB;
GO

PRINT '=== BankManagementDB Verification ===';
PRINT '';

-- Check tables
PRINT '--- Tables ---';
SELECT TABLE_NAME AS TableName FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;

-- Check row counts
PRINT '';
PRINT '--- Row Counts ---';
SELECT 'Admins' AS TableName, COUNT(*) AS RowCount FROM Admins
UNION ALL SELECT 'Customers', COUNT(*) FROM Customers
UNION ALL SELECT 'Accounts', COUNT(*) FROM Accounts
UNION ALL SELECT 'Transactions', COUNT(*) FROM Transactions
UNION ALL SELECT 'LoanApplications', COUNT(*) FROM LoanApplications
UNION ALL SELECT 'Loans', COUNT(*) FROM Loans
UNION ALL SELECT 'FraudAlerts', COUNT(*) FROM FraudAlerts
UNION ALL SELECT 'AuditLog', COUNT(*) FROM AuditLog;

-- Check audit log data signatures
PRINT '';
PRINT '--- Audit Log Data Signatures ---';
SELECT TOP 5 LogId, ActionType, LEFT(DataSignature, 16) + '...' AS SignaturePreview
FROM AuditLog
ORDER BY ActionDate DESC;

-- Check stored procedures
PRINT '';
PRINT '--- Stored Procedures ---';
SELECT ROUTINE_NAME AS ProcedureName FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_NAME LIKE 'sp_%' ORDER BY ROUTINE_NAME;

-- Check functions
PRINT '';
PRINT '--- Functions ---';
SELECT ROUTINE_NAME AS FunctionName FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_NAME LIKE 'fn_%' ORDER BY ROUTINE_NAME;

-- Check views
PRINT '';
PRINT '--- Views ---';
SELECT TABLE_NAME AS ViewName FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME LIKE 'vw_%' ORDER BY TABLE_NAME;

-- Verify admin login
PRINT '';
PRINT '--- Admin Login Check ---';
SELECT AdminId, FullName, Email, IsActive FROM Admins WHERE Email = 'admin@bms.com';

-- Verify customer accounts
PRINT '';
PRINT '--- Customer Accounts ---';
SELECT c.FullName, a.AccountNumber, a.AccountType, a.Balance, a.Status
FROM Customers c
INNER JOIN Accounts a ON c.CustomerId = a.CustomerId
ORDER BY a.AccountNumber;

-- Verify loan applications
PRINT '';
PRINT '--- Loan Applications ---';
SELECT LoanAppId, AccountNumber, LoanAmount, Purpose, Status, ApprovedAmount, InterestRate
FROM LoanApplications
ORDER BY LoanAppId;

-- Verify function: EMI Calculation
PRINT '';
PRINT '--- EMI Calculation Test (500000 @ 12.5% for 36 months) ---';
SELECT dbo.fn_CalculateEMI(500000, 12.50, 36) AS EMI;

-- Verify fraud alerts
PRINT '';
PRINT '--- Fraud Alerts ---';
SELECT AlertId, AccountNumber, AlertType, Status FROM FraudAlerts;

PRINT '';
PRINT '=== Verification Complete ===';
GO
