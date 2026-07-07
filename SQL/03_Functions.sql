-- ============================================================
-- 03_Functions.sql
-- Create scalar and table-valued functions
-- ============================================================

USE BankManagementDB;
GO

-- fn_GenerateAccountNumber: generates BMS + 11 random digits
IF OBJECT_ID('dbo.fn_GenerateAccountNumber', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GenerateAccountNumber;
GO

CREATE FUNCTION dbo.fn_GenerateAccountNumber()
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @AccountNumber NVARCHAR(20);
    DECLARE @RandomPart NVARCHAR(11);
    DECLARE @Exists BIT = 1;

    WHILE @Exists = 1
    BEGIN
        SET @RandomPart = RIGHT('00000000000' + CAST(ABS(CHECKSUM(NEWID())) % 100000000000 AS NVARCHAR(11)), 11);
        SET @AccountNumber = 'BMS' + @RandomPart;

        IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountNumber = @AccountNumber)
            SET @Exists = 0;
    END

    RETURN @AccountNumber;
END
GO

-- fn_CalculateEMI: calculates EMI using standard formula
-- EMI = P * r * (1+r)^n / ((1+r)^n - 1)
-- where r = monthly interest rate, n = number of months
IF OBJECT_ID('dbo.fn_CalculateEMI', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_CalculateEMI;
GO

CREATE FUNCTION dbo.fn_CalculateEMI(
    @Principal DECIMAL(18,2),
    @AnnualRate DECIMAL(5,2),
    @TenureMonths INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @MonthlyRate DECIMAL(10,8);
    DECLARE @EMI DECIMAL(18,2);
    DECLARE @Factor DECIMAL(18,6);

    IF @AnnualRate = 0 OR @TenureMonths = 0
    BEGIN
        IF @TenureMonths > 0
            SET @EMI = @Principal / @TenureMonths;
        ELSE
            SET @EMI = @Principal;
        RETURN @EMI;
    END

    SET @MonthlyRate = @AnnualRate / 100.0 / 12.0;
    SET @Factor = POWER(1.0 + @MonthlyRate, @TenureMonths);
    SET @EMI = @Principal * @MonthlyRate * @Factor / (@Factor - 1);

    RETURN ROUND(@EMI, 2);
END
GO
