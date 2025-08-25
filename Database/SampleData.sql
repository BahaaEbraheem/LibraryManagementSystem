-- Sample Data for Library Management System
-- This script inserts test data into the database

USE LibraryManagementSystem;
GO

-- Insert Sample Users (including admin)
-- Note: Password for admin is 'admin123' (hashed)
INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, Address, MembershipDate, IsActive, PasswordHash, Role)
VALUES
    -- Administrator user (password: admin123)
    ('مدير', 'النظام', 'admin@library.com', '555-0001', 'مكتب الإدارة', '2023-01-01', 1, 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 2),
    -- Regular users (password: user123)
    ('أحمد', 'محمد', 'ahmed.mohamed@email.com', '555-0101', '123 شارع الرئيسي، المدينة', '2023-01-15', 1, 'Et6pb+wgWTVmq9PtQ6Gi/qRvlzrS+Ph3/A6h81pNBSY=', 1),
    ('فاطمة', 'علي', 'fatima.ali@email.com', '555-0102', '456 شارع البلوط، المدينة', '2023-02-20', 1, 'Et6pb+wgWTVmq9PtQ6Gi/qRvlzrS+Ph3/A6h81pNBSY=', 1),
    ('محمد', 'أحمد', 'mohamed.ahmed@email.com', '555-0103', '789 شارع الصنوبر، المدينة', '2023-03-10', 1, 'Et6pb+wgWTVmq9PtQ6Gi/qRvlzrS+Ph3/A6h81pNBSY=', 1),
    ('عائشة', 'حسن', 'aisha.hassan@email.com', '555-0104', '321 شارع الدردار، المدينة', '2023-04-05', 1, 'Et6pb+wgWTVmq9PtQ6Gi/qRvlzrS+Ph3/A6h81pNBSY=', 1),
    ('عبدالله', 'سالم', 'abdullah.salem@email.com', '555-0105', '654 شارع القيقب، المدينة', '2023-05-12', 1, 'Et6pb+wgWTVmq9PtQ6Gi/qRvlzrS+Ph3/A6h81pNBSY=', 1),
    ('مريم', 'خالد', 'mariam.khaled@email.com', '555-0106', '987 شارع الأرز، المدينة', '2023-06-18', 1, 'Et6pb+wgWTVmq9PtQ6Gi/qRvlzrS+Ph3/A6h81pNBSY=', 1),
    ('يوسف', 'عمر', 'youssef.omar@email.com', '555-0107', '147 شارع البتولا، المدينة', '2023-07-22', 1, 'Et6pb+wgWTVmq9PtQ6Gi/qRvlzrS+Ph3/A6h81pNBSY=', 1),
    ('زينب', 'ناصر', 'zeinab.nasser@email.com', '555-0108', '258 شارع التنوب، المدينة', '2023-08-30', 1, 'Et6pb+wgWTVmq9PtQ6Gi/qRvlzrS+Ph3/A6h81pNBSY=', 1);
GO

-- Insert Sample Books
INSERT INTO Books (Title, Author, ISBN, Publisher, PublicationYear, Genre, TotalCopies, AvailableCopies, Description)
VALUES 
    ('The Great Gatsby', 'F. Scott Fitzgerald', '978-0-7432-7356-5', 'Scribner', 1925, 'Fiction', 3, 3, 'A classic American novel set in the Jazz Age.'),
    ('To Kill a Mockingbird', 'Harper Lee', '978-0-06-112008-4', 'J.B. Lippincott & Co.', 1960, 'Fiction', 2, 2, 'A gripping tale of racial injustice and childhood innocence.'),
    ('1984', 'George Orwell', '978-0-452-28423-4', 'Secker & Warburg', 1949, 'Dystopian Fiction', 4, 4, 'A dystopian social science fiction novel.'),
    ('Pride and Prejudice', 'Jane Austen', '978-0-14-143951-8', 'T. Egerton', 1813, 'Romance', 2, 2, 'A romantic novel of manners.'),
    ('The Catcher in the Rye', 'J.D. Salinger', '978-0-316-76948-0', 'Little, Brown and Company', 1951, 'Fiction', 3, 3, 'A controversial novel about teenage rebellion.'),
    ('Lord of the Flies', 'William Golding', '978-0-571-05686-2', 'Faber & Faber', 1954, 'Fiction', 2, 2, 'A novel about British boys stranded on an uninhabited island.'),
    ('The Hobbit', 'J.R.R. Tolkien', '978-0-547-92822-7', 'George Allen & Unwin', 1937, 'Fantasy', 3, 3, 'A fantasy novel about the adventures of Bilbo Baggins.'),
    ('Harry Potter and the Philosopher''s Stone', 'J.K. Rowling', '978-0-7475-3269-9', 'Bloomsbury', 1997, 'Fantasy', 5, 5, 'The first book in the Harry Potter series.'),
    ('The Da Vinci Code', 'Dan Brown', '978-0-385-50420-1', 'Doubleday', 2003, 'Mystery', 2, 2, 'A mystery thriller novel.'),
    ('The Alchemist', 'Paulo Coelho', '978-0-06-231500-7', 'HarperOne', 1988, 'Fiction', 3, 3, 'A philosophical novel about following your dreams.'),
    ('Brave New World', 'Aldous Huxley', '978-0-06-085052-4', 'Chatto & Windus', 1932, 'Science Fiction', 2, 2, 'A dystopian novel set in a futuristic society.'),
    ('The Lord of the Rings: The Fellowship of the Ring', 'J.R.R. Tolkien', '978-0-547-92832-6', 'George Allen & Unwin', 1954, 'Fantasy', 2, 2, 'The first volume of The Lord of the Rings.'),
    ('Animal Farm', 'George Orwell', '978-0-452-28424-1', 'Secker & Warburg', 1945, 'Political Satire', 3, 3, 'An allegorical novella about farm animals.'),
    ('The Chronicles of Narnia: The Lion, the Witch and the Wardrobe', 'C.S. Lewis', '978-0-06-764898-2', 'Geoffrey Bles', 1950, 'Fantasy', 2, 2, 'A fantasy novel for children.'),
    ('Fahrenheit 451', 'Ray Bradbury', '978-1-4516-7331-9', 'Ballantine Books', 1953, 'Dystopian Fiction', 2, 2, 'A dystopian novel about book burning.'),
    ('The Kite Runner', 'Khaled Hosseini', '978-1-59448-000-3', 'Riverhead Books', 2003, 'Historical Fiction', 2, 2, 'A novel about friendship and redemption in Afghanistan.'),
    ('Life of Pi', 'Yann Martel', '978-0-15-100811-7', 'Knopf Canada', 2001, 'Adventure Fiction', 2, 2, 'A novel about a boy stranded on a lifeboat with a tiger.'),
    ('The Book Thief', 'Markus Zusak', '978-0-375-83100-3', 'Picador', 2005, 'Historical Fiction', 2, 2, 'A novel set in Nazi Germany narrated by Death.'),
    ('Gone Girl', 'Gillian Flynn', '978-0-307-58836-4', 'Crown Publishing Group', 2012, 'Psychological Thriller', 3, 3, 'A psychological thriller about a missing wife.'),
    ('The Girl with the Dragon Tattoo', 'Stieg Larsson', '978-0-307-45454-1', 'Norstedts Förlag', 2005, 'Crime Fiction', 2, 2, 'A crime novel featuring Lisbeth Salander.');
