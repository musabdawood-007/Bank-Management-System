-- ============================================================
-- 01_CreateDatabase.sql
-- Create the BankManagementDB database
-- ============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BankManagementDB')
BEGIN
    CREATE DATABASE BankManagementDB;
END
GO

USE BankManagementDB;
GO
