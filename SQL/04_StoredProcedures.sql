-- ============================================================
-- 04_StoredProcedures.sql
-- Create all stored procedures
-- ============================================================

USE BankManagementDB;
GO

-- sp_RegisterCustomer
IF OBJECT_ID('sp_RegisterCustomer', 'P') IS NOT NULL
    DROP PROCEDURE sp_RegisterCustomer;
GO

CREATE PROCEDURE sp_RegisterCustomer
    @FullName      NVARCHAR(100),
    @Email         NVARCHAR(150),
    @Phone         NVARCHAR(20),
    @CNIC          NVARCHAR(15),
    @DateOfBirth   DATE,
    @City          NVARCHAR(50),
    @Area          NVARCHAR(100),
    @AccountType   NVARCHAR(20),
    @PIN           NVARCHAR(10),
    @AccountNumber NVARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert customer
        INSERT INTO Customers (FullName, Email, Phone, CNIC, DateOfBirth, City, Area)
        VALUES (@FullName, @Email, @Phone, @CNIC, @DateOfBirth, @City, @Area);

        DECLARE @CustomerId INT = SCOPE_IDENTITY();

        -- Generate account number
        SET @AccountNumber = dbo.fn_GenerateAccountNumber();

        -- Insert account
        INSERT INTO Accounts (CustomerId, AccountNumber, AccountType, PIN, Status)
        VALUES (@CustomerId, @AccountNumber, @AccountType, @PIN, 'Active');

        -- Log to audit with data signature
        DECLARE @Sig NVARCHAR(64) = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @FullName + @AccountNumber + CAST(GETDATE() AS NVARCHAR(30))), 2);
        INSERT INTO AuditLog (ActionType, Description, PerformedBy, ActionDate, DataSignature)
        VALUES ('CustomerRegistration', 'New customer registered: ' + @FullName + ', Account: ' + @AccountNumber, 'System', GETDATE(), @Sig);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @AccountNumber = N'';
        -- Re-throw with context
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Msg, 16, 1);
    END CATCH
END
GO

-- sp_Deposit
IF OBJECT_ID('sp_Deposit', 'P') IS NOT NULL
    DROP PROCEDURE sp_Deposit;
GO

CREATE PROCEDURE sp_Deposit
    @AccountNumber NVARCHAR(20),
    @Amount        DECIMAL(18,2),
    @Description   NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Verify account is active
        DECLARE @Status NVARCHAR(20);
        SELECT @Status = Status FROM Accounts WHERE AccountNumber = @AccountNumber;

        IF @Status IS NULL
        BEGIN
            RAISERROR('Account not found.', 16, 1);
            RETURN;
        END

        IF @Status != 'Active'
        BEGIN
            RAISERROR('Account is not active. Cannot deposit.', 16, 1);
            RETURN;
        END

        IF @Amount <= 0
        BEGIN
            RAISERROR('Amount must be greater than zero.', 16, 1);
            RETURN;
        END

        -- Update balance
        UPDATE Accounts SET Balance = Balance + @Amount WHERE AccountNumber = @AccountNumber;

        -- Create transaction record
        INSERT INTO Transactions (AccountNumber, TransactionType, Amount, Description, TransactionDate, ReferenceNumber)
        VALUES (@AccountNumber, 'Deposit', @Amount, ISNULL(@Description, 'Cash Deposit'), GETDATE(), 'DEP' + REPLACE(CONVERT(NVARCHAR(20), GETDATE(), 120), ':', '') + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS NVARCHAR(5)));

        -- Log to audit with data signature
        DECLARE @DepSig NVARCHAR(64) = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @AccountNumber + CAST(@Amount AS NVARCHAR(30)) + CAST(GETDATE() AS NVARCHAR(30))), 2);
        INSERT INTO AuditLog (ActionType, Description, PerformedBy, ActionDate, DataSignature)
        VALUES ('Deposit', 'Deposit of PKR ' + CAST(@Amount AS NVARCHAR(20)) + ' to account ' + @AccountNumber, 'System', GETDATE(), @DepSig);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
    END CATCH
END
GO

-- sp_Withdraw
IF OBJECT_ID('sp_Withdraw', 'P') IS NOT NULL
    DROP PROCEDURE sp_Withdraw;
GO

