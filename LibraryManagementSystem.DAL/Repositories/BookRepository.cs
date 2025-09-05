using LibraryManagementSystem.DAL.Caching;
using LibraryManagementSystem.DAL.Data;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text;

namespace LibraryManagementSystem.DAL.Repositories
{
    /// <summary>
    /// تنفيذ مستودع الكتب باستخدام ADO.NET والتخزين المؤقت
    /// Book repository implementation using ADO.NET and caching
    /// </summary>
    public class BookRepository : IBookRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ICacheService _cacheService;
        private readonly ILogger<BookRepository> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        /// <summary>
        /// منشئ مستودع الكتب
        /// Book repository constructor
        /// </summary>
        public BookRepository(
            IDatabaseConnectionFactory connectionFactory,
            ICacheService cacheService,
            ILogger<BookRepository> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على جميع الكتب
        /// Get all books
        /// </summary>
        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            try
            {
                // محاولة الحصول على البيانات من التخزين المؤقت أولاً
                // Try to get data from cache first
                var cachedBooks = await _cacheService.GetAsync<List<Book>>(CacheKeys.Books.All);
                if (cachedBooks != null)
                {
                    _logger.LogDebug("تم الحصول على جميع الكتب من التخزين المؤقت - Retrieved all books from cache");
                    return cachedBooks;
                }

                // إذا لم توجد في التخزين المؤقت، احصل عليها من قاعدة البيانات
                // If not in cache, get from database
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT BookId, Title, Author, ISBN, Publisher, PublicationYear, 
                           Genre, TotalCopies, AvailableCopies, Description, 
                           CreatedDate, ModifiedDate
                    FROM Books 
                    ORDER BY Title";

                var books = new List<Book>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql);

                while (reader.Read())
                {
                    books.Add(MapReaderToBook(reader));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(CacheKeys.Books.All, books, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} كتاب من قاعدة البيانات - Retrieved {Count} books from database", books.Count, books.Count);
                return books;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على جميع الكتب - Error getting all books");
                throw;
            }
        }

        /// <summary>
        /// الحصول على كتاب بالمعرف
        /// Get book by ID
        /// </summary>
        public async Task<Book?> GetByIdAsync(int id)
        {
            try
            {
                var cacheKey = CacheKeys.Books.ById(id);

                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedBook = await _cacheService.GetAsync<Book>(cacheKey);
                if (cachedBook != null)
                {
                    _logger.LogDebug("تم الحصول على الكتاب {BookId} من التخزين المؤقت - Retrieved book {BookId} from cache", id, id);
                    return cachedBook;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT BookId, Title, Author, ISBN, Publisher, PublicationYear, 
                           Genre, TotalCopies, AvailableCopies, Description, 
                           CreatedDate, ModifiedDate
                    FROM Books 
                    WHERE BookId = @BookId";

                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { BookId = id });

                if (reader.Read())
                {
                    var book = MapReaderToBook(reader);

                    // تخزين النتيجة في التخزين المؤقت
                    // Store result in cache
                    await _cacheService.SetAsync(cacheKey, book, _cacheExpiration);

                    _logger.LogDebug("تم الحصول على الكتاب {BookId} من قاعدة البيانات - Retrieved book {BookId} from database", id, id);
                    return book;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الكتاب بالمعرف {BookId} - Error getting book by ID {BookId}", id, id);
                throw;
            }
        }

        /// <summary>
        /// الحصول على كتاب بالرقم المعياري الدولي
        /// Get book by ISBN
        /// </summary>
        public async Task<Book?> GetByIsbnAsync(string isbn)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(isbn))
                    return null;

                var cacheKey = CacheKeys.Books.ByIsbn(isbn);

                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedBook = await _cacheService.GetAsync<Book>(cacheKey);
                if (cachedBook != null)
                {
                    _logger.LogDebug("تم الحصول على الكتاب بالرقم المعياري {ISBN} من التخزين المؤقت - Retrieved book by ISBN {ISBN} from cache", isbn, isbn);
                    return cachedBook;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT BookId, Title, Author, ISBN, Publisher, PublicationYear, 
                           Genre, TotalCopies, AvailableCopies, Description, 
                           CreatedDate, ModifiedDate
                    FROM Books 
                    WHERE ISBN = @ISBN";

                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { ISBN = isbn });

