-- ============================================================
-- 02_CreateTables.sql
-- Create all tables for BankManagementDB
-- ============================================================

USE BankManagementDB;
GO

-- Admins table
IF OBJECT_ID('Admins', 'U') IS NULL
BEGIN
    CREATE TABLE Admins (
        AdminId       INT IDENTITY(1,1) PRIMARY KEY,
        FullName      NVARCHAR(100) NOT NULL,
        Email         NVARCHAR(150) NOT NULL UNIQUE,
        PasswordHash  NVARCHAR(256) NOT NULL,
        IsActive      BIT DEFAULT 1,
        CreatedDate   DATETIME DEFAULT GETDATE()
    );
END
GO

-- Customers table
IF OBJECT_ID('Customers', 'U') IS NULL
BEGIN
    CREATE TABLE Customers (
        CustomerId   INT IDENTITY(1,1) PRIMARY KEY,
        FullName     NVARCHAR(100) NOT NULL,
        Email        NVARCHAR(150) NOT NULL UNIQUE,
        Phone        NVARCHAR(20) NOT NULL,
        CNIC         NVARCHAR(15) NOT NULL UNIQUE,
        DateOfBirth  DATE NOT NULL,
        City         NVARCHAR(50) NOT NULL,
        Area         NVARCHAR(100) NOT NULL,
        CreatedDate  DATETIME DEFAULT GETDATE()
    );
END
GO

-- Accounts table
IF OBJECT_ID('Accounts', 'U') IS NULL
BEGIN
    CREATE TABLE Accounts (
        AccountId      INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId     INT NOT NULL FOREIGN KEY REFERENCES Customers(CustomerId),
        AccountNumber  NVARCHAR(20) NOT NULL UNIQUE,
        AccountType    NVARCHAR(20) NOT NULL DEFAULT 'Savings',
        Balance        DECIMAL(18,2) DEFAULT 0,
        PIN            NVARCHAR(10) NOT NULL,
        Status         NVARCHAR(20) DEFAULT 'Active',
        CreatedDate    DATETIME DEFAULT GETDATE()
    );
END
GO

-- Transactions table
IF OBJECT_ID('Transactions', 'U') IS NULL
BEGIN
    CREATE TABLE Transactions (
        TransactionId    INT IDENTITY(1,1) PRIMARY KEY,
        AccountNumber    NVARCHAR(20) NOT NULL FOREIGN KEY REFERENCES Accounts(AccountNumber),
        TransactionType  NVARCHAR(30) NOT NULL,
        Amount           DECIMAL(18,2) NOT NULL,
        Description      NVARCHAR(500),
        TransactionDate  DATETIME DEFAULT GETDATE(),
        ReferenceNumber  NVARCHAR(50)
    );
END
GO

-- LoanApplications table
IF OBJECT_ID('LoanApplications', 'U') IS NULL
BEGIN
    CREATE TABLE LoanApplications (
        LoanAppId      INT IDENTITY(1,1) PRIMARY KEY,
        AccountNumber  NVARCHAR(20) NOT NULL FOREIGN KEY REFERENCES Accounts(AccountNumber),
        LoanAmount     DECIMAL(18,2) NOT NULL,
        Purpose        NVARCHAR(200) NOT NULL,
        TenureMonths   INT NOT NULL,
        Status         NVARCHAR(20) DEFAULT 'Pending',
        ApplicationDate DATETIME DEFAULT GETDATE(),
        ApprovedAmount  DECIMAL(18,2),
        InterestRate    DECIMAL(5,2),
        ApprovedDate    DATETIME,
        Notes           NVARCHAR(500)
    );
END
GO

-- Loans table (disbursed loans)
IF OBJECT_ID('Loans', 'U') IS NULL
BEGIN
    CREATE TABLE Loans (
        LoanId         INT IDENTITY(1,1) PRIMARY KEY,
        LoanAppId      INT NOT NULL FOREIGN KEY REFERENCES LoanApplications(LoanAppId),
        AccountNumber  NVARCHAR(20) NOT NULL FOREIGN KEY REFERENCES Accounts(AccountNumber),
        Principal      DECIMAL(18,2) NOT NULL,
        InterestRate   DECIMAL(5,2) NOT NULL,
        TenureMonths   INT NOT NULL,
        EMI            DECIMAL(18,2) NOT NULL,
        OutstandingBalance DECIMAL(18,2) NOT NULL,
        StartDate      DATETIME DEFAULT GETDATE(),
        Status         NVARCHAR(20) DEFAULT 'Active'
    );
END
GO

-- FraudAlerts table
IF OBJECT_ID('FraudAlerts', 'U') IS NULL
BEGIN
    CREATE TABLE FraudAlerts (
        AlertId        INT IDENTITY(1,1) PRIMARY KEY,
        AccountNumber  NVARCHAR(20) NOT NULL FOREIGN KEY REFERENCES Accounts(AccountNumber),
        AlertType      NVARCHAR(50) NOT NULL,
        Description    NVARCHAR(500),
        Status         NVARCHAR(20) DEFAULT 'Open',
        CreatedDate    DATETIME DEFAULT GETDATE(),
        ResolvedDate   DATETIME,
        ResolutionNotes NVARCHAR(500)
    );
END
GO

-- AuditLog table
IF OBJECT_ID('AuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE AuditLog (
        LogId          INT IDENTITY(1,1) PRIMARY KEY,
        ActionType     NVARCHAR(50) NOT NULL,
        Description    NVARCHAR(1000),
        PerformedBy    NVARCHAR(100),
        ActionDate     DATETIME DEFAULT GETDATE(),
        DataSignature  NVARCHAR(64),
        IPAddress      NVARCHAR(45)
    );
END
GO
