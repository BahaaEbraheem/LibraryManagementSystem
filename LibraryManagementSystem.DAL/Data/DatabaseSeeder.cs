using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.DAL.Data
{
    /// <summary>
    /// كلاس لإدراج البيانات الأولية في قاعدة البيانات
    /// Class for seeding initial data into the database
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(string connectionString, ILogger<DatabaseSeeder>? logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? NullLogger<DatabaseSeeder>.Instance;

        }

        /// <summary>
        /// إدراج جميع البيانات الأولية
        /// Seed all initial data
        /// </summary>
        public async Task SeedAllDataAsync()
        {
            try
            {
                _logger.LogInformation("بدء إدراج البيانات الأولية - Starting data seeding");

                await SeedUsersAsync();
                await SeedBooksAsync();

                _logger.LogInformation("تم إكمال إدراج البيانات الأولية بنجاح - Data seeding completed successfully");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// إدراج المستخدمين الأوليين
        /// Seed initial users
        /// </summary>
        private async Task SeedUsersAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // التحقق من وجود مستخدمين
                // Check if users exist
                var checkUsersQuery = "SELECT COUNT(*) FROM Users";
                using var checkCommand = new SqlCommand(checkUsersQuery, connection);
                var userCount = (int)await checkCommand.ExecuteScalarAsync();

                if (userCount > 0)
                {
                    _logger.LogInformation("المستخدمون موجودون مسبقاً، تخطي إدراج المستخدمين - Users already exist, skipping user seeding");
                    return;
                }

                _logger.LogInformation("إدراج المستخدمين الأوليين - Seeding initial users");

                // إدراج المدير
                // Insert administrator
                var adminPasswordHash = HashPassword("admin123");
                var insertAdminQuery = @"
                    INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, Address, MembershipDate, IsActive, PasswordHash, Role, CreatedDate, ModifiedDate)
                    VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @Address, @MembershipDate, @IsActive, @PasswordHash, @Role, @CreatedDate, @ModifiedDate)";

                using var adminCommand = new SqlCommand(insertAdminQuery, connection);
                adminCommand.Parameters.AddWithValue("@FirstName", "مدير");
                adminCommand.Parameters.AddWithValue("@LastName", "النظام");
                adminCommand.Parameters.AddWithValue("@Email", "admin@library.com");
                adminCommand.Parameters.AddWithValue("@PhoneNumber", "555-0001");
                adminCommand.Parameters.AddWithValue("@Address", "مكتب الإدارة");
                adminCommand.Parameters.AddWithValue("@MembershipDate", DateTime.Now);
                adminCommand.Parameters.AddWithValue("@IsActive", true);
                adminCommand.Parameters.AddWithValue("@PasswordHash", adminPasswordHash);
                adminCommand.Parameters.AddWithValue("@Role", (int)UserRole.Administrator);
                adminCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                adminCommand.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                await adminCommand.ExecuteNonQueryAsync();

                // إدراج مستخدمين عاديين
                // Insert regular users
                var userPasswordHash = HashPassword("user123");
                var users = new[]
                {
                    new { FirstName = "أحمد", LastName = "محمد", Email = "ahmed.mohamed@email.com", Phone = "555-0101", Address = "123 شارع الرئيسي، المدينة" },
                    new { FirstName = "فاطمة", LastName = "علي", Email = "fatima.ali@email.com", Phone = "555-0102", Address = "456 شارع البلوط، المدينة" },
                    new { FirstName = "محمد", LastName = "أحمد", Email = "mohamed.ahmed@email.com", Phone = "555-0103", Address = "789 شارع الصنوبر، المدينة" },
                    new { FirstName = "عائشة", LastName = "حسن", Email = "aisha.hassan@email.com", Phone = "555-0104", Address = "321 شارع الدردار، المدينة" },
                    new { FirstName = "عبدالله", LastName = "سالم", Email = "abdullah.salem@email.com", Phone = "555-0105", Address = "654 شارع القيقب، المدينة" }
                };

                var insertUserQuery = @"
                    INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, Address, MembershipDate, IsActive, PasswordHash, Role, CreatedDate, ModifiedDate)
                    VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @Address, @MembershipDate, @IsActive, @PasswordHash, @Role, @CreatedDate, @ModifiedDate)";

                foreach (var user in users)
                {
                    using var userCommand = new SqlCommand(insertUserQuery, connection);
                    userCommand.Parameters.AddWithValue("@FirstName", user.FirstName);
                    userCommand.Parameters.AddWithValue("@LastName", user.LastName);
                    userCommand.Parameters.AddWithValue("@Email", user.Email);
                    userCommand.Parameters.AddWithValue("@PhoneNumber", user.Phone);
                    userCommand.Parameters.AddWithValue("@Address", user.Address);
                    userCommand.Parameters.AddWithValue("@MembershipDate", DateTime.Now.AddDays(-Random.Shared.Next(30, 365)));
                    userCommand.Parameters.AddWithValue("@IsActive", true);
                    userCommand.Parameters.AddWithValue("@PasswordHash", userPasswordHash);
                    userCommand.Parameters.AddWithValue("@Role", (int)UserRole.User);
                    userCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    userCommand.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                    await userCommand.ExecuteNonQueryAsync();
                }

                _logger.LogInformation("تم إدراج المستخدمين بنجاح - Users seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إدراج المستخدمين - Error seeding users");
                throw;
            }
        }

        /// <summary>
        /// إدراج الكتب الأولية
        /// Seed initial books
        /// </summary>
        private async Task SeedBooksAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // التحقق من وجود كتب
                // Check if books exist
                var checkBooksQuery = "SELECT COUNT(*) FROM Books";
                using var checkCommand = new SqlCommand(checkBooksQuery, connection);
                var bookCount = (int)await checkCommand.ExecuteScalarAsync();

                if (bookCount > 0)
                {
                    _logger.LogInformation("الكتب موجودة مسبقاً، تخطي إدراج الكتب - Books already exist, skipping book seeding");
                    return;
                }

                _logger.LogInformation("إدراج الكتب الأولية - Seeding initial books");

                var books = new[]
                {
                    new { Title = "Clean Code", Author = "Robert C. Martin", ISBN = "978-0-13-235088-4", Publisher = "Prentice Hall", Year = 2008, Genre = "Programming", Copies = 3, Description = "A Handbook of Agile Software Craftsmanship" },
                    new { Title = "The Pragmatic Programmer", Author = "David Thomas", ISBN = "978-0-20-161622-4", Publisher = "Addison-Wesley", Year = 1999, Genre = "Programming", Copies = 2, Description = "From Journeyman to Master" },
                    new { Title = "Design Patterns", Author = "Gang of Four", ISBN = "978-0-20-163361-0", Publisher = "Addison-Wesley", Year = 1994, Genre = "Programming", Copies = 2, Description = "Elements of Reusable Object-Oriented Software" },
                    new { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "978-0-74-327356-5", Publisher = "Scribner", Year = 1925, Genre = "Fiction", Copies = 3, Description = "A classic American novel set in the Jazz Age" },
                    new { Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "978-0-06-112008-4", Publisher = "J.B. Lippincott & Co.", Year = 1960, Genre = "Fiction", Copies = 2, Description = "A gripping tale of racial injustice and childhood innocence" },
                    new { Title = "1984", Author = "George Orwell", ISBN = "978-0-45-228423-4", Publisher = "Secker & Warburg", Year = 1949, Genre = "Dystopian Fiction", Copies = 4, Description = "A dystopian social science fiction novel" },
                    new { Title = "Pride and Prejudice", Author = "Jane Austen", ISBN = "978-0-14-143951-8", Publisher = "T. Egerton", Year = 1813, Genre = "Romance", Copies = 2, Description = "A romantic novel of manners" },
                    new { Title = "The Hobbit", Author = "J.R.R. Tolkien", ISBN = "978-0-54-792822-7", Publisher = "George Allen & Unwin", Year = 1937, Genre = "Fantasy", Copies = 3, Description = "A fantasy novel about the adventures of Bilbo Baggins" },
                    new { Title = "Harry Potter and the Philosopher's Stone", Author = "J.K. Rowling", ISBN = "978-0-74-753269-9", Publisher = "Bloomsbury", Year = 1997, Genre = "Fantasy", Copies = 5, Description = "The first book in the Harry Potter series" },
                    new { Title = "The Alchemist", Author = "Paulo Coelho", ISBN = "978-0-06-231500-7", Publisher = "HarperOne", Year = 1988, Genre = "Fiction", Copies = 3, Description = "A philosophical novel about following your dreams" }
                };

                var insertBookQuery = @"
                    INSERT INTO Books (Title, Author, ISBN, Publisher, PublicationYear, Genre, TotalCopies, AvailableCopies, Description, CreatedDate, ModifiedDate)
                    VALUES (@Title, @Author, @ISBN, @Publisher, @PublicationYear, @Genre, @TotalCopies, @AvailableCopies, @Description, @CreatedDate, @ModifiedDate)";

                foreach (var book in books)
                {
                    using var bookCommand = new SqlCommand(insertBookQuery, connection);
                    bookCommand.Parameters.AddWithValue("@Title", book.Title);
                    bookCommand.Parameters.AddWithValue("@Author", book.Author);
                    bookCommand.Parameters.AddWithValue("@ISBN", book.ISBN);
                    bookCommand.Parameters.AddWithValue("@Publisher", book.Publisher);
                    bookCommand.Parameters.AddWithValue("@PublicationYear", book.Year);
                    bookCommand.Parameters.AddWithValue("@Genre", book.Genre);
                    bookCommand.Parameters.AddWithValue("@TotalCopies", book.Copies);
                    bookCommand.Parameters.AddWithValue("@AvailableCopies", book.Copies);
                    bookCommand.Parameters.AddWithValue("@Description", book.Description);
                    bookCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    bookCommand.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                    await bookCommand.ExecuteNonQueryAsync();
                }

                _logger.LogInformation("تم إدراج الكتب بنجاح - Books seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إدراج الكتب - Error seeding books");
                throw;
            }
        }

        /// <summary>
        /// تشفير كلمة المرور
        /// Hash password
        /// </summary>
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "LibraryManagementSalt"));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
