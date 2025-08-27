using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
                    new { Title = "الحرب والسلم", Author = "ليو تولستوي", ISBN = "978-0-14-044793-4", Publisher = "دار النشر", Year = 1869, Genre = "رواية", Copies = 3, Description = "رواية كلاسيكية روسية" },
                    new { Title = "الأمير", Author = "نيكولو مكيافيلي", ISBN = "978-0-19-953569-9", Publisher = "دار النشر", Year = 1532, Genre = "فكر", Copies = 2, Description = "كتاب سياسي وفلسفي" },
                    new { Title = "مئة عام من العزلة", Author = "غابرييل غارثيا ماركيث", ISBN = "978-0-06-088328-7", Publisher = "دار النشر", Year = 1967, Genre = "رواية", Copies = 4, Description = "رواية خيالية لاتينية" },
                    new { Title = "رحلة ابن بطوطة", Author = "ابن بطوطة", ISBN = "978-977-09-0467-6", Publisher = "دار النشر", Year = 1355, Genre = "تاريخ", Copies = 2, Description = "سيرة ذاتية ورحلات تاريخية" },
                    new { Title = "قواعد التفكير", Author = "أحمد أمين", ISBN = "978-977-09-0123-2", Publisher = "دار النشر", Year = 1950, Genre = "فكر", Copies = 3, Description = "كتاب في الفلسفة والمنطق" },
                    new { Title = "قصص للأطفال", Author = "جبران خليل جبران", ISBN = "978-977-09-0543-1", Publisher = "دار النشر", Year = 1920, Genre = "قصص قصيرة", Copies = 5, Description = "مجموعة قصص قصيرة للأطفال" },
                    new { Title = "إثارة الغموض", Author = "أجاثا كريستي", ISBN = "978-0-06-207348-8", Publisher = "دار النشر", Year = 1934, Genre = "إثارة", Copies = 2, Description = "رواية إثارة وغموض" },
                    new { Title = "خيال علمي المستقبل", Author = "إسحاق أسيموف", ISBN = "978-0-553-80370-9", Publisher = "دار النشر", Year = 1951, Genre = "خيال علمي", Copies = 3, Description = "رواية خيال علمي" },
                    new { Title = "قصص رومانسية", Author = "جين أوستن", ISBN = "978-0-14-143951-8", Publisher = "دار النشر", Year = 1813, Genre = "أدب", Copies = 2, Description = "رواية رومانسية" },
                    new { Title = "سيرة حياة شخصية مشهورة", Author = "جون سميث", ISBN = "978-0-06-231500-7", Publisher = "دار النشر", Year = 1988, Genre = "سيرة ذاتية", Copies = 3, Description = "سيرة شخصية" }
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
