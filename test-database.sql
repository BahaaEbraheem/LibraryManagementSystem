-- اختبار قاعدة البيانات - Database Test
-- استخدم هذا الملف في SSMS للتحقق من قاعدة البيانات

USE LibraryManagementSystem;

-- عرض معلومات قاعدة البيانات
SELECT 
    DB_NAME() as CurrentDatabase,
    @@SERVERNAME as ServerName,
    GETDATE() as CurrentDateTime;

-- عرض الجداول الموجودة
SELECT 
    TABLE_NAME as TableName,
    TABLE_TYPE as TableType
FROM INFORMATION_SCHEMA.TABLES
ORDER BY TABLE_NAME;

-- عرض إحصائيات البيانات
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Books' as TableName, COUNT(*) as RecordCount FROM Books  
UNION ALL
SELECT 'Borrowings' as TableName, COUNT(*) as RecordCount FROM Borrowings;

-- عرض المستخدمين
SELECT 
    UserId,
    FirstName + ' ' + LastName as FullName,
    Email,
    PhoneNumber,
    IsActive,
    MembershipDate
FROM Users
ORDER BY UserId;

-- عرض الكتب
SELECT 
    BookId,
    Title,
    Author,
    ISBN,
    Genre,
    TotalCopies,
    AvailableCopies,
    PublicationYear
FROM Books
ORDER BY BookId;

-- عرض الاستعارات (إن وجدت)
SELECT 
    BorrowingId,
    u.FirstName + ' ' + u.LastName as BorrowerName,
    b.Title as BookTitle,
    BorrowDate,
    DueDate,
    ReturnDate,
    IsReturned
FROM Borrowings br
LEFT JOIN Users u ON br.UserId = u.UserId
LEFT JOIN Books b ON br.BookId = b.BookId
ORDER BY BorrowingId;

-- عرض الفهارس
SELECT 
    i.name as IndexName,
    t.name as TableName,
    i.type_desc as IndexType
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name IS NOT NULL
ORDER BY t.name, i.name;