CREATE PROCEDURE sp_Withdraw
    @AccountNumber NVARCHAR(20),
    @Amount        DECIMAL(18,2),
    @Description   NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Verify account
        DECLARE @Status NVARCHAR(20);
        DECLARE @Balance DECIMAL(18,2);
        SELECT @Status = Status, @Balance = Balance FROM Accounts WHERE AccountNumber = @AccountNumber;

        IF @Status IS NULL
        BEGIN
            SELECT 0 AS Success, 'Account not found.' AS Message;
            RETURN;
        END

        IF @Status != 'Active'
        BEGIN
            SELECT 0 AS Success, 'Account is not active. Cannot withdraw.' AS Message;
            RETURN;
        END

        IF @Amount <= 0
        BEGIN
            SELECT 0 AS Success, 'Amount must be greater than zero.' AS Message;
            RETURN;
        END

        IF @Balance < @Amount
        BEGIN
            SELECT 0 AS Success, 'Insufficient balance.' AS Message;
            RETURN;
        END

        -- Update balance
        UPDATE Accounts SET Balance = Balance - @Amount WHERE AccountNumber = @AccountNumber;

        -- Create transaction record
        INSERT INTO Transactions (AccountNumber, TransactionType, Amount, Description, TransactionDate, ReferenceNumber)
        VALUES (@AccountNumber, 'Withdrawal', @Amount, ISNULL(@Description, 'Cash Withdrawal'), GETDATE(), 'WD' + REPLACE(CONVERT(NVARCHAR(20), GETDATE(), 120), ':', '') + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS NVARCHAR(5)));

        -- Log to audit with data signature
        DECLARE @WdSig NVARCHAR(64) = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @AccountNumber + CAST(@Amount AS NVARCHAR(30)) + 'WD' + CAST(GETDATE() AS NVARCHAR(30))), 2);
        INSERT INTO AuditLog (ActionType, Description, PerformedBy, ActionDate, DataSignature)
        VALUES ('Withdrawal', 'Withdrawal of PKR ' + CAST(@Amount AS NVARCHAR(20)) + ' from account ' + @AccountNumber, 'System', GETDATE(), @WdSig);

        COMMIT TRANSACTION;

        SELECT 1 AS Success, 'Withdrawal successful.' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO

-- sp_Transfer
IF OBJECT_ID('sp_Transfer', 'P') IS NOT NULL
    DROP PROCEDURE sp_Transfer;
GO

CREATE PROCEDURE sp_Transfer
    @FromAccountNumber NVARCHAR(20),
    @ToAccountNumber   NVARCHAR(20),
    @Amount            DECIMAL(18,2),
    @Description       NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Verify source account
        DECLARE @FromStatus NVARCHAR(20);
        DECLARE @FromBalance DECIMAL(18,2);
        SELECT @FromStatus = Status, @FromBalance = Balance FROM Accounts WHERE AccountNumber = @FromAccountNumber;

        IF @FromStatus IS NULL
        BEGIN
            SELECT 0 AS Success, 'Source account not found.' AS Message;
            RETURN;
        END

        IF @FromStatus != 'Active'
        BEGIN
            SELECT 0 AS Success, 'Source account is not active.' AS Message;
            RETURN;
        END

        -- Verify destination account
        DECLARE @ToStatus NVARCHAR(20);
        SELECT @ToStatus = Status FROM Accounts WHERE AccountNumber = @ToAccountNumber;

        IF @ToStatus IS NULL
        BEGIN
            SELECT 0 AS Success, 'Destination account not found.' AS Message;
            RETURN;
        END

        IF @ToStatus != 'Active'
        BEGIN
            SELECT 0 AS Success, 'Destination account is not active.' AS Message;
            RETURN;
        END

        IF @Amount <= 0
        BEGIN
            SELECT 0 AS Success, 'Amount must be greater than zero.' AS Message;
            RETURN;
        END

        IF @FromBalance < @Amount
        BEGIN
            SELECT 0 AS Success, 'Insufficient balance.' AS Message;
            RETURN;
        END

        -- Debit source
        UPDATE Accounts SET Balance = Balance - @Amount WHERE AccountNumber = @FromAccountNumber;

        -- Credit destination
        UPDATE Accounts SET Balance = Balance + @Amount WHERE AccountNumber = @ToAccountNumber;

        -- Create transaction for source
        INSERT INTO Transactions (AccountNumber, TransactionType, Amount, Description, TransactionDate, ReferenceNumber)
        VALUES (@FromAccountNumber, 'Transfer Out', @Amount, ISNULL(@Description, 'Transfer to ' + @ToAccountNumber), GETDATE(), 'TRF' + REPLACE(CONVERT(NVARCHAR(20), GETDATE(), 120), ':', '') + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS NVARCHAR(5)));

        -- Create transaction for destination
        INSERT INTO Transactions (AccountNumber, TransactionType, Amount, Description, TransactionDate, ReferenceNumber)
        VALUES (@ToAccountNumber, 'Transfer In', @Amount, ISNULL(@Description, 'Transfer from ' + @FromAccountNumber), GETDATE(), 'TRF' + REPLACE(CONVERT(NVARCHAR(20), GETDATE(), 120), ':', '') + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS NVARCHAR(5)));

        -- Log to audit with data signature
        DECLARE @TrfSig NVARCHAR(64) = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @FromAccountNumber + @ToAccountNumber + CAST(@Amount AS NVARCHAR(30)) + 'TRF' + CAST(GETDATE() AS NVARCHAR(30))), 2);
        INSERT INTO AuditLog (ActionType, Description, PerformedBy, ActionDate, DataSignature)
        VALUES ('Transfer', 'Transfer of PKR ' + CAST(@Amount AS NVARCHAR(20)) + ' from ' + @FromAccountNumber + ' to ' + @ToAccountNumber, 'System', GETDATE(), @TrfSig);

        COMMIT TRANSACTION;

        SELECT 1 AS Success, 'Transfer successful.' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO

