-- ============================================================
-- 07_SeedData.sql
-- Insert sample data for testing
-- ============================================================

USE BankManagementDB;
GO

-- Insert sample customers (if not exist)
IF NOT EXISTS (SELECT 1 FROM Customers WHERE CNIC = '3520112345671')
BEGIN
    INSERT INTO Customers (FullName, Email, Phone, CNIC, DateOfBirth, City, Area)
    VALUES ('Ahmed Khan', 'ahmed.khan@email.com', '03001234567', '3520112345671', '1990-05-15', 'Lahore', 'Gulberg');

    INSERT INTO Accounts (CustomerId, AccountNumber, AccountType, Balance, PIN, Status, CreatedDate)
    VALUES (SCOPE_IDENTITY(), 'BMS10000000001', 'Savings', 500000.00, '1234', 'Active', GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM Customers WHERE CNIC = '4210123456782')
BEGIN
    INSERT INTO Customers (FullName, Email, Phone, CNIC, DateOfBirth, City, Area)
    VALUES ('Fatima Rizvi', 'fatima.rizvi@email.com', '03119876543', '4210123456782', '1988-11-22', 'Karachi', 'DHA');

    DECLARE @FatimaId INT = (SELECT CustomerId FROM Customers WHERE CNIC = '4210123456782');
    IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountNumber = 'BMS10000000002')
    BEGIN
        INSERT INTO Accounts (CustomerId, AccountNumber, AccountType, Balance, PIN, Status, CreatedDate)
        VALUES (@FatimaId, 'BMS10000000002', 'Current', 1200000.00, '5678', 'Active', GETDATE());
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM Customers WHERE CNIC = '6110134567893')
BEGIN
    INSERT INTO Customers (FullName, Email, Phone, CNIC, DateOfBirth, City, Area)
    VALUES ('Muhammad Ali', 'muhammad.ali@email.com', '03214567890', '6110134567893', '1995-03-10', 'Islamabad', 'F-Sector');

    DECLARE @AliId INT = (SELECT CustomerId FROM Customers WHERE CNIC = '6110134567893');
    IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountNumber = 'BMS10000000003')
    BEGIN
        INSERT INTO Accounts (CustomerId, AccountNumber, AccountType, Balance, PIN, Status, CreatedDate)
        VALUES (@AliId, 'BMS10000000003', 'Savings', 250000.00, '9012', 'Active', GETDATE());
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM Customers WHERE CNIC = '3540145678904')
BEGIN
    INSERT INTO Customers (FullName, Email, Phone, CNIC, DateOfBirth, City, Area)
    VALUES ('Ayesha Malik', 'ayesha.malik@email.com', '03335678901', '3540145678904', '1992-07-28', 'Rawalpindi', 'Cantt');

    DECLARE @AyeshaId INT = (SELECT CustomerId FROM Customers WHERE CNIC = '3540145678904');
    IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountNumber = 'BMS10000000004')
    BEGIN
        INSERT INTO Accounts (CustomerId, AccountNumber, AccountType, Balance, PIN, Status, CreatedDate)
        VALUES (@AyeshaId, 'BMS10000000004', 'Business', 3500000.00, '3456', 'Active', GETDATE());
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM Customers WHERE CNIC = '3740156789015')
BEGIN
    INSERT INTO Customers (FullName, Email, Phone, CNIC, DateOfBirth, City, Area)
    VALUES ('Hassan Siddiqui', 'hassan.siddiqui@email.com', '03456789012', '3740156789015', '1987-12-05', 'Faisalabad', 'Madina Town');

    DECLARE @HassanId INT = (SELECT CustomerId FROM Customers WHERE CNIC = '3740156789015');
    IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountNumber = 'BMS10000000005')
    BEGIN
        INSERT INTO Accounts (CustomerId, AccountNumber, AccountType, Balance, PIN, Status, CreatedDate)
        VALUES (@HassanId, 'BMS10000000005', 'Savings', 800000.00, '7890', 'Frozen', GETDATE());
    END
END
GO

