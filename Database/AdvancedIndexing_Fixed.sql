-- Advanced Database Indexing for Library Management System (Fixed Version)
-- فهرسة متقدمة لقاعدة البيانات لنظام إدارة المكتبة (نسخة محدثة)

USE LibraryManagementSystem;
GO

PRINT 'بدء إنشاء الفهارس المتقدمة - Starting advanced indexing...';
GO

-- إزالة الفهارس الموجودة إذا كانت موجودة
-- Drop existing indexes if they exist
IF EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_Books_Title_Author' AND object_id = OBJECT_ID('Books'))
    DROP INDEX IX_Books_Title_Author ON Books;

IF EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_Books_Search_Composite' AND object_id = OBJECT_ID('Books'))
    DROP INDEX IX_Books_Search_Composite ON Books;

IF EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_Books_Available_Covering' AND object_id = OBJECT_ID('Books'))
    DROP INDEX IX_Books_Available_Covering ON Books;

IF EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_Users_Email_Unique' AND object_id = OBJECT_ID('Users'))
    DROP INDEX IX_Users_Email_Unique ON Users;

IF EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_Borrowings_User_Status' AND object_id = OBJECT_ID('Borrowings'))
    DROP INDEX IX_Borrowings_User_Status ON Borrowings;

IF EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_Users_Active' AND object_id = OBJECT_ID('Users'))
    DROP INDEX IX_Users_Active ON Users;

IF EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_Borrowings_BorrowDate' AND object_id = OBJECT_ID('Borrowings'))
    DROP INDEX IX_Borrowings_BorrowDate ON Borrowings;

IF EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_Books_Dates' AND object_id = OBJECT_ID('Books'))
    DROP INDEX IX_Books_Dates ON Books;

IF EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_Users_Dates' AND object_id = OBJECT_ID('Users'))
    DROP INDEX IX_Users_Dates ON Users;

IF EXISTS (SELECT *
FROM sys.indexes
WHERE name = 'IX_Borrowings_Dates' AND object_id = OBJECT_ID('Borrowings'))
    DROP INDEX IX_Borrowings_Dates ON Borrowings;

PRINT 'تم حذف الفهارس الموجودة - Existing indexes dropped';
GO

-- فهارس الكتب المتقدمة
-- Advanced book indexes

-- 1. فهرس مركب للبحث بالعنوان والمؤلف
-- Composite index for title and author search
CREATE NONCLUSTERED INDEX IX_Books_Title_Author
ON Books (Title, Author)
INCLUDE (ISBN, Genre, TotalCopies, AvailableCopies, PublicationYear, Description);

-- 2. فهرس مركب شامل للبحث المتقدم
-- Comprehensive composite index for advanced search
CREATE NONCLUSTERED INDEX IX_Books_Search_Composite
ON Books (Genre, PublicationYear, AvailableCopies)
INCLUDE (BookId, Title, Author, ISBN, Publisher, TotalCopies, Description);

-- 3. فهرس تغطية للكتب المتاحة
-- Covering index for available books
CREATE NONCLUSTERED INDEX IX_Books_Available_Covering
ON Books (AvailableCopies)
INCLUDE (BookId, Title, Author, ISBN, Genre, TotalCopies, PublicationYear, Description)
WHERE AvailableCopies > 0;

-- 4. فهرس للبحث النصي في العنوان
-- Text search index for title
CREATE NONCLUSTERED INDEX IX_Books_Title_Text
ON Books (Title)
INCLUDE (Author, ISBN, AvailableCopies);

-- 5. فهرس للبحث النصي في المؤلف
-- Text search index for author
CREATE NONCLUSTERED INDEX IX_Books_Author_Text
ON Books (Author)
INCLUDE (Title, ISBN, AvailableCopies);

PRINT 'تم إنشاء فهارس الكتب المتقدمة - Advanced book indexes created';
GO

-- فهارس المستخدمين المتقدمة
-- Advanced user indexes

-- 1. فهرس فريد للبريد الإلكتروني للمستخدمين النشطين
-- Unique index for email for active users
CREATE UNIQUE NONCLUSTERED INDEX IX_Users_Email_Unique
ON Users (Email)
INCLUDE (UserId, FirstName, LastName, PhoneNumber)
WHERE IsActive = 1;

-- 2. فهرس للمستخدمين النشطين
-- Index for active users
CREATE NONCLUSTERED INDEX IX_Users_Active
ON Users (IsActive, MembershipDate)
INCLUDE (UserId, FirstName, LastName, Email, PhoneNumber);

-- 3. فهرس للبحث بالاسم
-- Index for name search
CREATE NONCLUSTERED INDEX IX_Users_Name_Search
ON Users (FirstName, LastName)
INCLUDE (Email, PhoneNumber, MembershipDate, IsActive);

PRINT 'تم إنشاء فهارس المستخدمين المتقدمة - Advanced user indexes created';
GO

-- فهارس الاستعارات المتقدمة
-- Advanced borrowing indexes

