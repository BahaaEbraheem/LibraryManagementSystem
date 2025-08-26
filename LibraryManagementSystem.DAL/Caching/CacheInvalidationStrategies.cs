using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryManagementSystem.DAL.Caching
{
    /// <summary>
    /// استراتيجيات إبطال التخزين المؤقت
    /// Cache invalidation strategies
    /// </summary>
    public class CacheInvalidationStrategies
    {
        private readonly AdvancedCacheService _cacheService;
        private readonly ILogger<CacheInvalidationStrategies> _logger;

        /// <summary>
        /// منشئ استراتيجيات الإبطال
        /// Invalidation strategies constructor
        /// </summary>
        public CacheInvalidationStrategies(AdvancedCacheService cacheService, ILogger<CacheInvalidationStrategies> logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// إبطال تخزين الكتب المؤقت عند التحديث
        /// Invalidate book cache on update
        /// </summary>
        public async Task InvalidateBookCacheAsync(int bookId)
        {
            try
            {
                var keysToInvalidate = new[]
                {
                    CacheKeys.Books.ById(bookId),
                    CacheKeys.Books.Available,
                    CacheKeys.Books.All
                };

                foreach (var key in keysToInvalidate)
                {
                    await _cacheService.RemoveAsync(key);
                }

                // إبطال البحث والإحصائيات
                // Invalidate search and statistics
                await _cacheService.RemoveByTagAsync("books");
                await _cacheService.RemoveByTagAsync("statistics");

                _logger.LogDebug("تم إبطال تخزين الكتاب {BookId} المؤقت - Invalidated cache for book {BookId}", bookId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إبطال تخزين الكتاب {BookId} المؤقت - Error invalidating cache for book {BookId}", bookId);
            }
        }

        /// <summary>
        /// إبطال تخزين المستخدمين المؤقت عند التحديث
        /// Invalidate user cache on update
        /// </summary>
        public async Task InvalidateUserCacheAsync(int userId, string? email = null)
        {
            try
            {
                var keysToInvalidate = new List<string>
                {
                    CacheKeys.Users.ById(userId),
                    CacheKeys.Users.All,
                    CacheKeys.Users.Active
                };

                if (!string.IsNullOrEmpty(email))
                {
                    keysToInvalidate.Add(CacheKeys.Users.ByEmail(email));
                }

                foreach (var key in keysToInvalidate)
                {
                    await _cacheService.RemoveAsync(key);
                }

                // إبطال الإحصائيات
                // Invalidate statistics
                await _cacheService.RemoveByTagAsync("users");
                await _cacheService.RemoveByTagAsync("statistics");

                _logger.LogDebug("تم إبطال تخزين المستخدم {UserId} المؤقت - Invalidated cache for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إبطال تخزين المستخدم {UserId} المؤقت - Error invalidating cache for user {UserId}", userId);
            }
        }

        /// <summary>
        /// إبطال تخزين الاستعارات المؤقت عند التحديث
        /// Invalidate borrowing cache on update
        /// </summary>
        public async Task InvalidateBorrowingCacheAsync(int borrowingId, int userId, int bookId)
        {
            try
            {
                var keysToInvalidate = new[]
                {
                    CacheKeys.Borrowings.ById(borrowingId),
                    CacheKeys.Borrowings.ByUser(userId),
                    CacheKeys.Borrowings.ByBook(bookId),
                    CacheKeys.Borrowings.Active,
                    CacheKeys.Borrowings.Overdue,
                    CacheKeys.Borrowings.All
                };

                foreach (var key in keysToInvalidate)
                {
                    await _cacheService.RemoveAsync(key);
                }

                // إبطال الإحصائيات والكتب المتاحة
                // Invalidate statistics and available books
                await _cacheService.RemoveByTagAsync("borrowings");
                await _cacheService.RemoveByTagAsync("statistics");
                await _cacheService.RemoveAsync(CacheKeys.Books.Available);

                _logger.LogDebug("تم إبطال تخزين الاستعارة {BorrowingId} المؤقت - Invalidated cache for borrowing {BorrowingId}", borrowingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إبطال تخزين الاستعارة {BorrowingId} المؤقت - Error invalidating cache for borrowing {BorrowingId}", borrowingId);
            }
        }

        /// <summary>
        /// إبطال تخزين البحث المؤقت
        /// Invalidate search cache
        /// </summary>
        public async Task InvalidateSearchCacheAsync()
        {
            try
            {
                await _cacheService.RemoveByTagAsync("search");
                _logger.LogDebug("تم إبطال تخزين البحث المؤقت - Invalidated search cache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إبطال تخزين البحث المؤقت - Error invalidating search cache");
            }
        }

        /// <summary>
        /// إبطال تخزين الإحصائيات المؤقت
        /// Invalidate statistics cache
        /// </summary>
        public async Task InvalidateStatisticsCacheAsync()
        {
            try
            {
                await _cacheService.RemoveByTagAsync("statistics");
                _logger.LogDebug("تم إبطال تخزين الإحصائيات المؤقت - Invalidated statistics cache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إبطال تخزين الإحصائيات المؤقت - Error invalidating statistics cache");
            }
        }

        /// <summary>
        /// إبطال جميع التخزين المؤقت
        /// Invalidate all cache
        /// </summary>
        public async Task InvalidateAllCacheAsync()
        {
            try
            {
                await _cacheService.ClearAsync();
                _logger.LogInformation("تم إبطال جميع التخزين المؤقت - Invalidated all cache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إبطال جميع التخزين المؤقت - Error invalidating all cache");
            }
        }
    }

    /// <summary>
    /// سياسات التخزين المؤقت للبيانات المختلفة
    /// Cache policies for different data types
    /// </summary>
    public static class CachePolicies
    {
        /// <summary>
        /// سياسة تخزين الكتب المؤقت
        /// Books cache policy
        /// </summary>
        public static CacheOptions Books => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10),
            Priority = CacheItemPriority.High,
            Tags = new[] { "books" }
        };

        /// <summary>
        /// سياسة تخزين المستخدمين المؤقت
        /// Users cache policy
        /// </summary>
        public static CacheOptions Users => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20),
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.Normal,
            Tags = new[] { "users" }
        };

        /// <summary>
        /// سياسة تخزين الاستعارات المؤقت
        /// Borrowings cache policy
        /// </summary>
        public static CacheOptions Borrowings => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
            SlidingExpiration = TimeSpan.FromMinutes(3),
            Priority = CacheItemPriority.Normal,
            Tags = new[] { "borrowings" }
        };

        /// <summary>
        /// سياسة تخزين البحث المؤقت
        /// Search cache policy
        /// </summary>
        public static CacheOptions Search => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            Priority = CacheItemPriority.Low,
            Tags = new[] { "search" }
        };

        /// <summary>
        /// سياسة تخزين الإحصائيات المؤقت
        /// Statistics cache policy
        /// </summary>
        public static CacheOptions Statistics => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
            SlidingExpiration = TimeSpan.FromMinutes(15),
            Priority = CacheItemPriority.High,
            Tags = new[] { "statistics" }
        };

        /// <summary>
        /// سياسة تخزين قصير المدى
        /// Short-term cache policy
        /// </summary>
        public static CacheOptions ShortTerm => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.Low
        };

        /// <summary>
        /// سياسة تخزين طويل المدى
        /// Long-term cache policy
        /// </summary>
        public static CacheOptions LongTerm => new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
            SlidingExpiration = TimeSpan.FromMinutes(30),
            Priority = CacheItemPriority.High
        };
    }
}