-- Insert sample transactions
IF (SELECT COUNT(*) FROM Transactions) = 0
BEGIN
    INSERT INTO Transactions (AccountNumber, TransactionType, Amount, Description, TransactionDate, ReferenceNumber)
    VALUES
        ('BMS10000000001', 'Deposit', 500000.00, 'Initial Deposit', DATEADD(DAY, -30, GETDATE()), 'DEP2024010100001'),
        ('BMS10000000001', 'Withdrawal', 50000.00, 'ATM Withdrawal', DATEADD(DAY, -25, GETDATE()), 'WD2024010500001'),
        ('BMS10000000002', 'Deposit', 1200000.00, 'Initial Deposit', DATEADD(DAY, -28, GETDATE()), 'DEP2024010200002'),
        ('BMS10000000002', 'Transfer Out', 100000.00, 'Transfer to BMS10000000003', DATEADD(DAY, -20, GETDATE()), 'TRF2024011000001'),
        ('BMS10000000003', 'Transfer In', 100000.00, 'Transfer from BMS10000000002', DATEADD(DAY, -20, GETDATE()), 'TRF2024011000002'),
        ('BMS10000000003', 'Deposit', 150000.00, 'Salary Credit', DATEADD(DAY, -15, GETDATE()), 'DEP2024011500003'),
        ('BMS10000000004', 'Deposit', 3500000.00, 'Business Deposit', DATEADD(DAY, -22, GETDATE()), 'DEP2024010800004'),
        ('BMS10000000004', 'Withdrawal', 200000.00, 'Vendor Payment', DATEADD(DAY, -18, GETDATE()), 'WD2024011200002'),
        ('BMS10000000005', 'Deposit', 800000.00, 'Initial Deposit', DATEADD(DAY, -10, GETDATE()), 'DEP2024012000005'),
        ('BMS10000000001', 'Deposit', 75000.00, 'Freelance Payment', DATEADD(DAY, -5, GETDATE()), 'DEP2024012500006');
END
GO

-- Insert sample loan applications
IF (SELECT COUNT(*) FROM LoanApplications) = 0
BEGIN
    INSERT INTO LoanApplications (AccountNumber, LoanAmount, Purpose, TenureMonths, Status, ApplicationDate, ApprovedAmount, InterestRate, ApprovedDate, Notes)
    VALUES
        ('BMS10000000001', 1000000.00, 'Home Loan', 60, 'Pending', DATEADD(DAY, -3, GETDATE()), NULL, NULL, NULL, NULL),
        ('BMS10000000002', 500000.00, 'Car Loan', 36, 'Approved', DATEADD(DAY, -15, GETDATE()), 500000.00, 12.50, DATEADD(DAY, -12, GETDATE()), 'Approved with standard rate'),
        ('BMS10000000003', 200000.00, 'Personal Loan', 12, 'Rejected', DATEADD(DAY, -10, GETDATE()), NULL, NULL, DATEADD(DAY, -8, GETDATE()), 'Insufficient credit history'),
        ('BMS10000000004', 2000000.00, 'Business Loan', 48, 'Pending', DATEADD(DAY, -1, GETDATE()), NULL, NULL, NULL, NULL),
        ('BMS10000000001', 300000.00, 'Education Loan', 24, 'Pending', DATEADD(DAY, -7, GETDATE()), NULL, NULL, NULL, NULL);
END
GO

-- Insert corresponding loan record for the approved application
IF (SELECT COUNT(*) FROM Loans) = 0
BEGIN
    -- Loan for Fatima's car loan (LoanAppId = 2)
    DECLARE @LoanAppId2 INT = (SELECT LoanAppId FROM LoanApplications WHERE AccountNumber = 'BMS10000000002' AND Purpose = 'Car Loan');
    IF @LoanAppId2 IS NOT NULL
    BEGIN
        INSERT INTO Loans (LoanAppId, AccountNumber, Principal, InterestRate, TenureMonths, EMI, OutstandingBalance, StartDate, Status)
        VALUES (@LoanAppId2, 'BMS10000000002', 500000.00, 12.50, 36, dbo.fn_CalculateEMI(500000, 12.50, 36), 500000.00, DATEADD(DAY, -12, GETDATE()), 'Active');
    END