                if (reader.Read())
                {
                    var book = MapReaderToBook(reader);

                    // تخزين النتيجة في التخزين المؤقت
                    // Store result in cache
                    await _cacheService.SetAsync(cacheKey, book, _cacheExpiration);

                    _logger.LogDebug("تم الحصول على الكتاب بالرقم المعياري {ISBN} من قاعدة البيانات - Retrieved book by ISBN {ISBN} from database", isbn, isbn);
                    return book;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الكتاب بالرقم المعياري {ISBN} - Error getting book by ISBN {ISBN}", isbn, isbn);
                throw;
            }
        }

        /// <summary>
        /// تحويل قارئ البيانات إلى كائن كتاب
        /// Map data reader to book object
        /// </summary>
        private static Book MapReaderToBook(IDataReader reader)
        {
            return new Book
            {
                BookId = reader.GetInt32(reader.GetOrdinal("BookId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Author = reader.GetString(reader.GetOrdinal("Author")),
                ISBN = reader.GetString(reader.GetOrdinal("ISBN")),
                Publisher = reader.IsDBNull(reader.GetOrdinal("Publisher")) ? null : reader.GetString(reader.GetOrdinal("Publisher")),
                PublicationYear = reader.IsDBNull(reader.GetOrdinal("PublicationYear")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("PublicationYear")),
                Genre = reader.IsDBNull(reader.GetOrdinal("Genre")) ? null : reader.GetString(reader.GetOrdinal("Genre")),
                TotalCopies = reader.GetInt32(reader.GetOrdinal("TotalCopies")),
                AvailableCopies = reader.GetInt32(reader.GetOrdinal("AvailableCopies")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                ModifiedDate = reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
            };
        }

        /// <summary>
        /// البحث عن الكتب مع التقسيم على صفحات
        /// Search books with pagination
        /// </summary>
        public async Task<PagedResult<Book>> SearchAsync(BookSearchDto searchDto)
        {
            try
            {
                var cacheKey = GenerateSearchCacheKey(searchDto);

                // تعطيل التخزين المؤقت مؤقتاً للتشخيص - Disable cache temporarily for debugging
                // var cachedResult = await _cacheService.GetAsync<PagedResult<Book>>(cacheKey);
                // if (cachedResult != null)
                //     return cachedResult;

                _logger.LogDebug("تم تجاهل التخزين المؤقت للبحث - Cache bypassed for search debugging");

                using var connection = await _connectionFactory.CreateConnectionAsync();

                var (whereClause, parameters) = BuildSearchWhereClause(searchDto);
                var orderClause = BuildOrderClause(searchDto.SortBy, searchDto.SortDescending);

                // تسجيل استعلام البحث للتشخيص
                // Log search query for debugging
                _logger.LogDebug("Search query: SELECT COUNT(*) FROM Books {WhereClause} with SearchTerm: {SearchTerm}",
                    whereClause, searchDto.SearchTerm);

                // عد إجمالي النتائج
                var countSql = $"SELECT COUNT(*) FROM Books {whereClause}";
                var totalCount = await DatabaseHelper.ExecuteScalarAsync<int>(connection, countSql, parameters);

                // استعلام البيانات مع الصفحات
                var offset = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var dataSql = $@"
            SELECT BookId, Title, Author, ISBN, Publisher, PublicationYear,
                   Genre, TotalCopies, AvailableCopies, Description,
                   CreatedDate, ModifiedDate
            FROM Books
            {whereClause}
            {orderClause}
            OFFSET {offset} ROWS
            FETCH NEXT {searchDto.PageSize} ROWS ONLY";

                var books = new List<Book>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, dataSql, parameters);
                while (reader.Read())
                {
                    var book = MapReaderToBook(reader);
                    books.Add(book);

                    // تسجيل تفاصيل كل كتاب تم العثور عليه
                    // Log details of each book found
                    _logger.LogDebug("كتاب تم العثور عليه - Book found: ID={BookId}, Title='{Title}', Author='{Author}', ISBN='{ISBN}'",
                        book.BookId, book.Title, book.Author, book.ISBN);
                }

                var result = new PagedResult<Book>
                {
                    Items = books,
                    TotalCount = totalCount,
                    PageNumber = searchDto.PageNumber,
                    PageSize = searchDto.PageSize
                };

                // تسجيل النتائج النهائية
                // Log final results
                _logger.LogInformation("نتائج البحث - Search results: العدد الإجمالي={TotalCount}, عدد العناصر في الصفحة={ItemCount}, مصطلح البحث='{SearchTerm}'",
                    totalCount, books.Count, searchDto.SearchTerm ?? "فارغ");

                // تعطيل التخزين المؤقت مؤقتاً
                // Disable caching temporarily
                // await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في البحث عن الكتب - Error searching books");
                throw;
            }
        }


        /// <summary>
        /// الحصول على الكتب المتاحة
        /// Get available books
        /// </summary>
        public async Task<IEnumerable<Book>> GetAvailableAsync()
        {
            try
            {
                // محاولة الحصول على البيانات من التخزين المؤقت
                // Try to get data from cache
                var cachedBooks = await _cacheService.GetAsync<List<Book>>(CacheKeys.Books.Available);
                if (cachedBooks != null)
                {
                    _logger.LogDebug("تم الحصول على الكتب المتاحة من التخزين المؤقت - Retrieved available books from cache");
                    return cachedBooks;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT BookId, Title, Author, ISBN, Publisher, PublicationYear,
                           Genre, TotalCopies, AvailableCopies, Description,
                           CreatedDate, ModifiedDate
                    FROM Books
                    WHERE AvailableCopies > 0
                    ORDER BY Title";

                var books = new List<Book>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql);

                while (reader.Read())
                {
                    books.Add(MapReaderToBook(reader));
                }

                // تخزين النتائج في التخزين المؤقت
                // Store results in cache
                await _cacheService.SetAsync(CacheKeys.Books.Available, books, _cacheExpiration);

                _logger.LogDebug("تم الحصول على {Count} كتاب متاح - Retrieved {Count} available books", books.Count, books.Count);
                return books;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الكتب المتاحة - Error getting available books");
                throw;
            }
        }

        /// <summary>
        /// إضافة كتاب جديد
        /// Add a new book
        /// </summary>
        public async Task<int> AddAsync(Book book)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    INSERT INTO Books (Title, Author, ISBN, Publisher, PublicationYear,
                                     Genre, TotalCopies, AvailableCopies, Description)
                    VALUES (@Title, @Author, @ISBN, @Publisher, @PublicationYear,
                            @Genre, @TotalCopies, @AvailableCopies, @Description);
                    SELECT SCOPE_IDENTITY();";

                var bookId = await DatabaseHelper.ExecuteScalarAsync<int>(connection, sql, new
                {
                    book.Title,
                    book.Author,
                    book.ISBN,
                    book.Publisher,
                    book.PublicationYear,
                    book.Genre,
                    book.TotalCopies,
                    book.AvailableCopies,
                    book.Description
                });

                // إلغاء صحة التخزين المؤقت
                // Invalidate cache
                await InvalidateBookCacheAsync();

                _logger.LogInformation("تم إضافة كتاب جديد بالمعرف {BookId}: {Title} - Added new book with ID {BookId}: {Title}",
                    bookId, book.Title, bookId, book.Title);

                return bookId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة الكتاب: {Title} - Error adding book: {Title}", book.Title, book.Title);
                throw;
            }
        }

        /// <summary>
        /// إزالة البيانات المتعلقة بالكتب من التخزين المؤقت
        /// Remove book-related data from cache
        /// </summary>
        private async Task InvalidateBookCacheAsync(int? bookId = null)
        {
            try
            {
                // إزالة جميع مفاتيح الكتب العامة
                await _cacheService.RemoveByPatternAsync(CacheKeys.Patterns.AllBooks);
                await _cacheService.RemoveByPatternAsync(CacheKeys.Patterns.AllSearch);
                await _cacheService.RemoveByPatternAsync(CacheKeys.Patterns.AllStatistics);

                if (bookId.HasValue)
                {
                    // إزالة كاش الكتاب نفسه
                    await _cacheService.RemoveAsync(CacheKeys.Books.ById(bookId.Value));

                    // إزالة كاش الكتاب حسب ISBN
                    var book = await GetByIdAsync(bookId.Value);
                    if (book != null)
                        await _cacheService.RemoveAsync(CacheKeys.Books.ByIsbn(book.ISBN));

                    // إزالة مفاتيح الاستعارات المتعلقة بهذا الكتاب
                    await _cacheService.RemoveAsync(CacheKeys.Borrowings.ByBook(bookId.Value));
                }

                _logger.LogDebug("تم إلغاء صحة التخزين المؤقت للكتب - Invalidated book cache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إلغاء صحة التخزين المؤقت للكتب - Error invalidating book cache");
            }
        }


        /// <summary>
        /// إنشاء مفتاح التخزين المؤقت للبحث
        /// Generate search cache key
        /// </summary>
        private static string GenerateSearchCacheKey(BookSearchDto searchDto)
        {
            return CacheKeys.Search.Advanced(
                searchDto.Title ?? "",
                searchDto.Author ?? "",
                searchDto.ISBN ?? "",
                searchDto.Genre ?? "",
                searchDto.PageNumber,
                searchDto.PageSize);
        }

        /// <summary>
        /// بناء جملة WHERE للبحث
        /// Build WHERE clause for search
        /// </summary>
        private static (string whereClause, object parameters) BuildSearchWhereClause(BookSearchDto searchDto)
        {
            var conditions = new List<string>();

            // متغيرات للمعاملات
            string? searchTerm = null;
            string? title = null;
            string? author = null;
            string? isbn = null;
            string? genre = null;

            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm) && searchDto.SearchTerm.Trim().Length >= 2)
            {
                var trimmed = searchDto.SearchTerm.Trim();
                searchTerm = $"%{trimmed}%";

                // البحث الدقيق - ابحث عن العبارة الكاملة كما هي
                // Exact search - search for the complete phrase as is
                conditions.Add("(Title LIKE @SearchTerm OR Author LIKE @SearchTerm OR ISBN LIKE @SearchTerm)");

                // تسجيل معايير البحث
                // Log search criteria
                Console.WriteLine($"DEBUG: Search term: '{trimmed}', Pattern: '%{trimmed}%'");
                Console.WriteLine($"DEBUG: SQL condition will be: (Title LIKE @SearchTerm OR Author LIKE @SearchTerm OR ISBN LIKE @SearchTerm)");
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Title) && searchDto.Title.Trim().Length >= 2)
            {
                title = $"%{searchDto.Title.Trim()}%";
                conditions.Add("Title LIKE @Title");
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Author) && searchDto.Author.Trim().Length >= 2)
            {
                author = $"%{searchDto.Author.Trim()}%";
                conditions.Add("Author LIKE @Author");
            }

            if (!string.IsNullOrWhiteSpace(searchDto.ISBN) && searchDto.ISBN.Trim().Length >= 3)
            {
                isbn = $"%{searchDto.ISBN.Trim()}%";
                conditions.Add("ISBN LIKE @ISBN");
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Genre))
            {
                genre = $"%{searchDto.Genre.Trim()}%";
                conditions.Add("Genre LIKE @Genre");
            }

            if (searchDto.AvailableOnly)
                conditions.Add("AvailableCopies > 0");

            if (searchDto.IsAvailable.HasValue)
                conditions.Add(searchDto.IsAvailable.Value ? "AvailableCopies > 0" : "AvailableCopies = 0");

            // إذا لم تكن هناك شروط، إرجاع جميع الكتب (السلوك الافتراضي)
            // If no conditions, return all books (default behavior)
            var whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";

            // إنشاء كائن مجهول بالمعاملات المطلوبة فقط
            // Create anonymous object with only required parameters
            var parameters = new
            {
                SearchTerm = searchTerm,
                Title = title,
                Author = author,
                ISBN = isbn,
                Genre = genre
            };

            // تسجيل تفاصيل إضافية للتشخيص
            // Additional logging for debugging
            Console.WriteLine($"DEBUG: WHERE clause: '{whereClause}'");
            Console.WriteLine($"DEBUG: Parameters - SearchTerm: {searchTerm}, Title: {title}, Author: {author}, ISBN: {isbn}, Genre: {genre}");

            return (whereClause, parameters);
        }

        /// <summary>
        /// بناء جملة ORDER BY
        /// Build ORDER BY clause
        /// </summary>
        private static string BuildOrderClause(string sortBy, bool sortDescending)
        {
            var validSortColumns = new[] { "Title", "Author", "PublicationYear", "Genre", "CreatedDate" };
            var column = validSortColumns.Contains(sortBy) ? sortBy : "Title";
            var direction = sortDescending ? "DESC" : "ASC";
            return $"ORDER BY {column} {direction}";
        }

        /// <summary>
        /// الحصول على الكتب حسب المؤلف
        /// Get books by author
        /// </summary>
        public async Task<IEnumerable<Book>> GetByAuthorAsync(string author)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(author))
                    return new List<Book>();

                var cacheKey = CacheKeys.Books.ByAuthor(author);

                // محاولة الحصول على البيانات من التخزين المؤقت
                var cachedBooks = await _cacheService.GetAsync<List<Book>>(cacheKey);
                if (cachedBooks != null)
                {
                    return cachedBooks;
                }

                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    SELECT BookId, Title, Author, ISBN, Publisher, PublicationYear,
                           Genre, TotalCopies, AvailableCopies, Description,
                           CreatedDate, ModifiedDate
                    FROM Books
                    WHERE Author LIKE @Author
                    ORDER BY Title";

                var books = new List<Book>();
                using var reader = await DatabaseHelper.ExecuteReaderAsync(connection, sql, new { Author = $"%{author}%" });

                while (reader.Read())
                {
                    books.Add(MapReaderToBook(reader));
                }

                await _cacheService.SetAsync(cacheKey, books, _cacheExpiration);
                return books;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على كتب المؤلف {Author} - Error getting books by author {Author}", author, author);
                throw;
            }
        }

        /// <summary>
        /// تحديث بيانات كتاب
        /// Update book data
        /// </summary>
        public async Task<bool> UpdateAsync(Book book)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    UPDATE Books
                    SET Title = @Title, Author = @Author, ISBN = @ISBN,
                        Publisher = @Publisher, PublicationYear = @PublicationYear,
                        Genre = @Genre, TotalCopies = @TotalCopies,
                        AvailableCopies = @AvailableCopies, Description = @Description
                    WHERE BookId = @BookId";

                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(connection, sql, new
                {
                    book.BookId,
                    book.Title,
                    book.Author,
                    book.ISBN,
                    book.Publisher,
                    book.PublicationYear,
                    book.Genre,
                    book.TotalCopies,
                    book.AvailableCopies,
                    book.Description
                });

                if (rowsAffected > 0)
                {
                    await InvalidateBookCacheAsync(book.BookId);
                    _logger.LogInformation("تم تحديث الكتاب {BookId}: {Title} - Updated book {BookId}: {Title}",
                        book.BookId, book.Title, book.BookId, book.Title);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث الكتاب {BookId} - Error updating book {BookId}", book.BookId, book.BookId);
                throw;
            }
        }

        /// <summary>
        /// حذف كتاب
        /// Delete a book
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = "DELETE FROM Books WHERE BookId = @BookId";
                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(connection, sql, new { BookId = id });

                if (rowsAffected > 0)
                {
                    await InvalidateBookCacheAsync(id);
                    _logger.LogInformation("تم حذف الكتاب {BookId} - Deleted book {BookId}", id, id);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الكتاب {BookId} - Error deleting book {BookId}", id, id);
                throw;
            }
        }

        /// <summary>
        /// تحديث عدد النسخ المتاحة
        /// Update available copies count
        /// </summary>
        public async Task<bool> UpdateAvailableCopiesAsync(int bookId, int change)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = @"
                    UPDATE Books
                    SET AvailableCopies = AvailableCopies + @Change
                    WHERE BookId = @BookId AND (AvailableCopies + @Change) >= 0";

                var rowsAffected = await DatabaseHelper.ExecuteNonQueryAsync(connection, sql, new { BookId = bookId, Change = change });

                if (rowsAffected > 0)
                {
                    await InvalidateBookCacheAsync(bookId);
                    _logger.LogDebug("تم تحديث النسخ المتاحة للكتاب {BookId} بالتغيير {Change} - Updated available copies for book {BookId} with change {Change}",
                        bookId, change, bookId, change);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث النسخ المتاحة للكتاب {BookId} - Error updating available copies for book {BookId}", bookId, bookId);
                throw;
            }
        }

        /// <summary>
        /// التحقق من توفر كتاب للاستعارة
        /// Check if book is available for borrowing
        /// </summary>
        public async Task<bool> IsAvailableAsync(int bookId)
        {
            try
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();

                const string sql = "SELECT AvailableCopies FROM Books WHERE BookId = @BookId";
                var availableCopies = await DatabaseHelper.ExecuteScalarAsync<int?>(connection, sql, new { BookId = bookId });

                return availableCopies.HasValue && availableCopies.Value > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من توفر الكتاب {BookId} - Error checking book availability {BookId}", bookId, bookId);
                throw;
            }
        }


    }
}
