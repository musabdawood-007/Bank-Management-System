-- ============================================================
-- 05_Views.sql
-- Create views for reporting
-- ============================================================

USE BankManagementDB;
GO

-- vw_CustomerDetails: combined customer + account info
IF OBJECT_ID('vw_CustomerDetails', 'V') IS NOT NULL
    DROP VIEW vw_CustomerDetails;
GO

CREATE VIEW vw_CustomerDetails AS
SELECT
    c.CustomerId,
    c.FullName,
    c.Email,
    c.Phone,
    c.CNIC,
    c.DateOfBirth,
    c.City,
    c.Area,
    c.CreatedDate AS CustomerSince,
    a.AccountNumber,
    a.AccountType,
    a.Balance,
    a.Status AS AccountStatus,
    a.CreatedDate AS AccountCreatedDate
FROM Customers c
INNER JOIN Accounts a ON c.CustomerId = a.CustomerId;
GO

-- vw_TransactionSummary: transaction summary per account
IF OBJECT_ID('vw_TransactionSummary', 'V') IS NOT NULL
    DROP VIEW vw_TransactionSummary;
GO

CREATE VIEW vw_TransactionSummary AS
SELECT
    a.AccountNumber,
    c.FullName AS CustomerName,
    COUNT(t.TransactionId) AS TotalTransactions,
    ISNULL(SUM(CASE WHEN t.TransactionType IN ('Deposit', 'Transfer In', 'Loan Disbursement') THEN t.Amount ELSE 0 END), 0) AS TotalCredits,
    ISNULL(SUM(CASE WHEN t.TransactionType IN ('Withdrawal', 'Transfer Out') THEN t.Amount ELSE 0 END), 0) AS TotalDebits,
    a.Balance AS CurrentBalance
FROM Accounts a
INNER JOIN Customers c ON a.CustomerId = c.CustomerId
LEFT JOIN Transactions t ON a.AccountNumber = t.AccountNumber
GROUP BY a.AccountNumber, c.FullName, a.Balance;
GO

-- vw_LoanSummary: loan summary with customer info
IF OBJECT_ID('vw_LoanSummary', 'V') IS NOT NULL
    DROP VIEW vw_LoanSummary;
GO

CREATE VIEW vw_LoanSummary AS
SELECT
    l.LoanId,
    la.LoanAppId,
    a.AccountNumber,
    c.FullName AS CustomerName,
    la.LoanAmount AS RequestedAmount,
    l.Principal AS DisbursedAmount,
    l.InterestRate,
    l.TenureMonths,
    l.EMI,
    l.OutstandingBalance,
    l.StartDate,
    l.Status AS LoanStatus,
    la.Purpose
FROM Loans l
INNER JOIN LoanApplications la ON l.LoanAppId = la.LoanAppId
INNER JOIN Accounts a ON la.AccountNumber = a.AccountNumber
INNER JOIN Customers c ON a.CustomerId = c.CustomerId;
GO
