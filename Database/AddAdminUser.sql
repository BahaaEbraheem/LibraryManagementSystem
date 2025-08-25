-- Add Admin User Script
-- This script adds a default administrator user to the system

USE LibraryManagementSystem;
GO

-- Check if admin user already exists
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@library.com')
BEGIN
    -- Insert admin user
    -- Password: admin123 (hashed)
    INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, Address, MembershipDate, IsActive, PasswordHash, Role)
    VALUES ('Admin', 'User', 'admin@library.com', '555-0001', 'Admin Office', '2023-01-01', 1, 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 2);
    
    PRINT 'Admin user created successfully!';
    PRINT 'Email: admin@library.com';
    PRINT 'Password: admin123';
END
ELSE
BEGIN
    PRINT 'Admin user already exists!';
END
GO