-- sp_ApplyForLoan
IF OBJECT_ID('sp_ApplyForLoan', 'P') IS NOT NULL
    DROP PROCEDURE sp_ApplyForLoan;
GO

CREATE PROCEDURE sp_ApplyForLoan
    @AccountNumber NVARCHAR(20),
    @LoanAmount    DECIMAL(18,2),
    @Purpose       NVARCHAR(200),
    @TenureMonths  INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Verify account
        DECLARE @Status NVARCHAR(20);
        SELECT @Status = Status FROM Accounts WHERE AccountNumber = @AccountNumber;

        IF @Status IS NULL
        BEGIN
            SELECT 0 AS Success, 'Account not found.' AS Message;
            RETURN;
        END

        IF @Status != 'Active'
        BEGIN
            SELECT 0 AS Success, 'Account is not active. Cannot apply for loan.' AS Message;
            RETURN;
        END

        IF @LoanAmount <= 0
        BEGIN
            SELECT 0 AS Success, 'Loan amount must be greater than zero.' AS Message;
            RETURN;
        END

        IF @TenureMonths <= 0
        BEGIN
            SELECT 0 AS Success, 'Tenure must be greater than zero.' AS Message;
            RETURN;
        END

        -- Insert loan application
        INSERT INTO LoanApplications (AccountNumber, LoanAmount, Purpose, TenureMonths, Status, ApplicationDate)
        VALUES (@AccountNumber, @LoanAmount, @Purpose, @TenureMonths, 'Pending', GETDATE());

        -- Log to audit with data signature
        DECLARE @LoanSig NVARCHAR(64) = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @AccountNumber + CAST(@LoanAmount AS NVARCHAR(30)) + @Purpose + 'LOAN' + CAST(GETDATE() AS NVARCHAR(30))), 2);
        INSERT INTO AuditLog (ActionType, Description, PerformedBy, ActionDate, DataSignature)
        VALUES ('LoanApplication', 'Loan application of PKR ' + CAST(@LoanAmount AS NVARCHAR(20)) + ' for ' + @Purpose + ' from account ' + @AccountNumber, 'System', GETDATE(), @LoanSig);

        COMMIT TRANSACTION;

        SELECT 1 AS Success, 'Loan application submitted successfully.' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO

-- sp_ApproveLoan
IF OBJECT_ID('sp_ApproveLoan', 'P') IS NOT NULL
    DROP PROCEDURE sp_ApproveLoan;
GO