END
GO

-- Insert sample fraud alerts
IF (SELECT COUNT(*) FROM FraudAlerts) = 0
BEGIN
    INSERT INTO FraudAlerts (AccountNumber, AlertType, Description, Status, CreatedDate, ResolvedDate, ResolutionNotes)
    VALUES
        ('BMS10000000005', 'Suspicious Activity', 'Multiple failed login attempts detected', 'Open', DATEADD(DAY, -2, GETDATE()), NULL, NULL),
        ('BMS10000000001', 'Unusual Transaction', 'Large withdrawal amount detected', 'Open', DATEADD(DAY, -1, GETDATE()), NULL, NULL),
        ('BMS10000000003', 'Account Takeover', 'Login from unusual location', 'Resolved', DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, -8, GETDATE()), 'Customer confirmed activity was legitimate');
END
GO

-- Insert sample audit log
IF (SELECT COUNT(*) FROM AuditLog) = 0
BEGIN
    INSERT INTO AuditLog (ActionType, Description, PerformedBy, ActionDate, DataSignature)
    VALUES
        ('SystemStart', 'Bank Management System initialized', 'System', DATEADD(DAY, -30, GETDATE()), CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', 'SystemStart' + CAST(DATEADD(DAY, -30, GETDATE()) AS NVARCHAR(30))), 2)),
        ('CustomerRegistration', 'New customer registered: Ahmed Khan, Account: BMS10000000001', 'System', DATEADD(DAY, -30, GETDATE()), CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', 'Ahmed KhanBMS10000000001' + CAST(DATEADD(DAY, -30, GETDATE()) AS NVARCHAR(30))), 2)),
        ('CustomerRegistration', 'New customer registered: Fatima Rizvi, Account: BMS10000000002', 'System', DATEADD(DAY, -28, GETDATE()), CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', 'Fatima RizviBMS10000000002' + CAST(DATEADD(DAY, -28, GETDATE()) AS NVARCHAR(30))), 2)),
        ('Deposit', 'Deposit of PKR 500000 to account BMS10000000001', 'System', DATEADD(DAY, -30, GETDATE()), CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', 'BMS10000000001500000' + CAST(DATEADD(DAY, -30, GETDATE()) AS NVARCHAR(30))), 2)),
        ('Withdrawal', 'Withdrawal of PKR 50000 from account BMS10000000001', 'System', DATEADD(DAY, -25, GETDATE()), CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', 'BMS1000000000150000WD' + CAST(DATEADD(DAY, -25, GETDATE()) AS NVARCHAR(30))), 2)),
        ('Transfer', 'Transfer of PKR 100000 from BMS10000000002 to BMS10000000003', 'System', DATEADD(DAY, -20, GETDATE()), CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', 'BMS10000000002BMS10000000003100000TRF' + CAST(DATEADD(DAY, -20, GETDATE()) AS NVARCHAR(30))), 2)),
        ('LoanApplication', 'Loan application of PKR 1000000 for Home Loan from account BMS10000000001', 'System', DATEADD(DAY, -3, GETDATE()), CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', 'BMS100000000011000000Home LoanLOAN' + CAST(DATEADD(DAY, -3, GETDATE()) AS NVARCHAR(30))), 2)),
        ('LoanApproval', 'Loan approved for PKR 500000 at 12.50% for account BMS10000000002', 'System', DATEADD(DAY, -12, GETDATE()), CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', '250000012.5APPROVE' + CAST(DATEADD(DAY, -12, GETDATE()) AS NVARCHAR(30))), 2)),
        ('LoanRejection', 'Loan rejected for account BMS10000000003', 'System', DATEADD(DAY, -8, GETDATE()), CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', '3REJECT' + CAST(DATEADD(DAY, -8, GETDATE()) AS NVARCHAR(30))), 2)),
        ('AccountStatusUpdate', 'Account BMS10000000005 status changed to Frozen', 'System', DATEADD(DAY, -5, GETDATE()), CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', 'BMS10000000005Frozen' + CAST(DATEADD(DAY, -5, GETDATE()) AS NVARCHAR(30))), 2));
END
GO
