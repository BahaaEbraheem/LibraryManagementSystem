using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace LibraryManagementSystem.DAL.Caching
{
    /// <summary>
    /// تنفيذ خدمة التخزين المؤقت باستخدام الذاكرة
    /// Memory cache service implementation
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly ConcurrentDictionary<string, bool> _cacheKeys;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

        /// <summary>
        /// منشئ خدمة التخزين المؤقت
        /// Cache service constructor
        /// </summary>
        /// <param name="memoryCache">خدمة التخزين المؤقت في الذاكرة</param>
        /// <param name="logger">خدمة التسجيل</param>
        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheKeys = new ConcurrentDictionary<string, bool>();
        }

        /// <summary>
        /// الحصول على قيمة من التخزين المؤقت
        /// Get a value from cache
        /// </summary>
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogWarning("محاولة الحصول على قيمة بمفتاح فارغ - Attempted to get cache value with empty key");
                    return null;
                }

                if (_memoryCache.TryGetValue(key, out var cachedValue))
                {
                    _logger.LogDebug("تم العثور على القيمة في التخزين المؤقت للمفتاح: {Key} - Cache hit for key: {Key}", key, key);

                    if (cachedValue is string jsonString)
                    {
                        return JsonSerializer.Deserialize<T>(jsonString);
                    }

                    return cachedValue as T;
                }

                _logger.LogDebug("لم يتم العثور على القيمة في التخزين المؤقت للمفتاح: {Key} - Cache miss for key: {Key}", key, key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على القيمة من التخزين المؤقت للمفتاح: {Key} - Error getting cache value for key: {Key}", key, key);
                return null;
            }
        }

        /// <summary>
        /// تخزين قيمة في التخزين المؤقت
        /// Store a value in cache
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogWarning("محاولة تخزين قيمة بمفتاح فارغ - Attempted to set cache value with empty key");
                    return;
                }

                if (value == null)
                {
                    _logger.LogWarning("محاولة تخزين قيمة فارغة للمفتاح: {Key} - Attempted to set null value for key: {Key}", key, key);
                    return;
                }

                var cacheExpiration = expiration ?? _defaultExpiration;
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiration,
                    SlidingExpiration = TimeSpan.FromMinutes(5), // تجديد تلقائي عند الوصول - Auto-refresh on access
                    Priority = CacheItemPriority.Normal
                };

                // تسجيل المفتاح لتتبع جميع المفاتيح المخزنة - Register key for tracking all stored keys
                cacheOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _cacheKeys.TryRemove(key.ToString()!, out _);
                    _logger.LogDebug("تم إزالة المفتاح من التخزين المؤقت: {Key}, السبب: {Reason} - Cache key removed: {Key}, Reason: {Reason}",
                        key, reason, key, reason);
                });

                // تحويل الكائن إلى JSON للتخزين - Convert object to JSON for storage
                var jsonString = JsonSerializer.Serialize(value);
                _memoryCache.Set(key, jsonString, cacheOptions);
                _cacheKeys.TryAdd(key, true);

                _logger.LogDebug("تم تخزين القيمة في التخزين المؤقت للمفتاح: {Key}, انتهاء الصلاحية: {Expiration} - Value cached for key: {Key}, Expiration: {Expiration}",
                    key, cacheExpiration, key, cacheExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تخزين القيمة في التخزين المؤقت للمفتاح: {Key} - Error setting cache value for key: {Key}", key, key);
            }
        }

        /// <summary>
        /// إزالة قيمة من التخزين المؤقت
        /// Remove a value from cache
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogWarning("محاولة إزالة قيمة بمفتاح فارغ - Attempted to remove cache value with empty key");
                    return;
                }

                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);

                _logger.LogDebug("تم إزالة المفتاح من التخزين المؤقت: {Key} - Cache key removed: {Key}", key, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إزالة القيمة من التخزين المؤقت للمفتاح: {Key} - Error removing cache value for key: {Key}", key, key);
            }
        }

        /// <summary>
        /// إزالة عدة قيم من التخزين المؤقت بناءً على نمط
        /// Remove multiple values from cache based on pattern
        /// </summary>
        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    _logger.LogWarning("محاولة إزالة قيم بنمط فارغ - Attempted to remove cache values with empty pattern");
                    return;
                }

                var keysToRemove = _cacheKeys.Keys
                    .Where(key => key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    await RemoveAsync(key);
                }

                _logger.LogDebug("تم إزالة {Count} مفتاح بالنمط: {Pattern} - Removed {Count} keys with pattern: {Pattern}",
                    keysToRemove.Count, pattern, keysToRemove.Count, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إزالة القيم بالنمط: {Pattern} - Error removing cache values with pattern: {Pattern}", pattern, pattern);
            }
        }

        /// <summary>
        /// التحقق من وجود مفتاح في التخزين المؤقت
        /// Check if a key exists in cache
        /// </summary>
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return false;
                }

                return _cacheKeys.ContainsKey(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من وجود المفتاح: {Key} - Error checking if cache key exists: {Key}", key, key);
                return false;
            }
        }

        /// <summary>
        /// مسح جميع البيانات من التخزين المؤقت
        /// Clear all data from cache
        /// </summary>
        public async Task ClearAllAsync()
        {
            try
            {
                var keysToRemove = _cacheKeys.Keys.ToList();

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                }

                _cacheKeys.Clear();

                _logger.LogInformation("تم مسح جميع البيانات من التخزين المؤقت ({Count} مفتاح) - Cleared all cache data ({Count} keys)",
                    keysToRemove.Count, keysToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في مسح جميع البيانات من التخزين المؤقت - Error clearing all cache data");
            }
        }
    }
}