CREATE PROCEDURE sp_ApproveLoan
    @LoanAppId      INT,
    @ApprovedAmount DECIMAL(18,2),
    @InterestRate   DECIMAL(5,2),
    @Notes          NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Get loan application details
        DECLARE @AccountNumber NVARCHAR(20);
        DECLARE @TenureMonths INT;
        DECLARE @CurrentStatus NVARCHAR(20);

        SELECT @AccountNumber = AccountNumber, @TenureMonths = TenureMonths, @CurrentStatus = Status
        FROM LoanApplications WHERE LoanAppId = @LoanAppId;

        IF @AccountNumber IS NULL
        BEGIN
            RAISERROR('Loan application not found.', 16, 1);
            RETURN;
        END

        IF @CurrentStatus != 'Pending'
        BEGIN
            RAISERROR('Loan application is not in Pending status.', 16, 1);
            RETURN;
        END

        -- Calculate EMI inline
        DECLARE @MonthlyRate DECIMAL(10,8);
        DECLARE @EMI DECIMAL(18,2);
        DECLARE @Factor DECIMAL(18,6);

        SET @MonthlyRate = @InterestRate / 100.0 / 12.0;
        IF @InterestRate = 0
            SET @EMI = @ApprovedAmount / @TenureMonths;
        ELSE
        BEGIN
            SET @Factor = POWER(1.0 + @MonthlyRate, @TenureMonths);
            SET @EMI = @ApprovedAmount * @MonthlyRate * @Factor / (@Factor - 1);
            SET @EMI = ROUND(@EMI, 2);
        END

        -- Update loan application
        UPDATE LoanApplications
        SET Status = 'Approved',
            ApprovedAmount = @ApprovedAmount,
            InterestRate = @InterestRate,
            ApprovedDate = GETDATE(),
            Notes = ISNULL(@Notes, 'Approved')
        WHERE LoanAppId = @LoanAppId;

        -- Insert into Loans table
        INSERT INTO Loans (LoanAppId, AccountNumber, Principal, InterestRate, TenureMonths, EMI, OutstandingBalance, StartDate, Status)
        VALUES (@LoanAppId, @AccountNumber, @ApprovedAmount, @InterestRate, @TenureMonths, @EMI, @ApprovedAmount, GETDATE(), 'Active');

        -- Credit loan amount to account
        UPDATE Accounts SET Balance = Balance + @ApprovedAmount WHERE AccountNumber = @AccountNumber;

        -- Create transaction for loan disbursement
        INSERT INTO Transactions (AccountNumber, TransactionType, Amount, Description, TransactionDate, ReferenceNumber)
        VALUES (@AccountNumber, 'Loan Disbursement', @ApprovedAmount, 'Loan approved - EMI: PKR ' + CAST(@EMI AS NVARCHAR(20)) + '/month for ' + CAST(@TenureMonths AS NVARCHAR(5)) + ' months', GETDATE(), 'LOAN' + REPLACE(CONVERT(NVARCHAR(20), GETDATE(), 120), ':', '') + CAST(@LoanAppId AS NVARCHAR(10)));

        -- Log to audit with data signature
        DECLARE @AppSig NVARCHAR(64) = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST(@LoanAppId AS NVARCHAR(10)) + CAST(@ApprovedAmount AS NVARCHAR(30)) + CAST(@InterestRate AS NVARCHAR(10)) + 'APPROVE' + CAST(GETDATE() AS NVARCHAR(30))), 2);
        INSERT INTO AuditLog (ActionType, Description, PerformedBy, ActionDate, DataSignature)
        VALUES ('LoanApproval', 'Loan #' + CAST(@LoanAppId AS NVARCHAR(10)) + ' approved for PKR ' + CAST(@ApprovedAmount AS NVARCHAR(20)) + ' at ' + CAST(@InterestRate AS NVARCHAR(10)) + '%', 'System', GETDATE(), @AppSig);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        RAISERROR(ERROR_MESSAGE(), 16, 1);
    END CATCH
END
GO

-- sp_RejectLoan
IF OBJECT_ID('sp_RejectLoan', 'P') IS NOT NULL
    DROP PROCEDURE sp_RejectLoan;
GO

CREATE PROCEDURE sp_RejectLoan
    @LoanAppId INT,
    @Notes     NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CurrentStatus NVARCHAR(20);
        SELECT @CurrentStatus = Status FROM LoanApplications WHERE LoanAppId = @LoanAppId;

        IF @CurrentStatus IS NULL
        BEGIN
            RAISERROR('Loan application not found.', 16, 1);
            RETURN;
        END

        IF @CurrentStatus != 'Pending'
        BEGIN
            RAISERROR('Loan application is not in Pending status.', 16, 1);
            RETURN;
        END

        -- Update loan application
        UPDATE LoanApplications
        SET Status = 'Rejected',
            Notes = ISNULL(@Notes, 'Rejected'),
            ApprovedDate = GETDATE()
        WHERE LoanAppId = @LoanAppId;

        -- Log to audit with data signature
        DECLARE @RejSig NVARCHAR(64) = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST(@LoanAppId AS NVARCHAR(10)) + 'REJECT' + CAST(GETDATE() AS NVARCHAR(30))), 2);
        INSERT INTO AuditLog (ActionType, Description, PerformedBy, ActionDate, DataSignature)
        VALUES ('LoanRejection', 'Loan #' + CAST(@LoanAppId AS NVARCHAR(10)) + ' rejected. Reason: ' + ISNULL(@Notes, 'N/A'), 'System', GETDATE(), @RejSig);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        RAISERROR(ERROR_MESSAGE(), 16, 1);
    END CATCH
END
GO
