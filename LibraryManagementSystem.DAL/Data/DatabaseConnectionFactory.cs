using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace LibraryManagementSystem.DAL.Data
{
    /// <summary>
    /// مصنع لإنشاء اتصالات قاعدة البيانات
    /// Factory for creating database connections
    /// </summary>
    public interface IDatabaseConnectionFactory
    {
        /// <summary>
        /// إنشاء اتصال جديد بقاعدة البيانات
        /// Create a new database connection
        /// </summary>
        IDbConnection CreateConnection();

        /// <summary>
        /// إنشاء اتصال جديد بقاعدة البيانات بشكل غير متزامن
        /// Create a new database connection asynchronously
        /// </summary>
        Task<IDbConnection> CreateConnectionAsync();

        /// <summary>
        /// تهيئة قاعدة البيانات
        /// Initialize database
        /// </summary>
        Task InitializeDatabaseAsync();
    }

    /// <summary>
    /// تنفيذ مصنع اتصالات قاعدة البيانات باستخدام SQL Server
    /// Implementation of database connection factory using SQL Server
    /// </summary>
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseConnectionFactory>? _logger;

        /// <summary>
        /// منشئ الفئة مع سلسلة الاتصال
        /// Constructor with connection string
        /// </summary>
        /// <param name="connectionString">سلسلة الاتصال بقاعدة البيانات</param>
        public DatabaseConnectionFactory(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// منشئ الفئة مع سلسلة الاتصال والـ Logger
        /// Constructor with connection string and logger
        /// </summary>
        /// <param name="connectionString">سلسلة الاتصال بقاعدة البيانات</param>
        /// <param name="logger">Logger للتسجيل</param>
        public DatabaseConnectionFactory(string connectionString, ILogger<DatabaseConnectionFactory> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger;
        }

        /// <summary>
        /// إنشاء اتصال جديد وفتحه
        /// Create and open a new connection
        /// </summary>
        public IDbConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// إنشاء اتصال جديد وفتحه بشكل غير متزامن
        /// Create and open a new connection asynchronously
        /// </summary>
        public async Task<IDbConnection> CreateConnectionAsync()
        {
            // دائماً تحقق من وجود قاعدة البيانات أولاً
            Console.WriteLine("Checking database existence...");
            await EnsureDatabaseExistsAsync();

            Console.WriteLine("Connecting to database...");
            var connection = new SqlConnection(_connectionString);

            try
            {
                await connection.OpenAsync();
                Console.WriteLine("Connected successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect: {ex.Message}");
                throw;
            }

            Console.WriteLine("Ensuring tables exist...");
            await EnsureTablesExistAsync(connection);

            Console.WriteLine("Database setup complete!");
            return connection;
        }


        private async Task EnsureDatabaseExistsAsync()
        {
            var databaseName = GetDatabaseName();
            var masterConnectionString = GetMasterConnectionString();

            using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            var checkSql = "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName";
            using var checkCmd = new SqlCommand(checkSql, connection);
            checkCmd.Parameters.AddWithValue("@DatabaseName", databaseName);

            var exists = (int)await checkCmd.ExecuteScalarAsync();

            if (exists == 0)
            {
                Console.WriteLine($"Database '{databaseName}' does not exist. Creating...");
                using var createCmd = new SqlCommand($"CREATE DATABASE [{databaseName}]", connection);
                await createCmd.ExecuteNonQueryAsync();
                Console.WriteLine("Database created successfully!");
            }
            else
            {
                Console.WriteLine("Database already exists.");
            }
        }

        private async Task EnsureTablesExistAsync(IDbConnection connection)
        {
            try
            {
                Console.WriteLine("Creating tables if they don't exist...");

                // Create Users table
                Console.WriteLine("Creating Users table...");
                await ExecuteSqlAsync(connection, @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
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
                            Role INT NOT NULL DEFAULT 1,
                            CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
                            ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

                            CONSTRAINT CHK_Users_Role CHECK (Role IN (1, 2))
                        );
                    END
                    ELSE
                    BEGIN
                        -- Add columns if they don't exist (for existing tables)
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PasswordHash')
                        BEGIN
                            ALTER TABLE Users ADD PasswordHash NVARCHAR(255) NOT NULL DEFAULT '';
                        END

                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Role')
                        BEGIN
                            ALTER TABLE Users ADD Role INT NOT NULL DEFAULT 1;
                            ALTER TABLE Users ADD CONSTRAINT CHK_Users_Role CHECK (Role IN (1, 2));
                        END
                    END");

                // Create Books table
                await ExecuteSqlAsync(connection, @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Books')
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
                            ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE()
                        );
                    END");

                // Create Borrowings table
                await ExecuteSqlAsync(connection, @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Borrowings')
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
                            CONSTRAINT FK_Borrowings_Books FOREIGN KEY (BookId) REFERENCES Books(BookId)
                        );
                    END");

                // Create basic indexes
                await ExecuteSqlAsync(connection, @"
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Books_Title')
                        CREATE NONCLUSTERED INDEX IX_Books_Title ON Books(Title);

                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
                        CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);");

                //// Insert initial data if tables are empty
                //await InsertInitialDataIfNeededAsync(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create tables: {ex.Message}");
                throw;
            }
        }

        //private async Task InsertInitialDataIfNeededAsync(IDbConnection connection)
        //{
        //    try
        //    {
        //        // Check if data already exists
        //        var userCountSql = "SELECT COUNT(*) FROM Users";
        //        using var userCountCmd = new SqlCommand(userCountSql, (SqlConnection)connection);
        //        var userCount = (int)await userCountCmd.ExecuteScalarAsync();

        //        if (userCount == 0)
        //        {
        //            // Insert initial users
        //            await ExecuteSqlAsync(connection, @"
        //                INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, Address, IsActive)
        //                VALUES
        //                    ('Ahmed', 'Mohamed', 'ahmed.mohamed@email.com', '966501234567', 'Riyadh', 1),
        //                    ('Fatima', 'Ali', 'fatima.ali@email.com', '966502345678', 'Jeddah', 1),
        //                    ('Mohamed', 'Alsaeed', 'mohamed.alsaeed@email.com', '966503456789', 'Dammam', 1)");

        //            // Insert initial books
        //            await ExecuteSqlAsync(connection, @"
        //                INSERT INTO Books (Title, Author, ISBN, Publisher, PublicationYear, Genre, TotalCopies, AvailableCopies, Description)
        //                VALUES
        //                    ('Clean Code', 'Robert C. Martin', '978-0132350884', 'Prentice Hall', 2008, 'Programming', 3, 3, 'A handbook of agile software craftsmanship'),
        //                    ('Design Patterns', 'Gang of Four', '978-0201633612', 'Addison-Wesley', 1994, 'Programming', 2, 2, 'Elements of reusable object-oriented software'),
        //                    ('The Pragmatic Programmer', 'Andrew Hunt', '978-0201616224', 'Addison-Wesley', 1999, 'Programming', 2, 2, 'From journeyman to master')");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Failed to insert initial data: {ex.Message}");
        //        // Don't throw here, let the app continue
        //    }
        //}

        private async Task ExecuteSqlAsync(IDbConnection connection, string sql)
        {
            using var cmd = new SqlCommand(sql, (SqlConnection)connection);
            await cmd.ExecuteNonQueryAsync();
        }

        private string GetDatabaseName()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            return builder.InitialCatalog;
        }

        private string GetMasterConnectionString()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            builder.InitialCatalog = "master";
            // إضافة معاملات لتجنب مشاكل trigger
            builder.ApplicationName = "LibraryManagementSystem";
            builder.ConnectTimeout = 30;
            builder.CommandTimeout = 30;
            return builder.ConnectionString;
        }

        /// <summary>
        /// تهيئة قاعدة البيانات
        /// Initialize database
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            try
            {
                _logger?.LogInformation("بدء تهيئة قاعدة البيانات - Starting database initialization");

                await EnsureDatabaseExistsAsync();
                using var connection = await CreateConnectionAsync();
                await EnsureTablesExistAsync(connection);

                // إدراج البيانات الأولية
                // Seed initial data
                var seeder = new DatabaseSeeder(_connectionString);
                await seeder.SeedAllDataAsync();

                await SeedInitialDataAsync();

                _logger?.LogInformation("تم إكمال تهيئة قاعدة البيانات بنجاح - Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "خطأ في تهيئة قاعدة البيانات - Error during database initialization");
                throw;
            }
        }

        /// <summary>
        /// إدراج البيانات الأولية
        /// Seed initial data
        /// </summary>
        private async Task SeedInitialDataAsync()
        {
            try
            {
                // التحقق من وجود مستخدمين
                // Check if users exist
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
                await connection.OpenAsync();

                var checkUsersQuery = "SELECT COUNT(*) FROM Users";
                using var checkCommand = new Microsoft.Data.SqlClient.SqlCommand(checkUsersQuery, connection);
                var userCount = (int)await checkCommand.ExecuteScalarAsync();

                if (userCount > 0)
                {
                    _logger?.LogInformation("البيانات الأولية موجودة مسبقاً - Initial data already exists");
                    return;
                }

                _logger?.LogInformation("إدراج البيانات الأولية - Seeding initial data");

                // إدراج المدير
                // Insert administrator
                var adminPasswordHash = HashPassword("admin123");
                var insertAdminQuery = @"
                    INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, Address, MembershipDate, IsActive, PasswordHash, Role, CreatedDate, ModifiedDate)
                    VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @Address, @MembershipDate, @IsActive, @PasswordHash, @Role, @CreatedDate, @ModifiedDate)";

                using var adminCommand = new Microsoft.Data.SqlClient.SqlCommand(insertAdminQuery, connection);
                adminCommand.Parameters.AddWithValue("@FirstName", "Admin");
                adminCommand.Parameters.AddWithValue("@LastName", "User");
                adminCommand.Parameters.AddWithValue("@Email", "admin@library.com");
                adminCommand.Parameters.AddWithValue("@PhoneNumber", "555-0001");
                adminCommand.Parameters.AddWithValue("@Address", "Admin Office");
                adminCommand.Parameters.AddWithValue("@MembershipDate", DateTime.Now);
                adminCommand.Parameters.AddWithValue("@IsActive", true);
                adminCommand.Parameters.AddWithValue("@PasswordHash", adminPasswordHash);
                adminCommand.Parameters.AddWithValue("@Role", 2); // Administrator
                adminCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                adminCommand.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                await adminCommand.ExecuteNonQueryAsync();

                _logger?.LogInformation("تم إدراج المدير بنجاح - Admin user seeded successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "خطأ في إدراج البيانات الأولية - Error seeding initial data");
                throw;
            }
        }

        /// <summary>
        /// تشفير كلمة المرور
        /// Hash password
        /// </summary>
        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "LibraryManagementSalt"));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    /// <summary>
    /// فئة مساعدة لقاعدة البيانات للعمليات الشائعة
    /// Database helper class for common operations
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// تنفيذ استعلام scalar وإرجاع النتيجة
        /// Executes a scalar query and returns the result
        /// </summary>
        /// <typeparam name="T">نوع البيانات المطلوب إرجاعه</typeparam>
        /// <param name="connection">اتصال قاعدة البيانات</param>
        /// <param name="sql">استعلام SQL</param>
        /// <param name="parameters">معاملات الاستعلام</param>
        public static async Task<T?> ExecuteScalarAsync<T>(IDbConnection connection, string sql, object? parameters = null)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            var result = await ((SqlCommand)command).ExecuteScalarAsync();
            return result == null || result == DBNull.Value
                ? default(T)
                : (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// تنفيذ أمر غير استعلامي وإرجاع عدد الصفوف المتأثرة
        /// Executes a non-query command and returns the number of affected rows
        /// </summary>
        /// <param name="connection">اتصال قاعدة البيانات</param>
        /// <param name="sql">أمر SQL</param>
        /// <param name="parameters">معاملات الأمر</param>
        public static async Task<int> ExecuteNonQueryAsync(IDbConnection connection, string sql, object? parameters = null)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            return await ((SqlCommand)command).ExecuteNonQueryAsync();
        }

        /// <summary>
        /// تنفيذ استعلام وإرجاع قارئ البيانات
        /// Executes a query and returns a data reader
        /// </summary>
        /// <param name="connection">اتصال قاعدة البيانات</param>
        /// <param name="sql">استعلام SQL</param>
        /// <param name="parameters">معاملات الاستعلام</param>
        public static async Task<IDataReader> ExecuteReaderAsync(IDbConnection connection, string sql, object? parameters = null)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            return await ((SqlCommand)command).ExecuteReaderAsync();
        }

        /// <summary>
        /// إضافة معاملات إلى الأمر من كائن مجهول
        /// Adds parameters to a command from an anonymous object
        /// </summary>
        /// <param name="command">أمر قاعدة البيانات</param>
        /// <param name="parameters">كائن المعاملات</param>
        private static void AddParameters(IDbCommand command, object parameters)
        {
            if (parameters == null) return;

            var properties = parameters.GetType().GetProperties();
            foreach (var property in properties)
            {
                // تجاهل الخصائص المعقدة أو التي تحتاج indexer
                if (property.GetIndexParameters().Length > 0)
                    continue;

                var value = property.GetValue(parameters);

                // السماح فقط بالأنواع المدعومة
                if (value == null ||
                    value is string ||
                    value is int || value is long || value is short || value is byte ||
                    value is bool || value is bool? ||
                    value is decimal || value is double || value is float ||
                    value is DateTime || value is DateTime?)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{property.Name}";
                    parameter.Value = value ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
                else
                {
                    // يمكن وضع log هنا لتعرف الخصائص التي تم تجاهلها
                    Console.WriteLine($"تجاهل property غير مدعوم: {property.Name} ({property.PropertyType.Name})");
                }
            }
        }



        /// <summary>
        /// تحويل قارئ البيانات إلى كائن باستخدام الانعكاس
        /// Maps a data reader to an object using reflection
        /// </summary>
        /// <typeparam name="T">نوع الكائن المطلوب</typeparam>
        /// <param name="reader">قارئ البيانات</param>
        public static T MapToObject<T>(IDataReader reader) where T : new()
        {
            var obj = new T();
            var properties = typeof(T).GetProperties();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var property = properties.FirstOrDefault(p =>
                    string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

                if (property != null && !reader.IsDBNull(i))
                {
                    var value = reader.GetValue(i);
                    if (value != DBNull.Value)
                    {
                        // التعامل مع الأنواع القابلة للإلغاء - Handle nullable types
                        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        var convertedValue = Convert.ChangeType(value, targetType);
                        property.SetValue(obj, convertedValue);
                    }
                }
            }

            return obj;
        }
    }
}
