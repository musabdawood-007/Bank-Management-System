-- ============================================================
-- 06_Security.sql
-- Insert admin user and set permissions
-- ============================================================

USE BankManagementDB;
GO

-- Insert default admin (if not exists)
IF NOT EXISTS (SELECT 1 FROM Admins WHERE Email = 'admin@bms.com')
BEGIN
    INSERT INTO Admins (FullName, Email, PasswordHash, IsActive, CreatedDate)
    VALUES ('System Administrator', 'admin@bms.com', 'admin12345', 1, GETDATE());
END
GO

-- Grant EXECUTE on stored procedures
BEGIN TRY
    GRANT EXECUTE ON sp_RegisterCustomer TO public;
END TRY
BEGIN CATCH
    -- Ignore permission errors
    PRINT 'Could not grant execute on sp_RegisterCustomer';
END CATCH
GO

BEGIN TRY
    GRANT EXECUTE ON sp_Deposit TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant execute on sp_Deposit';
END CATCH
GO

BEGIN TRY
    GRANT EXECUTE ON sp_Withdraw TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant execute on sp_Withdraw';
END CATCH
GO

BEGIN TRY
    GRANT EXECUTE ON sp_Transfer TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant execute on sp_Transfer';
END CATCH
GO

BEGIN TRY
    GRANT EXECUTE ON sp_ApplyForLoan TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant execute on sp_ApplyForLoan';
END CATCH
GO

BEGIN TRY
    GRANT EXECUTE ON sp_ApproveLoan TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant execute on sp_ApproveLoan';
END CATCH
GO

BEGIN TRY
    GRANT EXECUTE ON sp_RejectLoan TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant execute on sp_RejectLoan';
END CATCH
GO

-- Grant SELECT on views
BEGIN TRY
    GRANT SELECT ON vw_CustomerDetails TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant select on vw_CustomerDetails';
END CATCH
GO

BEGIN TRY
    GRANT SELECT ON vw_TransactionSummary TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant select on vw_TransactionSummary';
END CATCH
GO

BEGIN TRY
    GRANT SELECT ON vw_LoanSummary TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant select on vw_LoanSummary';
END CATCH
GO

-- Grant SELECT on functions
BEGIN TRY
    GRANT EXECUTE ON dbo.fn_GenerateAccountNumber TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant execute on fn_GenerateAccountNumber';
END CATCH
GO

BEGIN TRY
    GRANT EXECUTE ON dbo.fn_CalculateEMI TO public;
END TRY
BEGIN CATCH
    PRINT 'Could not grant execute on fn_CalculateEMI';
END CATCH
GO