GO

-- Insert Sample Borrowings (some active, some returned)
INSERT INTO Borrowings (UserId, BookId, BorrowDate, DueDate, ReturnDate, IsReturned, LateFee, Notes)
VALUES 
    -- Active borrowings
    (1, 1, '2024-08-01', '2024-08-15', NULL, 0, 0, 'First borrowing'),
    (2, 3, '2024-08-05', '2024-08-19', NULL, 0, 0, 'Student research'),
    (3, 8, '2024-08-10', '2024-08-24', NULL, 0, 0, 'Summer reading'),
    (4, 19, '2024-08-12', '2024-08-26', NULL, 0, 0, 'Book club selection'),
    
    -- Returned borrowings
    (1, 2, '2024-07-01', '2024-07-15', '2024-07-14', 1, 0, 'Returned on time'),
    (2, 5, '2024-07-05', '2024-07-19', '2024-07-20', 1, 2.50, 'Returned late'),
    (3, 7, '2024-07-10', '2024-07-24', '2024-07-22', 1, 0, 'Excellent condition'),
    (5, 10, '2024-07-15', '2024-07-29', '2024-07-28', 1, 0, 'Great book!'),
    (6, 12, '2024-07-20', '2024-08-03', '2024-08-01', 1, 0, 'Enjoyed reading'),
    (7, 15, '2024-07-25', '2024-08-08', '2024-08-10', 1, 5.00, 'Returned late'),
    
    -- More historical borrowings
    (8, 4, '2024-06-01', '2024-06-15', '2024-06-14', 1, 0, 'Classic literature'),
    (1, 6, '2024-06-10', '2024-06-24', '2024-06-23', 1, 0, 'Second borrowing'),
    (4, 9, '2024-06-15', '2024-06-29', '2024-07-01', 1, 1.00, 'Slightly late'),
    (5, 11, '2024-06-20', '2024-07-04', '2024-07-03', 1, 0, 'Science fiction fan'),
    (6, 13, '2024-06-25', '2024-07-09', '2024-07-08', 1, 0, 'Children''s literature');
GO

-- Update available copies based on active borrowings
-- This is handled automatically by the trigger, but let's verify the counts
UPDATE Books 
SET AvailableCopies = TotalCopies - (
    SELECT COUNT(*) 
    FROM Borrowings 
    WHERE Borrowings.BookId = Books.BookId 
    AND IsReturned = 0
);
GO

-- Display summary information
SELECT 'Database populated successfully!' as Message;

SELECT 
    'Users' as TableName,
    COUNT(*) as RecordCount
FROM Users
UNION ALL
SELECT 
    'Books' as TableName,
    COUNT(*) as RecordCount
FROM Books
UNION ALL
SELECT 
    'Borrowings' as TableName,
    COUNT(*) as RecordCount
FROM Borrowings;

-- Show current borrowing status
SELECT 
    b.Title,
    b.TotalCopies,
    b.AvailableCopies,
    (b.TotalCopies - b.AvailableCopies) as CurrentlyBorrowed
FROM Books b
ORDER BY b.Title;
GO
