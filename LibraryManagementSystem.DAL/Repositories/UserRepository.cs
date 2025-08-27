using LibraryManagementSystem.DAL.Caching;
using LibraryManagementSystem.DAL.Data;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.Extensions.Logging;
using System.Data;

namespace LibraryManagementSystem.DAL.Repositories
{
    /// <summary>
    /// تنفيذ مستودع المستخدمين
    /// User repository implementation
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UserRepository> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public UserRepository(
            IDatabaseConnectionFactory connectionFactory,
            ICacheService cacheService,
            ILogger<UserRepository> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على جميع المستخدمين
        /// Get all users
        /// </summary>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedUsers = await _cacheService.GetAsync<List<User>>(CacheKeys.Users.All);
                if (cachedUsers != null)
                {
                    _logger.LogDebug("تم الحصول على جميع المستخدمين من التخزين المؤقت - Retrieved all users from cache");
                    return cachedUsers;
                }

                // إذا لم توجد في التخزين المؤقت، احصل عليها من قاعدة البيانات
                // If not in cache, get from database
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT UserId, FirstName, LastName, Email, PhoneNumber, Address,
                           MembershipDate, IsActive, CreatedDate, ModifiedDate, PasswordHash, Role
                    FROM Users
                    ORDER BY FirstName, LastName";

                var users = new List<User>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql);

