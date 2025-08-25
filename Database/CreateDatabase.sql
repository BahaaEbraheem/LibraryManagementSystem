-- Library Management System Database Schema
-- This script creates the database and all required tables

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'LibraryManagementSystem')
BEGIN
    CREATE DATABASE LibraryManagementSystem;
END
GO

USE LibraryManagementSystem;
GO

-- Create Users Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PhoneNumber NVARCHAR(15),
        Address NVARCHAR(200),
        MembershipDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        PasswordHash NVARCHAR(255) NOT NULL DEFAULT '',
        Role INT NOT NULL DEFAULT 1, -- 1 = User, 2 = Administrator
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

        CONSTRAINT CHK_Users_Role CHECK (Role IN (1, 2))
    );
END

-- Add columns if they don't exist (for existing databases)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE Users ADD PasswordHash NVARCHAR(255) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Role')
BEGIN
    ALTER TABLE Users ADD Role INT NOT NULL DEFAULT 1;
    ALTER TABLE Users ADD CONSTRAINT CHK_Users_Role CHECK (Role IN (1, 2));
END
GO

-- Create Books Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Books' AND xtype='U')
BEGIN
    CREATE TABLE Books (
        BookId INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Author NVARCHAR(100) NOT NULL,
        ISBN NVARCHAR(20) NOT NULL UNIQUE,
        Publisher NVARCHAR(100),
        PublicationYear INT,
        Genre NVARCHAR(50),
        TotalCopies INT NOT NULL DEFAULT 1,
        AvailableCopies INT NOT NULL DEFAULT 1,
        Description NVARCHAR(500),
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT CHK_Books_TotalCopies CHECK (TotalCopies >= 0),
        CONSTRAINT CHK_Books_AvailableCopies CHECK (AvailableCopies >= 0),
        CONSTRAINT CHK_Books_AvailableCopies_LTE_Total CHECK (AvailableCopies <= TotalCopies),
        CONSTRAINT CHK_Books_PublicationYear CHECK (PublicationYear >= 1000 AND PublicationYear <= YEAR(GETDATE()))
    );
END
GO

-- Create Borrowings Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Borrowings' AND xtype='U')
BEGIN
    CREATE TABLE Borrowings (
        BorrowingId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        BookId INT NOT NULL,
        BorrowDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        DueDate DATETIME2 NOT NULL,
        ReturnDate DATETIME2 NULL,
        IsReturned BIT NOT NULL DEFAULT 0,
        LateFee DECIMAL(10,2) DEFAULT 0,
        Notes NVARCHAR(200),
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT FK_Borrowings_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
        CONSTRAINT FK_Borrowings_Books FOREIGN KEY (BookId) REFERENCES Books(BookId),
        CONSTRAINT CHK_Borrowings_DueDate CHECK (DueDate > BorrowDate),
        CONSTRAINT CHK_Borrowings_ReturnDate CHECK (ReturnDate IS NULL OR ReturnDate >= BorrowDate),
        CONSTRAINT CHK_Borrowings_LateFee CHECK (LateFee >= 0)
    );
END
GO

-- Create Indexes for better performance
CREATE NONCLUSTERED INDEX IX_Books_Title ON Books(Title);
CREATE NONCLUSTERED INDEX IX_Books_Author ON Books(Author);
CREATE NONCLUSTERED INDEX IX_Books_ISBN ON Books(ISBN);
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
CREATE NONCLUSTERED INDEX IX_Borrowings_UserId ON Borrowings(UserId);
CREATE NONCLUSTERED INDEX IX_Borrowings_BookId ON Borrowings(BookId);
CREATE NONCLUSTERED INDEX IX_Borrowings_BorrowDate ON Borrowings(BorrowDate);
CREATE NONCLUSTERED INDEX IX_Borrowings_IsReturned ON Borrowings(IsReturned);
GO

-- Create triggers for automatic timestamp updates
CREATE OR ALTER TRIGGER TR_Users_UpdateModifiedDate
ON Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users 
    SET ModifiedDate = GETDATE()
    FROM Users u
    INNER JOIN inserted i ON u.UserId = i.UserId;
END
GO

CREATE OR ALTER TRIGGER TR_Books_UpdateModifiedDate
ON Books
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Books 
    SET ModifiedDate = GETDATE()
    FROM Books b
    INNER JOIN inserted i ON b.BookId = i.BookId;
END
GO

CREATE OR ALTER TRIGGER TR_Borrowings_UpdateModifiedDate
ON Borrowings
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Borrowings 
    SET ModifiedDate = GETDATE()
    FROM Borrowings br
    INNER JOIN inserted i ON br.BorrowingId = i.BorrowingId;
END
GO

-- Create trigger to automatically update book availability when borrowing/returning
CREATE OR ALTER TRIGGER TR_Borrowings_UpdateBookAvailability
ON Borrowings
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Handle new borrowings (INSERT)
    IF EXISTS (SELECT * FROM inserted) AND NOT EXISTS (SELECT * FROM deleted)
    BEGIN
        UPDATE Books 
        SET AvailableCopies = AvailableCopies - 1
        FROM Books b
        INNER JOIN inserted i ON b.BookId = i.BookId
        WHERE i.IsReturned = 0;
    END
    
    -- Handle returns (UPDATE where IsReturned changes from 0 to 1)
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        UPDATE Books 
        SET AvailableCopies = AvailableCopies + 1
        FROM Books b
        INNER JOIN inserted i ON b.BookId = i.BookId
        INNER JOIN deleted d ON i.BorrowingId = d.BorrowingId
        WHERE i.IsReturned = 1 AND d.IsReturned = 0;
    END
END
GO

PRINT 'Database schema created successfully!';
