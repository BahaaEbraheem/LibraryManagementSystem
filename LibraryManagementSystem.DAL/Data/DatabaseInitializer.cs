using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace LibraryManagementSystem.DAL.Data
{
    /// <summary>
    /// فئة إدارة وتهيئة قاعدة البيانات التلقائية
    /// Database initialization and management class
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(string connectionString, ILogger<DatabaseInitializer> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// تهيئة قاعدة البيانات بالكامل - إنشاء قاعدة البيانات والجداول والبيانات الأولية
        /// Initialize the complete database - create database, tables, and initial data
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("بدء تهيئة قاعدة البيانات - Starting database initialization");

                // 1. إنشاء قاعدة البيانات إذا لم تكن موجودة
                await CreateDatabaseIfNotExistsAsync();

                // 2. إنشاء الجداول إذا لم تكن موجودة
                await CreateTablesIfNotExistAsync();

                // 3. إنشاء الفهارس المتقدمة
                await CreateAdvancedIndexesAsync();

                // 4. إدراج البيانات الأولية إذا لم تكن موجودة
                await InsertInitialDataIfNotExistsAsync();

                _logger.LogInformation("تم إكمال تهيئة قاعدة البيانات بنجاح - Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في تهيئة قاعدة البيانات - Failed to initialize database");
                throw;
            }
        }

        /// <summary>
        /// إنشاء قاعدة البيانات إذا لم تكن موجودة
        /// Create database if it doesn't exist
        /// </summary>
        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                var databaseName = GetDatabaseName();
                _logger.LogDebug("فحص وجود قاعدة البيانات: {DatabaseName} - Checking database existence: {DatabaseName}", databaseName, databaseName);

                using var masterConnection = await CreateMasterConnectionAsync();

                var checkDbSql = "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName";
                var dbExists = await ExecuteScalarAsync<int>(masterConnection, checkDbSql, new { DatabaseName = databaseName });

                if (dbExists == 0)
                {
                    _logger.LogInformation("إنشاء قاعدة البيانات: {DatabaseName} - Creating database: {DatabaseName}", databaseName, databaseName);

                    var createDbSql = $"CREATE DATABASE [{databaseName}]";
                    await ExecuteNonQueryAsync(masterConnection, createDbSql);

                    _logger.LogInformation("تم إنشاء قاعدة البيانات بنجاح - Database created successfully");
                }
                else
                {
                    _logger.LogDebug("قاعدة البيانات موجودة بالفعل - Database already exists");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في إنشاء قاعدة البيانات - Failed to create database");
                throw;
            }
        }

        /// <summary>
        /// إنشاء الجداول إذا لم تكن موجودة
        /// Create tables if they don't exist
        /// </summary>
        private async Task CreateTablesIfNotExistAsync()
        {
            try
            {
                _logger.LogDebug("فحص وإنشاء الجداول - Checking and creating tables");

                using var connection = await CreateConnectionAsync();

                // إنشاء جدول المستخدمين
                await CreateUsersTableAsync(connection);

                // إنشاء جدول الكتب
                await CreateBooksTableAsync(connection);

                // إنشاء جدول الاستعارات
                await CreateBorrowingsTableAsync(connection);

                // إنشاء المشغلات التلقائية
                await CreateTriggersAsync(connection);

                _logger.LogInformation("تم إنشاء جميع الجداول بنجاح - All tables created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في إنشاء الجداول - Failed to create tables");
                throw;
            }
        }

        /// <summary>
        /// إنشاء جدول المستخدمين
        /// Create Users table
        /// </summary>
        private async Task CreateUsersTableAsync(IDbConnection connection)
        {
            const string checkTableSql = @"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'Users' AND TABLE_SCHEMA = 'dbo'";

            var tableExists = await ExecuteScalarAsync<int>(connection, checkTableSql);

            if (tableExists == 0)
            {
                _logger.LogDebug("إنشاء جدول المستخدمين - Creating Users table");

                const string createTableSql = @"
                    CREATE TABLE Users (
                        UserId INT IDENTITY(1,1) PRIMARY KEY,
                        FirstName NVARCHAR(50) NOT NULL,
                        LastName NVARCHAR(50) NOT NULL,
                        Email NVARCHAR(100) NOT NULL UNIQUE,
                        PhoneNumber NVARCHAR(15),
                        Address NVARCHAR(200),
                        MembershipDate DATETIME2 NOT NULL DEFAULT GETDATE(),
                        IsActive BIT NOT NULL DEFAULT 1,
                        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
                        ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE()
                    )";

                await ExecuteNonQueryAsync(connection, createTableSql);
                _logger.LogDebug("تم إنشاء جدول المستخدمين - Users table created");
            }
        }

        /// <summary>
        /// إنشاء جدول الكتب
        /// Create Books table
        /// </summary>
        private async Task CreateBooksTableAsync(IDbConnection connection)
        {
            const string checkTableSql = @"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'Books' AND TABLE_SCHEMA = 'dbo'";

            var tableExists = await ExecuteScalarAsync<int>(connection, checkTableSql);

            if (tableExists == 0)
            {
                _logger.LogDebug("إنشاء جدول الكتب - Creating Books table");

                const string createTableSql = @"
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
                    )";

                await ExecuteNonQueryAsync(connection, createTableSql);
                _logger.LogDebug("تم إنشاء جدول الكتب - Books table created");
            }
        }

        /// <summary>
        /// إنشاء جدول الاستعارات
        /// Create Borrowings table
        /// </summary>
        private async Task CreateBorrowingsTableAsync(IDbConnection connection)
        {
            const string checkTableSql = @"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'Borrowings' AND TABLE_SCHEMA = 'dbo'";

            var tableExists = await ExecuteScalarAsync<int>(connection, checkTableSql);

            if (tableExists == 0)
            {
                _logger.LogDebug("إنشاء جدول الاستعارات - Creating Borrowings table");

                const string createTableSql = @"
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
                    )";

                await ExecuteNonQueryAsync(connection, createTableSql);
                _logger.LogDebug("تم إنشاء جدول الاستعارات - Borrowings table created");
            }
        }

        /// <summary>
        /// إنشاء اتصال بقاعدة البيانات الرئيسية
        /// Create master database connection
        /// </summary>
        private async Task<SqlConnection> CreateMasterConnectionAsync()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            builder.InitialCatalog = "master";

            var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
            return connection;
        }

        /// <summary>
        /// إنشاء اتصال بقاعدة البيانات المحددة
        /// Create connection to specified database
        /// </summary>
        private async Task<SqlConnection> CreateConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        /// <summary>
        /// الحصول على اسم قاعدة البيانات من سلسلة الاتصال
        /// Get database name from connection string
        /// </summary>
        private string GetDatabaseName()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            return builder.InitialCatalog;
        }

        /// <summary>
        /// تنفيذ استعلام scalar
        /// Execute scalar query
        /// </summary>
        private async Task<T?> ExecuteScalarAsync<T>(IDbConnection connection, string sql, object? parameters = null)
        {
            return await DatabaseHelper.ExecuteScalarAsync<T>(connection, sql, parameters);
        }

        /// <summary>
        /// تنفيذ أمر غير استعلامي
        /// Execute non-query command
        /// </summary>
        private async Task<int> ExecuteNonQueryAsync(IDbConnection connection, string sql, object? parameters = null)
        {
            return await DatabaseHelper.ExecuteNonQueryAsync(connection, sql, parameters);
        }

        /// <summary>
        /// إنشاء المشغلات التلقائية
        /// Create automatic triggers
        /// </summary>
        private async Task CreateTriggersAsync(IDbConnection connection)
        {
            try
            {
                _logger.LogDebug("إنشاء المشغلات التلقائية - Creating automatic triggers");

                // مشغل تحديث تاريخ التعديل للمستخدمين
                const string usersTrigger = @"
                    IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_UpdateModifiedDate')
                    BEGIN
                        EXEC('
                        CREATE TRIGGER TR_Users_UpdateModifiedDate
                        ON Users
                        AFTER UPDATE
                        AS
                        BEGIN
                            SET NOCOUNT ON;
                            UPDATE Users
                            SET ModifiedDate = GETDATE()
                            FROM Users u
                            INNER JOIN inserted i ON u.UserId = i.UserId;
                        END')
                    END";

                await ExecuteNonQueryAsync(connection, usersTrigger);

                // مشغل تحديث تاريخ التعديل للكتب
                const string booksTrigger = @"
                    IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Books_UpdateModifiedDate')
                    BEGIN
                        EXEC('
                        CREATE TRIGGER TR_Books_UpdateModifiedDate
                        ON Books
                        AFTER UPDATE
                        AS
                        BEGIN
                            SET NOCOUNT ON;
                            UPDATE Books
                            SET ModifiedDate = GETDATE()
                            FROM Books b
                            INNER JOIN inserted i ON b.BookId = i.BookId;
                        END')
                    END";

                await ExecuteNonQueryAsync(connection, booksTrigger);

                // مشغل تحديث تاريخ التعديل للاستعارات
                const string borrowingsTrigger = @"
                    IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Borrowings_UpdateModifiedDate')
                    BEGIN
                        EXEC('
                        CREATE TRIGGER TR_Borrowings_UpdateModifiedDate
                        ON Borrowings
                        AFTER UPDATE
                        AS
                        BEGIN
                            SET NOCOUNT ON;
                            UPDATE Borrowings
                            SET ModifiedDate = GETDATE()
                            FROM Borrowings br
                            INNER JOIN inserted i ON br.BorrowingId = i.BorrowingId;
                        END')
                    END";

                await ExecuteNonQueryAsync(connection, borrowingsTrigger);

                // مشغل تحديث توفر الكتب تلقائياً
                const string availabilityTrigger = @"
                    IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Borrowings_UpdateBookAvailability')
                    BEGIN
                        EXEC('
                        CREATE TRIGGER TR_Borrowings_UpdateBookAvailability
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
                        END')
                    END";

                await ExecuteNonQueryAsync(connection, availabilityTrigger);

                _logger.LogDebug("تم إنشاء جميع المشغلات بنجاح - All triggers created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في إنشاء المشغلات - Failed to create triggers");
                throw;
            }
        }

        /// <summary>
        /// إنشاء الفهارس المتقدمة للأداء
        /// Create advanced indexes for performance
        /// </summary>
        private async Task CreateAdvancedIndexesAsync()
        {
            try
            {
                _logger.LogDebug("إنشاء الفهارس المتقدمة - Creating advanced indexes");

                using var connection = await CreateConnectionAsync();

                // فهارس أساسية للكتب
                await CreateIndexIfNotExistsAsync(connection, "IX_Books_Title", "Books", "Title");
                await CreateIndexIfNotExistsAsync(connection, "IX_Books_Author", "Books", "Author");
                await CreateIndexIfNotExistsAsync(connection, "IX_Books_ISBN", "Books", "ISBN");
                await CreateIndexIfNotExistsAsync(connection, "IX_Books_Genre", "Books", "Genre");

                // فهارس مركبة للبحث المتقدم
                const string compositeSearchIndex = @"
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Books_Search_Composite')
                    BEGIN
                        CREATE NONCLUSTERED INDEX IX_Books_Search_Composite
                        ON Books (Genre, PublicationYear, AvailableCopies)
                        INCLUDE (BookId, Title, Author, ISBN, Publisher, TotalCopies, Description);
                    END";

                await ExecuteNonQueryAsync(connection, compositeSearchIndex);

                // فهرس للبحث بالعنوان والمؤلف
                const string titleAuthorIndex = @"
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Books_Title_Author')
                    BEGIN
                        CREATE NONCLUSTERED INDEX IX_Books_Title_Author
                        ON Books (Title, Author)
                        INCLUDE (ISBN, Genre, TotalCopies, AvailableCopies, PublicationYear, Description);
                    END";

                await ExecuteNonQueryAsync(connection, titleAuthorIndex);

                // فهارس للمستخدمين
                await CreateIndexIfNotExistsAsync(connection, "IX_Users_Email", "Users", "Email");
                await CreateIndexIfNotExistsAsync(connection, "IX_Users_IsActive", "Users", "IsActive");

                // فهارس للاستعارات
                await CreateIndexIfNotExistsAsync(connection, "IX_Borrowings_UserId", "Borrowings", "UserId");
                await CreateIndexIfNotExistsAsync(connection, "IX_Borrowings_BookId", "Borrowings", "BookId");
                await CreateIndexIfNotExistsAsync(connection, "IX_Borrowings_IsReturned", "Borrowings", "IsReturned");
                await CreateIndexIfNotExistsAsync(connection, "IX_Borrowings_BorrowDate", "Borrowings", "BorrowDate");
                await CreateIndexIfNotExistsAsync(connection, "IX_Borrowings_DueDate", "Borrowings", "DueDate");

                // فهرس مركب للاستعارات النشطة
                const string activeBorrowingsIndex = @"
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Borrowings_Active')
                    BEGIN
                        CREATE NONCLUSTERED INDEX IX_Borrowings_Active
                        ON Borrowings (IsReturned, UserId, BookId)
                        INCLUDE (BorrowDate, DueDate, LateFee)
                        WHERE IsReturned = 0;
                    END";

                await ExecuteNonQueryAsync(connection, activeBorrowingsIndex);

                _logger.LogInformation("تم إنشاء جميع الفهارس بنجاح - All indexes created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في إنشاء الفهارس - Failed to create indexes");
                throw;
            }
        }

        /// <summary>
        /// إنشاء فهرس إذا لم يكن موجوداً
        /// Create index if it doesn't exist
        /// </summary>
        private async Task CreateIndexIfNotExistsAsync(IDbConnection connection, string indexName, string tableName, string columnName)
        {
            const string checkIndexSql = @"
                SELECT COUNT(*) FROM sys.indexes
                WHERE name = @IndexName AND object_id = OBJECT_ID(@TableName)";

            var indexExists = await ExecuteScalarAsync<int>(connection, checkIndexSql, new { IndexName = indexName, TableName = tableName });

            if (indexExists == 0)
            {
                var createIndexSql = $"CREATE NONCLUSTERED INDEX {indexName} ON {tableName}({columnName})";
                await ExecuteNonQueryAsync(connection, createIndexSql);
                _logger.LogDebug("تم إنشاء الفهرس: {IndexName} - Index created: {IndexName}", indexName, indexName);
            }
        }

        /// <summary>
        /// إدراج البيانات الأولية إذا لم تكن موجودة
        /// Insert initial data if it doesn't exist
        /// </summary>
        private async Task InsertInitialDataIfNotExistsAsync()
        {
            try
            {
                _logger.LogDebug("فحص وإدراج البيانات الأولية - Checking and inserting initial data");

                using var connection = await CreateConnectionAsync();

                // فحص وإدراج المستخدمين الأوليين
                await InsertInitialUsersAsync(connection);

                // فحص وإدراج الكتب الأولية
                await InsertInitialBooksAsync(connection);

                _logger.LogInformation("تم إدراج البيانات الأولية بنجاح - Initial data inserted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في إدراج البيانات الأولية - Failed to insert initial data");
                throw;
            }
        }

        /// <summary>
        /// إدراج المستخدمين الأوليين
        /// Insert initial users
        /// </summary>
        private async Task InsertInitialUsersAsync(IDbConnection connection)
        {
            const string checkUsersSql = "SELECT COUNT(*) FROM Users";
            var userCount = await ExecuteScalarAsync<int>(connection, checkUsersSql);

            if (userCount == 0)
            {
                _logger.LogDebug("إدراج المستخدمين الأوليين - Inserting initial users");

                const string insertUsersSql = @"
                    INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, Address, MembershipDate, IsActive)
                    VALUES
                        ('أحمد', 'محمد', 'ahmed.mohamed@email.com', '966501234567', 'الرياض، المملكة العربية السعودية', GETDATE(), 1),
                        ('فاطمة', 'علي', 'fatima.ali@email.com', '966502345678', 'جدة، المملكة العربية السعودية', GETDATE(), 1),
                        ('محمد', 'السعيد', 'mohamed.alsaeed@email.com', '966503456789', 'الدمام، المملكة العربية السعودية', GETDATE(), 1),
                        ('نورا', 'أحمد', 'nora.ahmed@email.com', '966504567890', 'مكة المكرمة، المملكة العربية السعودية', GETDATE(), 1),
                        ('خالد', 'العتيبي', 'khalid.alotaibi@email.com', '966505678901', 'المدينة المنورة، المملكة العربية السعودية', GETDATE(), 1),
                        ('سارة', 'الزهراني', 'sara.alzahrani@email.com', '966506789012', 'الطائف، المملكة العربية السعودية', GETDATE(), 1),
                        ('عبدالله', 'القحطاني', 'abdullah.alqahtani@email.com', '966507890123', 'أبها، المملكة العربية السعودية', GETDATE(), 1),
                        ('مريم', 'الشهري', 'mariam.alshahri@email.com', '966508901234', 'تبوك، المملكة العربية السعودية', GETDATE(), 1)";

                await ExecuteNonQueryAsync(connection, insertUsersSql);
                _logger.LogDebug("تم إدراج {Count} مستخدم أولي - Inserted {Count} initial users", 8, 8);
            }
        }

        /// <summary>
        /// إدراج الكتب الأولية
        /// Insert initial books
        /// </summary>
        private async Task InsertInitialBooksAsync(IDbConnection connection)
        {
            const string checkBooksSql = "SELECT COUNT(*) FROM Books";
            var bookCount = await ExecuteScalarAsync<int>(connection, checkBooksSql);

            if (bookCount == 0)
            {
                _logger.LogDebug("إدراج الكتب الأولية - Inserting initial books");

                const string insertBooksSql = @"
                    INSERT INTO Books (Title, Author, ISBN, Publisher, PublicationYear, Genre, TotalCopies, AvailableCopies, Description)
                    VALUES
                        ('مئة عام من العزلة', 'غابرييل غارسيا ماركيز', '978-0-06-088328-7', 'دار الآداب', 1967, 'أدب', 3, 3, 'رواية كلاسيكية من أدب أمريكا اللاتينية'),
                        ('الأسود يليق بك', 'أحلام مستغانمي', '978-9953-68-155-2', 'دار الآداب', 2012, 'رواية', 2, 2, 'رواية عربية معاصرة'),
                        ('مدن الملح', 'عبد الرحمن منيف', '978-977-416-023-1', 'المؤسسة العربية للدراسات والنشر', 1984, 'رواية', 4, 4, 'خماسية روائية عن تحولات الخليج العربي'),
                        ('الخبز الحافي', 'محمد شكري', '978-9953-68-089-0', 'دار الساقي', 1973, 'سيرة ذاتية', 2, 2, 'سيرة ذاتية مؤثرة'),
                        ('رجال في الشمس', 'غسان كنفاني', '978-9953-68-012-8', 'دار الطليعة', 1963, 'قصص قصيرة', 3, 3, 'مجموعة قصصية فلسطينية'),
                        ('الطنطورية', 'رضوى عاشور', '978-977-416-456-7', 'دار الشروق', 2010, 'رواية تاريخية', 2, 2, 'رواية تاريخية عن فلسطين'),
                        ('موسم الهجرة إلى الشمال', 'الطيب صالح', '978-977-416-789-6', 'دار العودة', 1966, 'أدب', 3, 3, 'رواية سودانية كلاسيكية'),
                        ('تراب الماس', 'أحمد مراد', '978-977-14-2345-8', 'دار الشروق', 2018, 'إثارة', 5, 5, 'رواية إثارة مصرية معاصرة'),
                        ('أرض زيكولا', 'عمرو عبد الحميد', '978-977-14-3456-9', 'دار نهضة مصر', 2020, 'خيال علمي', 2, 2, 'رواية خيال علمي عربية'),
                        ('في قلبي أنثى عبرية', 'خولة حمدي', '978-9953-68-234-4', 'دار الفارابي', 2015, 'رواية', 3, 3, 'رواية عن الصراع العربي الإسرائيلي'),
                        ('البحث عن وليد مسعود', 'جبرا إبراهيم جبرا', '978-9953-68-345-7', 'المؤسسة العربية للدراسات والنشر', 1978, 'أدب', 2, 2, 'رواية فلسطينية كلاسيكية'),
                        ('ذاكرة الجسد', 'أحلام مستغانمي', '978-9953-68-456-8', 'دار الآداب', 1993, 'رواية', 2, 2, 'الجزء الأول من ثلاثية الذاكرة'),
                        ('الحرافيش', 'نجيب محفوظ', '978-977-416-567-9', 'مكتبة مصر', 1977, 'أدب', 3, 3, 'ملحمة شعبية مصرية'),
                        ('أولاد حارتنا', 'نجيب محفوظ', '978-977-416-678-0', 'دار الشروق', 1959, 'أدب', 2, 2, 'رواية رمزية مثيرة للجدل'),
                        ('الزمن الموحش', 'حيدر حيدر', '978-9953-68-789-1', 'دار الآداب', 1973, 'رواية', 2, 2, 'رواية سورية معاصرة'),
                        ('الوسمية', 'محمد العلي', '978-9953-68-890-2', 'دار الحوار', 2019, 'رواية', 2, 2, 'رواية سعودية معاصرة'),
                        ('بنات الرياض', 'رجاء الصانع', '978-9953-68-901-3', 'دار الساقي', 2005, 'رواية', 2, 2, 'رواية سعودية نسائية'),
                        ('الحي اللاتيني', 'سهيل إدريس', '978-9953-68-012-4', 'دار الآداب', 1954, 'رواية', 3, 3, 'رواية لبنانية كلاسيكية'),
                        ('الخروج من التيه', 'عبد الله ثابت', '978-9953-68-123-5', 'دار الفكر', 2021, 'فكر', 2, 2, 'كتاب فكري معاصر'),
                        ('تاريخ الطبري', 'محمد بن جرير الطبري', '978-977-416-234-6', 'دار المعارف', 1960, 'تاريخ', 2, 2, 'كتاب تاريخي إسلامي كلاسيكي')";

                await ExecuteNonQueryAsync(connection, insertBooksSql);
                _logger.LogDebug("تم إدراج {Count} كتاب أولي - Inserted {Count} initial books", 20, 20);
            }
        }
    }
}
