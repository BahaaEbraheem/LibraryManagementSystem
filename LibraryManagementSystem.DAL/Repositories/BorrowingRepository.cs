using LibraryManagementSystem.DAL.Caching;
using LibraryManagementSystem.DAL.Data;
using LibraryManagementSystem.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Data;
using static LibraryManagementSystem.DAL.Caching.CacheKeys;

namespace LibraryManagementSystem.DAL.Repositories
{
    /// <summary>
    /// تنفيذ مستودع الاستعارات
    /// Borrowing repository implementation
    /// </summary>
    public class BorrowingRepository : IBorrowingRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ICacheService _cacheService;
        private readonly ILogger<BorrowingRepository> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

        public BorrowingRepository(
            IDatabaseConnectionFactory connectionFactory,
            ICacheService cacheService,
            ILogger<BorrowingRepository> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على جميع الاستعارات
        /// Get all borrowings
        /// </summary>
        public async Task<IEnumerable<Borrowing>> GetAllAsync()
        {
            try
            {
                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedBorrowings = await _cacheService.GetAsync<List<Borrowing>>(CacheKeys.Borrowings.All);
                if (cachedBorrowings != null)
                {
                    _logger.LogDebug("تم الحصول على جميع الاستعارات من التخزين المؤقت - Retrieved all borrowings from cache");
                    return cachedBorrowings;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT b.BorrowingId, b.UserId, b.BookId, b.BorrowDate, b.DueDate, 
                           b.ReturnDate, b.IsReturned, b.LateFee, b.Notes, 
                           b.CreatedDate, b.ModifiedDate,
                           u.FirstName, u.LastName, u.Email,
                           bk.Title, bk.Author, bk.ISBN
                    FROM Borrowings b
                    INNER JOIN Users u ON b.UserId = u.UserId
                    INNER JOIN Books bk ON b.BookId = bk.BookId
                    ORDER BY b.BorrowDate DESC";

                var borrowings = new List<Borrowing>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql);

                while (reader.Read())
                {
                    borrowings.Add(MapReaderToBorrowing(reader));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(CacheKeys.Borrowings.All, borrowings, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} استعارة من قاعدة البيانات - Retrieved borrowings from database", borrowings.Count);
                return borrowings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على جميع الاستعارات - Error getting all borrowings");
                throw;
            }
        }

        /// <summary>
        /// الحصول على استعارة بالمعرف
        /// Get borrowing by ID
        /// </summary>
        public async Task<Borrowing?> GetByIdAsync(int id)
        {
            try
            {
                var cacheKey = CacheKeys.Borrowings.ById(id);

                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedBorrowing = await _cacheService.GetAsync<Borrowing>(cacheKey);
                if (cachedBorrowing != null)
                {
                    _logger.LogDebug("تم الحصول على الاستعارة {BorrowingId} من التخزين المؤقت - Retrieved borrowing from cache", id);
                    return cachedBorrowing;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT b.BorrowingId, b.UserId, b.BookId, b.BorrowDate, b.DueDate, 
                           b.ReturnDate, b.IsReturned, b.LateFee, b.Notes, 
                           b.CreatedDate, b.ModifiedDate,
                           u.FirstName, u.LastName, u.Email,
                           bk.Title, bk.Author, bk.ISBN
                    FROM Borrowings b
                    INNER JOIN Users u ON b.UserId = u.UserId
                    INNER JOIN Books bk ON b.BookId = bk.BookId
                    WHERE b.BorrowingId = @BorrowingId";

                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { BorrowingId = id });

                if (reader.Read())
                {
                    var borrowing = MapReaderToBorrowing(reader);

                    // تخزين النتيجة في التخزين المؤقت
                    // Store result in cache
                    await _cacheService.SetAsync(cacheKey, borrowing, _cacheExpiration);

                    _logger.LogDebug("تم الحصول على الاستعارة {BorrowingId} من قاعدة البيانات - Retrieved borrowing from database", id);
                    return borrowing;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الاستعارة بالمعرف {BorrowingId} - Error getting borrowing by ID ", id);
                throw;
            }
        }

        /// <summary>
        /// الحصول على الاستعارات النشطة
        /// Get active borrowings
        /// </summary>
        public async Task<IEnumerable<Borrowing>> GetActiveBorrowingsAsync()
        {
            try
            {
                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedBorrowings = await _cacheService.GetAsync<List<Borrowing>>(CacheKeys.Borrowings.Active);
                if (cachedBorrowings != null)
                {
                    _logger.LogDebug("تم الحصول على الاستعارات النشطة من التخزين المؤقت - Retrieved active borrowings from cache");
                    return cachedBorrowings;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT b.BorrowingId, b.UserId, b.BookId, b.BorrowDate, b.DueDate, 
                           b.ReturnDate, b.IsReturned, b.LateFee, b.Notes, 
                           b.CreatedDate, b.ModifiedDate,
                           u.FirstName, u.LastName, u.Email,
                           bk.Title, bk.Author, bk.ISBN
                    FROM Borrowings b
                    INNER JOIN Users u ON b.UserId = u.UserId
                    INNER JOIN Books bk ON b.BookId = bk.BookId
                    WHERE b.IsReturned = 0
                    ORDER BY b.DueDate ASC";

                var borrowings = new List<Borrowing>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql);

                while (reader.Read())
                {
                    borrowings.Add(MapReaderToBorrowing(reader));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(CacheKeys.Borrowings.Active, borrowings, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} استعارة نشطة - Retrieved active borrowings", borrowings.Count);
                return borrowings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الاستعارات النشطة - Error getting active borrowings");
                throw;
            }
        }

        /// <summary>
        /// الحصول على الاستعارات المتأخرة
        /// Get overdue borrowings
        /// </summary>
        public async Task<IEnumerable<Borrowing>> GetOverdueBorrowingsAsync()
        {
            try
            {
                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedBorrowings = await _cacheService.GetAsync<List<Borrowing>>(CacheKeys.Borrowings.Overdue);
                if (cachedBorrowings != null)
                {
                    _logger.LogDebug("تم الحصول على الاستعارات المتأخرة من التخزين المؤقت - Retrieved overdue borrowings from cache");
                    return cachedBorrowings;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT b.BorrowingId, b.UserId, b.BookId, b.BorrowDate, b.DueDate, 
                           b.ReturnDate, b.IsReturned, b.LateFee, b.Notes, 
                           b.CreatedDate, b.ModifiedDate,
                           u.FirstName, u.LastName, u.Email,
                           bk.Title, bk.Author, bk.ISBN
                    FROM Borrowings b
                    INNER JOIN Users u ON b.UserId = u.UserId
                    INNER JOIN Books bk ON b.BookId = bk.BookId
                    WHERE b.IsReturned = 0 AND b.DueDate < GETDATE()
                    ORDER BY b.DueDate ASC";

                var borrowings = new List<Borrowing>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql);

                while (reader.Read())
                {
                    borrowings.Add(MapReaderToBorrowing(reader));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(CacheKeys.Borrowings.Overdue, borrowings, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} استعارة متأخرة - Retrieved overdue borrowings", borrowings.Count);
                return borrowings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الاستعارات المتأخرة - Error getting overdue borrowings");
                throw;
            }
        }

        /// <summary>
        /// الحصول على استعارات المستخدم
        /// Get user borrowings
        /// </summary>
        public async Task<IEnumerable<Borrowing>> GetUserBorrowingsAsync(int userId)
        {
            try
            {
                var cacheKey = CacheKeys.Borrowings.ByUser(userId);

                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedBorrowings = await _cacheService.GetAsync<List<Borrowing>>(cacheKey);
                if (cachedBorrowings != null)
                {
                    _logger.LogDebug("تم الحصول على استعارات المستخدم {UserId} من التخزين المؤقت - Retrieved user borrowings from cache", userId);
                    return cachedBorrowings;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT b.BorrowingId, b.UserId, b.BookId, b.BorrowDate, b.DueDate,
                           b.ReturnDate, b.IsReturned, b.LateFee, b.Notes,
                           b.CreatedDate, b.ModifiedDate,
                           u.FirstName, u.LastName, u.Email,
                           bk.Title, bk.Author, bk.ISBN
                    FROM Borrowings b
                    INNER JOIN Users u ON b.UserId = u.UserId
                    INNER JOIN Books bk ON b.BookId = bk.BookId
                    WHERE b.UserId = @UserId
                    ORDER BY b.BorrowDate DESC";

                var borrowings = new List<Borrowing>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { UserId = userId });

                while (reader.Read())
                {
                    borrowings.Add(MapReaderToBorrowing(reader));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(cacheKey, borrowings, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} استعارة للمستخدم {UserId} - Retrieved borrowings for user", borrowings.Count, userId);
                return borrowings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على استعارات المستخدم {UserId} - Error getting userborrowings", userId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على الاستعارات النشطة للمستخدم
        /// Get active user borrowings
        /// </summary>
        public async Task<IEnumerable<Borrowing>> GetActiveUserBorrowingsAsync(int userId)
        {
            try
            {
                var cacheKey = CacheKeys.Borrowings.ActiveByUser(userId);

                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedBorrowings = await _cacheService.GetAsync<List<Borrowing>>(cacheKey);
                if (cachedBorrowings != null)
                {
                    _logger.LogDebug("تم الحصول على الاستعارات النشطة للمستخدم {UserId} من التخزين المؤقت - Retrieved active borrowings for user from cache", userId);
                    return cachedBorrowings;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT b.BorrowingId, b.UserId, b.BookId, b.BorrowDate, b.DueDate,
                           b.ReturnDate, b.IsReturned, b.LateFee, b.Notes,
                           b.CreatedDate, b.ModifiedDate,
                           u.FirstName, u.LastName, u.Email,
                           bk.Title, bk.Author, bk.ISBN
                    FROM Borrowings b
                    INNER JOIN Users u ON b.UserId = u.UserId
                    INNER JOIN Books bk ON b.BookId = bk.BookId
                    WHERE b.UserId = @UserId AND b.IsReturned = 0
                    ORDER BY b.DueDate ASC";

                var borrowings = new List<Borrowing>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { UserId = userId });

                while (reader.Read())
                {
                    borrowings.Add(MapReaderToBorrowing(reader));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(cacheKey, borrowings, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} استعارة نشطة للمستخدم {UserId} - Retrieved active borrowings for user", borrowings.Count, userId);
                return borrowings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الاستعارات النشطة للمستخدم {UserId} - Error getting active borrowings for user", userId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على استعارات الكتاب
        /// Get book borrowings
        /// </summary>
        public async Task<IEnumerable<Borrowing>> GetBookBorrowingsAsync(int bookId)
        {
            try
            {
                var cacheKey = CacheKeys.Borrowings.ByBook(bookId);

                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedBorrowings = await _cacheService.GetAsync<List<Borrowing>>(cacheKey);
                if (cachedBorrowings != null)
                {
                    _logger.LogDebug("تم الحصول على استعارات الكتاب {BookId} من التخزين المؤقت - Retrieved book borrowings from cache", bookId);
                    return cachedBorrowings;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT b.BorrowingId, b.UserId, b.BookId, b.BorrowDate, b.DueDate,
                           b.ReturnDate, b.IsReturned, b.LateFee, b.Notes,
                           b.CreatedDate, b.ModifiedDate,
                           u.FirstName, u.LastName, u.Email,
                           bk.Title, bk.Author, bk.ISBN
                    FROM Borrowings b
                    INNER JOIN Users u ON b.UserId = u.UserId
                    INNER JOIN Books bk ON b.BookId = bk.BookId
                    WHERE b.BookId = @BookId
                    ORDER BY b.BorrowDate DESC";

                var borrowings = new List<Borrowing>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { BookId = bookId });

                while (reader.Read())
                {
                    borrowings.Add(MapReaderToBorrowing(reader));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(cacheKey, borrowings, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} استعارة للكتاب  - Retrieved {Count} borrowings for book ", borrowings.Count, bookId);
                return borrowings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على استعارات الكتاب {BookId} - Error getting bookborrowings", bookId);
                throw;
            }
        }

        /// <summary>
        /// إضافة استعارة جديدة
        /// Add a new borrowing
        /// </summary>
        public async Task<int> AddAsync(Borrowing borrowing)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    INSERT INTO Borrowings (UserId, BookId, BorrowDate, DueDate, IsReturned, LateFee, Notes)
                    VALUES (@UserId, @BookId, @BorrowDate, @DueDate, @IsReturned, @LateFee, @Notes);
                    SELECT SCOPE_IDENTITY();";

                var borrowingId = await DatabaseHelper.ExecuteScalarAsync<int>(connection, sql, new
                {
                    borrowing.UserId,
                    borrowing.BookId,
                    borrowing.BorrowDate,
                    borrowing.DueDate,
                    borrowing.IsReturned,
                    borrowing.LateFee,
                    borrowing.Notes
                });

                await InvalidateBorrowingCacheAsync(borrowingId, borrowing.UserId, borrowing.BookId);
                _logger.LogDebug("تم إضافة استعارة جديدة بالمعرف {BorrowingId} - Added new borrowing with ID ", borrowingId);

                return borrowingId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة استعارة جديدة - Error adding new borrowing");
                throw;
            }
        }

        /// <summary>
        /// تحديث استعارة موجودة
        /// Update an existing borrowing
        /// </summary>
        public async Task<bool> UpdateAsync(Borrowing borrowing)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    UPDATE Borrowings
                    SET DueDate = @DueDate,
                        ReturnDate = @ReturnDate,
                        IsReturned = @IsReturned,
                        LateFee = @LateFee,
                        Notes = @Notes
                    WHERE BorrowingId = @BorrowingId";

                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(connection, sql, new
                {
                    borrowing.DueDate,
                    borrowing.ReturnDate,
                    borrowing.IsReturned,
                    borrowing.LateFee,
                    borrowing.Notes,
                    borrowing.BorrowingId
                });

                if (rowsAffected > 0)
                {
                    await InvalidateBorrowingCacheAsync(borrowing.BorrowingId, borrowing.UserId, borrowing.BookId);
                    _logger.LogDebug("تم تحديث الاستعارة");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// إرجاع كتاب
        /// Return a book
        /// </summary>
        public async Task<bool> ReturnBookAsync(int borrowingId, DateTime returnDate, decimal lateFee = 0, string? notes = null)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    UPDATE Borrowings
                    SET ReturnDate = @ReturnDate,
                        IsReturned = 1,
                        LateFee = @LateFee,
                        Notes = @Notes
                    WHERE BorrowingId = @BorrowingId AND IsReturned = 0";

                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(connection, sql, new
                {
                    ReturnDate = returnDate,
                    LateFee = lateFee,
                    Notes = notes,
                    BorrowingId = borrowingId
                });

                if (rowsAffected > 0)
                {
                    // الحصول على معلومات الاستعارة لإلغاء التخزين المؤقت
                    var borrowing = await GetByIdAsync(borrowingId);
                    if (borrowing != null)
                    {
                        await InvalidateBorrowingCacheAsync(borrowingId, borrowing.UserId, borrowing.BookId);
                    }

                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// حذف استعارة
        /// Delete a borrowing
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                // الحصول على الاستعارة أولاً للحصول على معلومات المستخدم والكتاب
                var borrowing = await GetByIdAsync(id);
                if (borrowing == null) return false;

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = "DELETE FROM Borrowings WHERE BorrowingId = @BorrowingId";
                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(connection, sql, new { BorrowingId = id });

                if (rowsAffected > 0)
                {
                    await InvalidateBorrowingCacheAsync(id, borrowing.UserId, borrowing.BookId);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// تحويل قارئ البيانات إلى كائن استعارة
        /// Map data reader to borrowing object
        /// </summary>
        private static Borrowing MapReaderToBorrowing(IDataReader reader)
        {
            return new Borrowing
            {
                BorrowingId = reader.GetInt32(reader.GetOrdinal("BorrowingId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                BookId = reader.GetInt32(reader.GetOrdinal("BookId")),
                BorrowDate = reader.GetDateTime(reader.GetOrdinal("BorrowDate")),
                DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                ReturnDate = reader.IsDBNull(reader.GetOrdinal("ReturnDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ReturnDate")),
                IsReturned = reader.GetBoolean(reader.GetOrdinal("IsReturned")),
                LateFee = reader.GetDecimal(reader.GetOrdinal("LateFee")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                ModifiedDate = reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                User = new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.GetString(reader.GetOrdinal("Email"))
                },
                Book = new Book
                {
                    BookId = reader.GetInt32(reader.GetOrdinal("BookId")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Author = reader.GetString(reader.GetOrdinal("Author")),
                    ISBN = reader.GetString(reader.GetOrdinal("ISBN"))
                }
            };
        }

        /// <summary>
        /// التحقق من إمكانية استعارة كتاب
        /// Check if a book can be borrowed
        /// </summary>
        public async Task<bool> CanBorrowBookAsync(int userId, int bookId)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                // التحقق من وجود استعارة نشطة لنفس الكتاب من نفس المستخدم
                const string checkActiveBorrowingSql = @"
                    SELECT COUNT(*)
                    FROM Borrowings
                    WHERE UserId = @UserId AND BookId = @BookId AND IsReturned = 0";

                var activeBorrowingCount = await DatabaseHelper.ExecuteScalarAsync<int>(connection, checkActiveBorrowingSql, new { UserId = userId, BookId = bookId });

                if (activeBorrowingCount > 0)
                {
                    return false; // المستخدم لديه استعارة نشطة لنفس الكتاب
                }

                // التحقق من توفر نسخ من الكتاب
                const string checkAvailabilitySql = "SELECT AvailableCopies FROM Books WHERE BookId = @BookId";
                var availableCopies = await DatabaseHelper.ExecuteScalarAsync<int>(connection, checkAvailabilitySql, new { BookId = bookId });

                return availableCopies > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// الحصول على عدد الكتب المستعارة حالياً للمستخدم
        /// Get current borrowed books count for user
        /// </summary>
        public async Task<int> GetCurrentBorrowedBooksCountAsync(int userId)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = "SELECT COUNT(*) FROM Borrowings WHERE UserId = @UserId AND IsReturned = 0";
                var count = await DatabaseHelper.ExecuteScalarAsync<int>(connection, sql, new { UserId = userId });

                return count;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// الحصول على إحصائيات الاستعارات
        /// Get borrowing statistics
        /// </summary>
        public async Task<BorrowingStatistics> GetBorrowingStatisticsAsync()
        {
            try
            {
                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedStats = await _cacheService.GetAsync<BorrowingStatistics>(CacheKeys.Borrowings.Statistics);
                if (cachedStats != null)
                {
                    _logger.LogDebug("تم الحصول على إحصائيات الاستعارات من التخزين المؤقت - Retrieved borrowing statistics from cache");
                    return cachedStats;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT
       COUNT(*) as TotalBorrowings, -- إجمالي عدد الاستعارات
       SUM(CASE WHEN IsReturned = 0 THEN 1 ELSE 0 END) as ActiveBorrowings, -- عدد الاستعارات النشطة (غير مُعادة)
       SUM(CASE WHEN IsReturned = 0 AND DueDate < GETDATE() THEN 1 ELSE 0 END) as OverdueBorrowings, -- عدد الاستعارات المتأخرة (تاريخ الإرجاع انتهى ولم تُعاد)
       SUM(CASE WHEN IsReturned = 1 THEN 1 ELSE 0 END) as ReturnedBorrowings, -- عدد الكتب المُعادة
       SUM(LateFee) as TotalLateFees, -- مجموع الغرامات المتأخرة
       AVG(CASE WHEN IsReturned = 1 THEN DATEDIFF(DAY, BorrowDate, ReturnDate) ELSE NULL END) as AverageBorrowingPeriod, -- متوسط فترة الاستعارة (بالأيام)
       SUM(CASE WHEN BorrowDate >= DATEADD(MONTH, -1, GETDATE()) THEN 1 ELSE 0 END) as BorrowingsThisMonth, -- عدد الاستعارات خلال هذا الشهر (آخر 30 يوم تقريبًا)
       SUM(CASE WHEN IsReturned = 1 AND ReturnDate >= DATEADD(MONTH, -1, GETDATE()) THEN 1 ELSE 0 END) as ReturnsThisMonth -- عدد الإرجاعات خلال هذا الشهر
       FROM Borrowings";

                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql);

                var statistics = new BorrowingStatistics();
                if (reader.Read())
                {
                    statistics.TotalBorrowings = reader.IsDBNull(reader.GetOrdinal("TotalBorrowings")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalBorrowings"));
                    statistics.ActiveBorrowings = reader.IsDBNull(reader.GetOrdinal("ActiveBorrowings")) ? 0 : reader.GetInt32(reader.GetOrdinal("ActiveBorrowings"));
                    statistics.OverdueBorrowings = reader.IsDBNull(reader.GetOrdinal("OverdueBorrowings")) ? 0 : reader.GetInt32(reader.GetOrdinal("OverdueBorrowings"));
                    statistics.ReturnedBorrowings = reader.IsDBNull(reader.GetOrdinal("ReturnedBorrowings")) ? 0 : reader.GetInt32(reader.GetOrdinal("ReturnedBorrowings"));
                    statistics.TotalLateFees = reader.IsDBNull(reader.GetOrdinal("TotalLateFees")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalLateFees"));
                    statistics.AverageBorrowingPeriod = reader.IsDBNull(reader.GetOrdinal("AverageBorrowingPeriod"))? 0: reader.GetInt32(reader.GetOrdinal("AverageBorrowingPeriod"));
                    statistics.BorrowingsThisMonth = reader.IsDBNull(reader.GetOrdinal("BorrowingsThisMonth")) ? 0 : reader.GetInt32(reader.GetOrdinal("BorrowingsThisMonth"));
                    statistics.ReturnsThisMonth = reader.IsDBNull(reader.GetOrdinal("ReturnsThisMonth")) ? 0 : reader.GetInt32(reader.GetOrdinal("ReturnsThisMonth"));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(CacheKeys.Borrowings.Statistics, statistics, _cacheExpiration);

                _logger.LogDebug("تم الحصول على إحصائيات الاستعارات من قاعدة البيانات - Retrieved borrowing statistics from database");
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على إحصائيات الاستعارات - Error getting borrowing statistics");
                throw;
            }
        }

        /// <summary>
        /// الحصول على الكتب الأكثر استعارة
        /// Get most borrowed books
        /// </summary>
        public async Task<IEnumerable<MostBorrowedBook>> GetMostBorrowedBooksAsync(int topCount = 10)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT TOP (@TopCount)
                        b.BookId,
                        b.Title,
                        b.Author,
                        b.ISBN,
                        COUNT(*) as BorrowCount,
                        SUM(CASE WHEN br.IsReturned = 0 THEN 1 ELSE 0 END) as CurrentActiveBorrowings
                    FROM Books b
                    INNER JOIN Borrowings br ON b.BookId = br.BookId
                    GROUP BY b.BookId, b.Title, b.Author, b.ISBN
                    ORDER BY BorrowCount DESC";

                var books = new List<MostBorrowedBook>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { TopCount = topCount });

                while (reader.Read())
                {
                    books.Add(new MostBorrowedBook
                    {
                        BookId = reader.GetInt32(reader.GetOrdinal("BookId")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Author = reader.GetString(reader.GetOrdinal("Author")),
                        ISBN = reader.GetString(reader.GetOrdinal("ISBN")),
                        BorrowCount = reader.GetInt32(reader.GetOrdinal("BorrowCount")),
                        CurrentActiveBorrowings = reader.GetInt32(reader.GetOrdinal("CurrentActiveBorrowings"))
                    });
                }

                return books;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الكتب الأكثر استعارة - Error getting most borrowed books");
                throw;
            }
        }

        /// <summary>
        /// الحصول على المستخدمين الأكثر نشاطاً في الاستعارة
        /// Get most active borrowing users
        /// </summary>
        public async Task<IEnumerable<MostActiveUser>> GetMostActiveUsersAsync(int topCount = 10)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT TOP (@TopCount)
                        u.UserId,
                        CONCAT(u.FirstName, ' ', u.LastName) as FullName,
                        u.Email,
                        COUNT(*) as TotalBorrowings,
                        SUM(CASE WHEN br.IsReturned = 0 THEN 1 ELSE 0 END) as CurrentActiveBorrowings,
                        SUM(CASE WHEN br.IsReturned = 0 AND br.DueDate < GETDATE() THEN 1 ELSE 0 END) as OverdueBooks
                    FROM Users u
                    INNER JOIN Borrowings br ON u.UserId = br.UserId
                    WHERE u.IsActive = 1
                    GROUP BY u.UserId, u.FirstName, u.LastName, u.Email
                    ORDER BY TotalBorrowings DESC";

                var users = new List<MostActiveUser>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { TopCount = topCount });

                while (reader.Read())
                {
                    users.Add(new MostActiveUser
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        TotalBorrowings = reader.GetInt32(reader.GetOrdinal("TotalBorrowings")),
                        CurrentActiveBorrowings = reader.GetInt32(reader.GetOrdinal("CurrentActiveBorrowings")),
                        OverdueBooks = reader.GetInt32(reader.GetOrdinal("OverdueBooks"))
                    });
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدمين الأكثر نشاطاً - Error getting most active users");
                throw;
            }
        }

        /// <summary>
        /// إلغاء صحة التخزين المؤقت للاستعارة
        /// Invalidate borrowing cache
        /// </summary>
        private async Task InvalidateBorrowingCacheAsync(int borrowingId, int userId, int bookId)
        {
            await _cacheService.RemoveAsync(CacheKeys.Borrowings.All);
            await _cacheService.RemoveAsync(CacheKeys.Borrowings.Active);
            await _cacheService.RemoveAsync(CacheKeys.Borrowings.Overdue);
            await _cacheService.RemoveAsync(CacheKeys.Borrowings.ById(borrowingId));
            await _cacheService.RemoveAsync(CacheKeys.Borrowings.ByUser(userId));
            await _cacheService.RemoveAsync(CacheKeys.Borrowings.ByBook(bookId));
            await _cacheService.RemoveAsync(CacheKeys.Borrowings.ActiveByUser(userId));
            await _cacheService.RemoveAsync(CacheKeys.Borrowings.Statistics);
        }

        public async Task<bool> HasBorrowingsAsync(int bookId)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = "SELECT COUNT(*) FROM Borrowings WHERE BookId = @bookId AND IsReturned = 1";

                var result = await DatabaseHelper.ExecuteScalarAsync<int>(connection, sql, new { BookId = bookId });

                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking borrowings: {ex.Message}");
                throw;
            }
        }
    }
}
