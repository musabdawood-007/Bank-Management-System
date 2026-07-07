-- ============================================================
-- 09_Indexes.sql
-- Create indexes for performance optimization
-- Run AFTER all tables, procedures, and data are in place
-- ============================================================

USE BankManagementDB;
GO

-- ============================================================
-- Accounts table indexes
-- ============================================================

-- Primary lookup index: most queries filter by AccountNumber
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_AccountNumber' AND object_id = OBJECT_ID('Accounts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Accounts_AccountNumber
    ON Accounts(AccountNumber);
    PRINT 'Created IX_Accounts_AccountNumber';
END
GO

-- Filter accounts by status (Active/Frozen/Closed)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_Status' AND object_id = OBJECT_ID('Accounts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Accounts_Status
    ON Accounts(Status);
    PRINT 'Created IX_Accounts_Status';
END
GO

-- Customer to Account join optimization
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_CustomerId' AND object_id = OBJECT_ID('Accounts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Accounts_CustomerId
    ON Accounts(CustomerId);
    PRINT 'Created IX_Accounts_CustomerId';
END
GO

-- ============================================================
-- Transactions table indexes
-- ============================================================

-- Most common query: transactions for a specific account
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transactions_AccountNumber' AND object_id = OBJECT_ID('Transactions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Transactions_AccountNumber
    ON Transactions(AccountNumber);
    PRINT 'Created IX_Transactions_AccountNumber';
END
GO

-- Filter by transaction type (Deposit, Withdrawal, Transfer)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transactions_TransactionType' AND object_id = OBJECT_ID('Transactions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Transactions_TransactionType
    ON Transactions(TransactionType);
    PRINT 'Created IX_Transactions_TransactionType';
END
GO

-- Sort transactions by date (most recent first)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transactions_TransactionDate' AND object_id = OBJECT_ID('Transactions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Transactions_TransactionDate
    ON Transactions(TransactionDate DESC);
    PRINT 'Created IX_Transactions_TransactionDate';
END
GO

-- Composite index for account + date range queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transactions_AccountDate' AND object_id = OBJECT_ID('Transactions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Transactions_AccountDate
    ON Transactions(AccountNumber, TransactionDate DESC);
    PRINT 'Created IX_Transactions_AccountDate';
END
GO

-- ============================================================
-- LoanApplications table indexes
-- ============================================================

-- Look up loans by account
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoanApplications_AccountNumber' AND object_id = OBJECT_ID('LoanApplications'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_LoanApplications_AccountNumber
    ON LoanApplications(AccountNumber);
    PRINT 'Created IX_LoanApplications_AccountNumber';
END
GO

-- Filter by status (Pending/Approved/Rejected)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoanApplications_Status' AND object_id = OBJECT_ID('LoanApplications'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_LoanApplications_Status
    ON LoanApplications(Status);
    PRINT 'Created IX_LoanApplications_Status';
END
GO

-- Sort by application date
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LoanApplications_ApplicationDate' AND object_id = OBJECT_ID('LoanApplications'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_LoanApplications_ApplicationDate
    ON LoanApplications(ApplicationDate DESC);
    PRINT 'Created IX_LoanApplications_ApplicationDate';
END
GO

-- ============================================================
-- Loans table indexes
-- ============================================================

-- Look up active loans by account
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Loans_AccountNumber' AND object_id = OBJECT_ID('Loans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Loans_AccountNumber
    ON Loans(AccountNumber);
    PRINT 'Created IX_Loans_AccountNumber';
END
GO

-- Filter by loan status
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Loans_Status' AND object_id = OBJECT_ID('Loans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Loans_Status
    ON Loans(Status);
    PRINT 'Created IX_Loans_Status';
END
GO

-- Join optimization: LoanAppId lookup
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Loans_LoanAppId' AND object_id = OBJECT_ID('Loans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Loans_LoanAppId
    ON Loans(LoanAppId);
    PRINT 'Created IX_Loans_LoanAppId';
END
GO

-- ============================================================
-- FraudAlerts table indexes
-- ============================================================

-- Look up fraud alerts by account
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FraudAlerts_AccountNumber' AND object_id = OBJECT_ID('FraudAlerts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FraudAlerts_AccountNumber
    ON FraudAlerts(AccountNumber);
    PRINT 'Created IX_FraudAlerts_AccountNumber';
END
GO

-- Filter by status (Open/Resolved)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FraudAlerts_Status' AND object_id = OBJECT_ID('FraudAlerts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FraudAlerts_Status
    ON FraudAlerts(Status);
    PRINT 'Created IX_FraudAlerts_Status';
END
GO

-- Sort by creation date
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FraudAlerts_CreatedDate' AND object_id = OBJECT_ID('FraudAlerts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FraudAlerts_CreatedDate
    ON FraudAlerts(CreatedDate DESC);
    PRINT 'Created IX_FraudAlerts_CreatedDate';
END
GO

-- ============================================================
-- AuditLog table indexes
-- ============================================================

-- Sort audit log by date (most recent first)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_ActionDate' AND object_id = OBJECT_ID('AuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLog_ActionDate
    ON AuditLog(ActionDate DESC);
    PRINT 'Created IX_AuditLog_ActionDate';
END
GO

-- Filter by action type
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_ActionType' AND object_id = OBJECT_ID('AuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLog_ActionType
    ON AuditLog(ActionType);
    PRINT 'Created IX_AuditLog_ActionType';
END
GO

-- Data signature lookup for integrity verification
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLog_DataSignature' AND object_id = OBJECT_ID('AuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLog_DataSignature
    ON AuditLog(DataSignature);
    PRINT 'Created IX_AuditLog_DataSignature';
END
GO

-- ============================================================
-- Customers table indexes
-- ============================================================

-- Email lookup for login and uniqueness
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_Email' AND object_id = OBJECT_ID('Customers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Customers_Email
    ON Customers(Email);
    PRINT 'Created IX_Customers_Email';
END
GO

-- CNIC lookup for verification
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_CNIC' AND object_id = OBJECT_ID('Customers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Customers_CNIC
    ON Customers(CNIC);
    PRINT 'Created IX_Customers_CNIC';
END
GO

-- City-based queries and reports
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_City' AND object_id = OBJECT_ID('Customers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Customers_City
    ON Customers(City);
    PRINT 'Created IX_Customers_City';
END
GO

-- ============================================================
-- Admins table indexes
-- ============================================================

-- Email lookup for admin login
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Admins_Email' AND object_id = OBJECT_ID('Admins'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Admins_Email
    ON Admins(Email);
    PRINT 'Created IX_Admins_Email';
END
GO

-- Filter active admins
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Admins_IsActive' AND object_id = OBJECT_ID('Admins'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Admins_IsActive
    ON Admins(IsActive);
    PRINT 'Created IX_Admins_IsActive';
END
GO

-- ============================================================
-- Verification
-- ============================================================

PRINT '';
PRINT '=== Index Creation Summary ===';
PRINT '';

SELECT
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name LIKE 'IX_%'
ORDER BY t.name, i.name;

PRINT '';
PRINT '=== Index Creation Complete ===';
GO