                while (reader.Read())
                {
                    users.Add(MapReaderToUser(reader));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(CacheKeys.Users.All, users, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} مستخدم من قاعدة البيانات - Retrieved {Count} users from database", users.Count, users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على جميع المستخدمين - Error getting all users");
                throw;
            }
        }

        /// <summary>
        /// الحصول على مستخدم بالمعرف
        /// Get user by ID
        /// </summary>
        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                var cacheKey = CacheKeys.Users.ById(id);

                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
                if (cachedUser != null)
                {
                    _logger.LogDebug("تم الحصول على المستخدم {UserId} من التخزين المؤقت - Retrieved user {UserId} from cache", id, id);
                    return cachedUser;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT UserId, FirstName, LastName, Email, PhoneNumber, Address,
                           MembershipDate, IsActive, CreatedDate, ModifiedDate, PasswordHash, Role
                    FROM Users
                    WHERE UserId = @UserId";

                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { UserId = id });

                if (reader.Read())
                {
                    var user = MapReaderToUser(reader);

                    // تخزين النتيجة في التخزين المؤقت
                    // Store result in cache
                    await _cacheService.SetAsync(cacheKey, user, _cacheExpiration);

                    _logger.LogDebug("تم الحصول على المستخدم {UserId} من قاعدة البيانات - Retrieved user {UserId} from database", id, id);
                    return user;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدم بالمعرف {UserId} - Error getting user by ID {UserId}", id, id);
                throw;
            }
        }

        /// <summary>
        /// الحصول على مستخدم بالبريد الإلكتروني
        /// Get user by email
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                var cacheKey = CacheKeys.Users.ByEmail(email);

                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
                if (cachedUser != null)
                {
                    _logger.LogDebug("تم الحصول على المستخدم بالبريد الإلكتروني من التخزين المؤقت - Retrieved user by email from cache");
                    return cachedUser;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT UserId, FirstName, LastName, Email, PhoneNumber, Address,
                           MembershipDate, IsActive, CreatedDate, ModifiedDate, PasswordHash, Role
                    FROM Users
                    WHERE Email = @Email";

                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { Email = email });

                if (reader.Read())
                {
                    var user = MapReaderToUser(reader);

                    // تخزين النتيجة في التخزين المؤقت
                    // Store result in cache
                    await _cacheService.SetAsync(cacheKey, user, _cacheExpiration);

                    _logger.LogDebug("تم الحصول على المستخدم بالبريد الإلكتروني من قاعدة البيانات - Retrieved user by email from database");
                    return user;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدم بالبريد الإلكتروني {Email} - Error getting user by email {Email}", email, email);
                throw;
            }
        }

        /// <summary>
        /// الحصول على المستخدمين النشطين
        /// Get active users
        /// </summary>
        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            try
            {
                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedUsers = await _cacheService.GetAsync<List<User>>(CacheKeys.Users.Active);
                if (cachedUsers != null)
                {
                    _logger.LogDebug("تم الحصول على المستخدمين النشطين من التخزين المؤقت - Retrieved active users from cache");
                    return cachedUsers;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT UserId, FirstName, LastName, Email, PhoneNumber, Address,
                           MembershipDate, IsActive, CreatedDate, ModifiedDate, PasswordHash, Role
                    FROM Users
                    WHERE IsActive = 1
                    ORDER BY FirstName, LastName";

                var users = new List<User>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql);

                while (reader.Read())
                {
                    users.Add(MapReaderToUser(reader));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(CacheKeys.Users.Active, users, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} مستخدم نشط - Retrieved {Count} active users", users.Count, users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدمين النشطين - Error getting active users");
                throw;
            }
        }

        /// <summary>
        /// تحويل قارئ البيانات إلى كائن مستخدم
        /// Map data reader to user object
        /// </summary>
        private static User MapReaderToUser(IDataReader reader)
        {
            // الحصول على مؤشرات الأعمدة مرة واحدة
            int userIdIndex = reader.GetOrdinal("UserId");
            int firstNameIndex = reader.GetOrdinal("FirstName");
            int lastNameIndex = reader.GetOrdinal("LastName");
            int emailIndex = reader.GetOrdinal("Email");
            int phoneNumberIndex = reader.GetOrdinal("PhoneNumber");
            int addressIndex = reader.GetOrdinal("Address");
            int membershipDateIndex = reader.GetOrdinal("MembershipDate");
            int isActiveIndex = reader.GetOrdinal("IsActive");
            int createdDateIndex = reader.GetOrdinal("CreatedDate");
            int modifiedDateIndex = reader.GetOrdinal("ModifiedDate");
            int passwordHashIndex = reader.GetOrdinal("PasswordHash");
            int roleIndex = reader.GetOrdinal("Role");

            return new User
            {
                UserId = reader.GetInt32(userIdIndex),
                FirstName = reader.GetString(firstNameIndex),
                LastName = reader.GetString(lastNameIndex),
                Email = reader.GetString(emailIndex),
                PhoneNumber = reader.IsDBNull(phoneNumberIndex) ? null : reader.GetString(phoneNumberIndex),
                Address = reader.IsDBNull(addressIndex) ? null : reader.GetString(addressIndex),
                MembershipDate = reader.GetDateTime(membershipDateIndex),
                IsActive = reader.GetBoolean(isActiveIndex),
                CreatedDate = reader.GetDateTime(createdDateIndex),
                ModifiedDate = reader.GetDateTime(modifiedDateIndex),
                PasswordHash = reader.IsDBNull(passwordHashIndex) ? string.Empty : reader.GetString(passwordHashIndex),
                Role = reader.IsDBNull(roleIndex)
    ? UserRole.User
    : (UserRole)reader.GetInt32(roleIndex)
            };
        }


        /// <summary>
        /// البحث عن المستخدمين
        /// Search users
        /// </summary>
        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllAsync();
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT UserId, FirstName, LastName, Email, PhoneNumber, Address,
                           MembershipDate, IsActive, CreatedDate, ModifiedDate, PasswordHash, Role
                    FROM Users
                    WHERE FirstName LIKE @SearchTerm
                       OR LastName LIKE @SearchTerm
                       OR Email LIKE @SearchTerm
                       OR PhoneNumber LIKE @SearchTerm
                    ORDER BY FirstName, LastName";

                var searchPattern = $"%{searchTerm}%";
                var users = new List<User>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { SearchTerm = searchPattern });

                while (reader.Read())
                {
                    users.Add(MapReaderToUser(reader));
                }

                _logger.LogDebug("تم العثور على {Count} مستخدم بالبحث عن '{SearchTerm}' - Found {Count} users searching for '{SearchTerm}'", users.Count, searchTerm, users.Count, searchTerm);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في البحث عن المستخدمين بالمصطلح '{SearchTerm}' - Error searching users with term '{SearchTerm}'", searchTerm, searchTerm);
                throw;
            }
        }

        /// <summary>
        /// إضافة مستخدم جديد
        /// Add a new user
        /// </summary>
        public async Task<int> AddAsync(User user)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    INSERT INTO Users (FirstName, LastName, Email, PhoneNumber, Address, MembershipDate, IsActive, PasswordHash, Role, CreatedDate, ModifiedDate)
                    VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @Address, @MembershipDate, @IsActive, @PasswordHash, @Role, @CreatedDate, @ModifiedDate);
                    SELECT SCOPE_IDENTITY();";

                var userId = await DatabaseHelper.ExecuteScalarAsync<int>(connection, sql, new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.Address,
                    user.MembershipDate,
                    user.IsActive,
                    user.PasswordHash,
                    Role = (int)user.Role, // ✅ تم تعديل هذا السطر
                    user.CreatedDate,
                    user.ModifiedDate
                });

                await InvalidateUserCacheAsync(userId, user.Email);
                _logger.LogDebug("تم إضافة مستخدم جديد بالمعرف {UserId} - Added new user with ID {UserId}", userId, userId);

                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة مستخدم جديد - Error adding new user");
                throw;
            }
        }

        /// <summary>
        /// تحديث مستخدم موجود
        /// Update an existing user
        /// </summary>
        public async Task<bool> UpdateAsync(User user)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    UPDATE Users
                    SET FirstName = @FirstName,
                        LastName = @LastName,
                        Email = @Email,
                        PhoneNumber = @PhoneNumber,
                        Address = @Address,
                        IsActive = @IsActive,
                        PasswordHash = @PasswordHash,
                        Role = @Role,
                        ModifiedDate = @ModifiedDate
                    WHERE UserId = @UserId";

                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(connection, sql, new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.Address,
                    user.IsActive,
                    user.PasswordHash,
                    Role = user.Role.ToString(),
                    user.ModifiedDate,
                    user.UserId
                });

                if (rowsAffected > 0)
                {
                    await InvalidateUserCacheAsync(user.UserId, user.Email);
                    _logger.LogDebug("تم تحديث المستخدم {UserId} - Updated user {UserId}", user.UserId, user.UserId);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث المستخدم {UserId} - Error updating user {UserId}", user.UserId, user.UserId);
                throw;
            }
        }

        /// <summary>
        /// حذف مستخدم
        /// Delete a user
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                // الحصول على المستخدم أولاً للحصول على البريد الإلكتروني
                var user = await GetByIdAsync(id);
                if (user == null) return false;

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = "DELETE FROM Users WHERE UserId = @UserId";
                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(connection, sql, new { UserId = id });

                if (rowsAffected > 0)
                {
                    await InvalidateUserCacheAsync(id, user.Email);
                    _logger.LogDebug("تم حذف المستخدم {UserId} - Deleted user {UserId}", id, id);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المستخدم {UserId} - Error deleting user {UserId}", id, id);
                throw;
            }
        }

        /// <summary>
        /// تفعيل أو إلغاء تفعيل مستخدم
        /// Activate or deactivate a user
        /// </summary>
        public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = "UPDATE Users SET IsActive = @IsActive WHERE UserId = @UserId";
                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(connection, sql, new { IsActive = isActive, UserId = id });

                if (rowsAffected > 0)
                {
                    await InvalidateUserCacheAsync(id);
                    _logger.LogDebug("تم تحديث حالة المستخدم {UserId} إلى {IsActive} - Updated user {UserId} active status to {IsActive}", id, isActive, id, isActive);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث حالة المستخدم {UserId} - Error updating user {UserId} active status", id, id);
                throw;
            }
        }

        /// <summary>
        /// التحقق من وجود مستخدم بالبريد الإلكتروني
        /// Check if user exists by email
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                var count = await DatabaseHelper.ExecuteScalarAsync<int>(connection, sql, new { Email = email });

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من وجود المستخدم بالبريد الإلكتروني {Email} - Error checking user existence by email {Email}", email, email);
                throw;
            }
        }

        /// <summary>
        /// الحصول على إحصائيات المستخدمين
        /// Get user statistics
        /// </summary>
        public async Task<UserStatistics> GetUserStatisticsAsync()
        {
            try
            {
                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedStats = await _cacheService.GetAsync<UserStatistics>(CacheKeys.Users.Statistics);
                if (cachedStats != null)
                {
                    _logger.LogDebug("تم الحصول على إحصائيات المستخدمين من التخزين المؤقت - Retrieved user statistics from cache");
                    return cachedStats;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT
                        COUNT(*) as TotalUsers,
                        SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActiveUsers,
                        SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) as InactiveUsers,
                        SUM(CASE WHEN MembershipDate >= DATEADD(MONTH, -1, GETDATE()) THEN 1 ELSE 0 END) as NewUsersThisMonth,
                        (SELECT COUNT(DISTINCT UserId) FROM Borrowings WHERE IsReturned = 0) as UsersWithActiveBorrowings
                    FROM Users";

                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql);

                var statistics = new UserStatistics();
                if (reader.Read())
                {
                    statistics.TotalUsers = reader.GetInt32(reader.GetOrdinal("TotalUsers"));
                    statistics.ActiveUsers = reader.GetInt32(reader.GetOrdinal("ActiveUsers"));
                    statistics.InactiveUsers = reader.GetInt32(reader.GetOrdinal("InactiveUsers"));
                    statistics.NewUsersThisMonth = reader.GetInt32(reader.GetOrdinal("NewUsersThisMonth"));
                    statistics.UsersWithActiveBorrowings = reader.GetInt32(reader.GetOrdinal("UsersWithActiveBorrowings"));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(CacheKeys.Users.Statistics, statistics, _cacheExpiration);

                _logger.LogDebug("تم الحصول على إحصائيات المستخدمين من قاعدة البيانات - Retrieved user statistics from database");
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على إحصائيات المستخدمين - Error getting user statistics");
                throw;
            }
        }

        /// <summary>
        /// الحصول على المستخدمين مع عدد الكتب المستعارة
        /// Get users with borrowed books count
        /// </summary>
        public async Task<IEnumerable<UserWithBorrowingCount>> GetUsersWithBorrowingCountAsync()
        {
            try
            {
                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedData = await _cacheService.GetAsync<List<UserWithBorrowingCount>>(CacheKeys.Users.WithBorrowingCount);
                if (cachedData != null)
                {
                    _logger.LogDebug("تم الحصول على المستخدمين مع عدد الاستعارات من التخزين المؤقت - Retrieved users with borrowing count from cache");
                    return cachedData;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT
                        u.UserId,
                        CONCAT(u.FirstName, ' ', u.LastName) as FullName,
                        u.Email,
                        ISNULL(current_borrowings.CurrentBorrowedBooks, 0) as CurrentBorrowedBooks,
                        ISNULL(total_borrowings.TotalBorrowedBooks, 0) as TotalBorrowedBooks,
                        ISNULL(overdue_borrowings.OverdueBooks, 0) as OverdueBooks,
                        ISNULL(late_fees.TotalLateFees, 0) as TotalLateFees
                    FROM Users u
                    LEFT JOIN (
                        SELECT UserId, COUNT(*) as CurrentBorrowedBooks
                        FROM Borrowings
                        WHERE IsReturned = 0
                        GROUP BY UserId
                    ) current_borrowings ON u.UserId = current_borrowings.UserId
                    LEFT JOIN (
                        SELECT UserId, COUNT(*) as TotalBorrowedBooks
                        FROM Borrowings
                        GROUP BY UserId
                    ) total_borrowings ON u.UserId = total_borrowings.UserId
                    LEFT JOIN (
                        SELECT UserId, COUNT(*) as OverdueBooks
                        FROM Borrowings
                        WHERE IsReturned = 0 AND DueDate < GETDATE()
                        GROUP BY UserId
                    ) overdue_borrowings ON u.UserId = overdue_borrowings.UserId
                    LEFT JOIN (
                        SELECT UserId, SUM(LateFee) as TotalLateFees
                        FROM Borrowings
                        GROUP BY UserId
                    ) late_fees ON u.UserId = late_fees.UserId
                    WHERE u.IsActive = 1
                    ORDER BY u.FirstName, u.LastName";

                var users = new List<UserWithBorrowingCount>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql);

                // الحصول على مؤشرات الأعمدة مرة واحدة
                int userIdIndex = reader.GetOrdinal("UserId");
                int fullNameIndex = reader.GetOrdinal("FullName");
                int emailIndex = reader.GetOrdinal("Email");
                int currentBorrowedBooksIndex = reader.GetOrdinal("CurrentBorrowedBooks");
                int totalBorrowedBooksIndex = reader.GetOrdinal("TotalBorrowedBooks");
                int overdueBooksIndex = reader.GetOrdinal("OverdueBooks");
                int totalLateFeesIndex = reader.GetOrdinal("TotalLateFees");

                while (reader.Read())
                {
                    users.Add(new UserWithBorrowingCount
                    {
                        UserId = reader.GetInt32(userIdIndex),
                        FullName = reader.GetString(fullNameIndex),
                        Email = reader.GetString(emailIndex),
                        CurrentBorrowedBooks = reader.GetInt32(currentBorrowedBooksIndex),
                        TotalBorrowedBooks = reader.GetInt32(totalBorrowedBooksIndex),
                        OverdueBooks = reader.GetInt32(overdueBooksIndex),
                        TotalLateFees = reader.GetDecimal(totalLateFeesIndex)
                    });
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(CacheKeys.Users.WithBorrowingCount, users, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} مستخدم مع عدد الاستعارات - Retrieved {Count} users with borrowing count", users.Count, users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدمين مع عدد الاستعارات - Error getting users with borrowing count");
                throw;
            }
        }

        /// <summary>
        /// إلغاء صحة التخزين المؤقت للمستخدم
        /// Invalidate user cache
        /// </summary>
        private async Task InvalidateUserCacheAsync(int userId, string? email = null)
        {
            await _cacheService.RemoveAsync(CacheKeys.Users.All);
            await _cacheService.RemoveAsync(CacheKeys.Users.Active);
            await _cacheService.RemoveAsync(CacheKeys.Users.ById(userId));

            if (!string.IsNullOrEmpty(email))
            {
                await _cacheService.RemoveAsync(CacheKeys.Users.ByEmail(email));
            }

            await _cacheService.RemoveAsync(CacheKeys.Users.Statistics);
            await _cacheService.RemoveAsync(CacheKeys.Users.WithBorrowingCount);
        }
    }
}