-- 1. فهرس مركب للمستخدم والحالة
-- Composite index for user and status
CREATE NONCLUSTERED INDEX IX_Borrowings_User_Status
ON Borrowings (UserId, IsReturned)
INCLUDE (BookId, BorrowDate, DueDate, ReturnDate, LateFee);

-- 2. فهرس مركب للكتاب والحالة
-- Composite index for book and status
CREATE NONCLUSTERED INDEX IX_Borrowings_Book_Status
ON Borrowings (BookId, IsReturned)
INCLUDE (UserId, BorrowDate, DueDate, ReturnDate);

-- 3. فهرس لتاريخ الاستحقاق والحالة (للكتب المتأخرة)
-- Index for due date and status (for overdue books)
CREATE NONCLUSTERED INDEX IX_Borrowings_DueDate_Status
ON Borrowings (DueDate, IsReturned)
INCLUDE (BorrowingId, UserId, BookId, BorrowDate, LateFee)
WHERE IsReturned = 0;

-- 4. فهرس لتاريخ الاستعارة
-- Index for borrow date
CREATE NONCLUSTERED INDEX IX_Borrowings_BorrowDate
ON Borrowings (BorrowDate DESC)
INCLUDE (UserId, BookId, DueDate, IsReturned);

-- 5. فهرس للاستعارات النشطة
-- Index for active borrowings
CREATE NONCLUSTERED INDEX IX_Borrowings_Active
ON Borrowings (IsReturned, BorrowDate)
INCLUDE (BorrowingId, UserId, BookId, DueDate)
WHERE IsReturned = 0;

-- 6. فهرس للإحصائيات الشهرية
-- Index for monthly statistics
CREATE NONCLUSTERED INDEX IX_Borrowings_Monthly_Stats
ON Borrowings (BorrowDate, IsReturned)
INCLUDE (UserId, BookId, ReturnDate, LateFee);

PRINT 'تم إنشاء فهارس الاستعارات المتقدمة - Advanced borrowing indexes created';
GO

-- فهارس للأداء العام
-- Indexes for general performance

-- 1. فهارس لتواريخ الإنشاء والتعديل
-- Indexes for creation and modification dates
CREATE NONCLUSTERED INDEX IX_Books_Dates
ON Books (CreatedDate, ModifiedDate);

CREATE NONCLUSTERED INDEX IX_Users_Dates
ON Users (CreatedDate, ModifiedDate);

CREATE NONCLUSTERED INDEX IX_Borrowings_Dates
ON Borrowings (CreatedDate, ModifiedDate);

PRINT 'تم إنشاء فهارس التواريخ - Date indexes created';
GO

-- إحصائيات الفهارس
-- Index statistics
PRINT 'إحصائيات الفهارس الجديدة - New index statistics:';

SELECT
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    CASE 
        WHEN i.has_filter = 1 THEN i.filter_definition 
        ELSE 'No Filter' 
    END AS FilterDefinition
FROM sys.indexes i
WHERE OBJECT_NAME(i.object_id) IN ('Books', 'Users', 'Borrowings')
    AND i.type > 0 -- Exclude heap
    AND i.name LIKE 'IX_%'
-- Only our custom indexes
ORDER BY OBJECT_NAME(i.object_id), i.name;

-- تحديث إحصائيات الفهارس
-- Update index statistics
UPDATE STATISTICS Books;
UPDATE STATISTICS Users;
UPDATE STATISTICS Borrowings;

PRINT 'تم تحديث إحصائيات الفهارس - Index statistics updated';
GO

-- إنشاء خطة صيانة للفهارس
-- Create index maintenance plan
PRINT 'توصيات صيانة الفهارس - Index maintenance recommendations:';
PRINT '1. قم بإعادة بناء الفهارس أسبوعياً للجداول الكبيرة';
PRINT '2. قم بتحديث الإحصائيات يومياً';
PRINT '3. راقب استخدام الفهارس باستخدام DMVs';
PRINT '4. احذف الفهارس غير المستخدمة';

PRINT 'تم الانتهاء من إنشاء الفهارس المتقدمة بنجاح! - Advanced indexing completed successfully!';

-- استعلام لمراقبة استخدام الفهارس
-- Query to monitor index usage
PRINT '';
PRINT 'لمراقبة استخدام الفهارس، استخدم الاستعلام التالي:';
PRINT 'To monitor index usage, use the following query:';
PRINT '';
PRINT 'SELECT ';
PRINT '    OBJECT_NAME(s.object_id) AS TableName,';
PRINT '    i.name AS IndexName,';
PRINT '    s.user_seeks,';
PRINT '    s.user_scans,';
PRINT '    s.user_lookups,';
PRINT '    s.user_updates,';
PRINT '    s.last_user_seek,';
PRINT '    s.last_user_scan,';
PRINT '    s.last_user_lookup';
PRINT 'FROM sys.dm_db_index_usage_stats s';
PRINT 'INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id';
PRINT 'WHERE OBJECT_NAME(s.object_id) IN (''Books'', ''Users'', ''Borrowings'')';
PRINT 'ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;';

GO
