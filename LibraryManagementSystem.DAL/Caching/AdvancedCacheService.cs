using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LibraryManagementSystem.DAL.Caching
{
    /// <summary>
    /// خدمة التخزين المؤقت المتقدمة مع إدارة الإبطال والسياسات المختلفة
    /// Advanced caching service with invalidation management and different policies
    /// </summary>
    public class AdvancedCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AdvancedCacheService> _logger;
        private readonly ConcurrentDictionary<string, CacheMetadata> _cacheMetadata;
        private readonly ConcurrentDictionary<string, HashSet<string>> _taggedKeys;
        private readonly Timer _cleanupTimer;

        /// <summary>
        /// منشئ خدمة التخزين المؤقت المتقدمة
        /// Advanced cache service constructor
        /// </summary>
        public AdvancedCacheService(IMemoryCache memoryCache, ILogger<AdvancedCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheMetadata = new ConcurrentDictionary<string, CacheMetadata>();
            _taggedKeys = new ConcurrentDictionary<string, HashSet<string>>();

            // تنظيف دوري كل 5 دقائق
            // Periodic cleanup every 5 minutes
            _cleanupTimer = new Timer(PerformCleanup, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// الحصول على قيمة من التخزين المؤقت
        /// Get value from cache
        /// </summary>
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return default;

                if (_memoryCache.TryGetValue(key, out var cachedValue))
                {
                    // تحديث إحصائيات الوصول
                    // Update access statistics
                    if (_cacheMetadata.TryGetValue(key, out var metadata))
                    {
                        metadata.AccessCount++;
                        metadata.LastAccessTime = DateTime.UtcNow;
                    }

                    _logger.LogDebug("تم العثور على القيمة في التخزين المؤقت للمفتاح: {Key} - Cache hit for key: {Key}", key);
                    return (T)cachedValue;
                }

                _logger.LogDebug("لم يتم العثور على القيمة في التخزين المؤقت للمفتاح: {Key} - Cache miss for key: {Key}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على القيمة من التخزين المؤقت للمفتاح: {Key} - Error getting value from cache for key: {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// تعيين قيمة في التخزين المؤقت
        /// Set value in cache
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            await SetAsync(key, value, new CacheOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
                Priority = CacheItemPriority.Normal
            });
        }

        /// <summary>
        /// تعيين قيمة في التخزين المؤقت مع خيارات متقدمة
        /// Set value in cache with advanced options
        /// </summary>
        public async Task SetAsync<T>(string key, T value, CacheOptions options)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key) || value == null)
                    return;

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    Priority = options.Priority,
                    Size = options.Size
                };

                // تعيين انتهاء الصلاحية
                // Set expiration
                if (options.AbsoluteExpiration.HasValue)
                    cacheEntryOptions.AbsoluteExpiration = options.AbsoluteExpiration;
                else if (options.AbsoluteExpirationRelativeToNow.HasValue)
                    cacheEntryOptions.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;

                if (options.SlidingExpiration.HasValue)
                    cacheEntryOptions.SlidingExpiration = options.SlidingExpiration;

                // إضافة callback للإزالة
                // Add removal callback
                cacheEntryOptions.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
                {
                    OnCacheItemRemoved(evictedKey.ToString()!, reason);
                });

                _memoryCache.Set(key, value, cacheEntryOptions);

                // تسجيل البيانات الوصفية
                // Record metadata
                var metadata = new CacheMetadata
                {
                    Key = key,
                    CreatedTime = DateTime.UtcNow,
                    LastAccessTime = DateTime.UtcNow,
                    ExpirationTime = options.AbsoluteExpiration?.DateTime ??
                                   (options.AbsoluteExpirationRelativeToNow.HasValue ?
                                    DateTime.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value) : null),
                    Tags = options.Tags?.ToHashSet() ?? new HashSet<string>(),
                    AccessCount = 0,
                    Size = options.Size ?? 1
                };

                _cacheMetadata.TryAdd(key, metadata);

                // إضافة العلامات
                // Add tags
                if (options.Tags != null)
                {
                    foreach (var tag in options.Tags)
                    {
                        _taggedKeys.AddOrUpdate(tag,
                            new HashSet<string> { key },
                            (_, existingKeys) =>
                            {
                                existingKeys.Add(key);
                                return existingKeys;
                            });
                    }
                }

                _logger.LogDebug("تم تعيين القيمة في التخزين المؤقت للمفتاح: {Key} مع انتهاء الصلاحية: {Expiration} - Set cache value for key: {Key} with expiration: {Expiration}",
                    key, metadata.ExpirationTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تعيين القيمة في التخزين المؤقت للمفتاح: {Key} - Error setting cache value for key: {Key}", key);
            }
        }

        /// <summary>
        /// إزالة قيمة من التخزين المؤقت
        /// Remove value from cache
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return;

                _memoryCache.Remove(key);
                _cacheMetadata.TryRemove(key, out _);

                // إزالة من العلامات
                // Remove from tags
                foreach (var taggedKeysPair in _taggedKeys)
                {
                    taggedKeysPair.Value.Remove(key);
                    if (!taggedKeysPair.Value.Any())
                    {
                        _taggedKeys.TryRemove(taggedKeysPair.Key, out _);
                    }
                }

                _logger.LogDebug("تم إزالة القيمة من التخزين المؤقت للمفتاح: {Key} - Removed cache value for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إزالة القيمة من التخزين المؤقت للمفتاح: {Key} - Error removing cache value for key: {Key}", key);
            }
        }

        /// <summary>
        /// إزالة جميع القيم المرتبطة بعلامة
        /// Remove all values associated with a tag
        /// </summary>
        public async Task RemoveByTagAsync(string tag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tag))
                    return;

                if (_taggedKeys.TryGetValue(tag, out var keys))
                {
                    var keysList = keys.ToList();
                    foreach (var key in keysList)
                    {
                        await RemoveAsync(key);
                    }

                    _logger.LogInformation("تم إزالة {Count} عنصر من التخزين المؤقت للعلامة: {Tag} - Removed {Count} items from cache for tag: {Tag}",
                        keysList.Count, tag);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إزالة القيم من التخزين المؤقت للعلامة: {Tag} - Error removing cache values for tag: {Tag}", tag);
            }
        }

        /// <summary>
        /// مسح جميع القيم من التخزين المؤقت
        /// Clear all values from cache
        /// </summary>
        public async Task ClearAsync()
        {
            try
            {
                // لا يوجد طريقة مباشرة لمسح IMemoryCache، لذا نزيل كل مفتاح
                // No direct way to clear IMemoryCache, so remove each key
                var keys = _cacheMetadata.Keys.ToList();
                foreach (var key in keys)
                {
                    await RemoveAsync(key);
                }

                _logger.LogInformation("تم مسح جميع القيم من التخزين المؤقت - Cleared all cache values");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في مسح التخزين المؤقت - Error clearing cache");
            }
        }

        /// <summary>
        /// مسح جميع البيانات من التخزين المؤقت (تنفيذ واجهة ICacheService)
        /// Clear all data from cache (ICacheService interface implementation)
        /// </summary>
        public async Task ClearAllAsync()
        {
            await ClearAsync();
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
                    return;

                var keys = _cacheMetadata.Keys.Where(k => k.Contains(pattern)).ToList();
                foreach (var key in keys)
                {
                    await RemoveAsync(key);
                }

                _logger.LogDebug("تم إزالة {Count} عنصر من التخزين المؤقت للنمط: {Pattern} - Removed {Count} items from cache for pattern: {Pattern}",
                    keys.Count, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إزالة القيم من التخزين المؤقت للنمط: {Pattern} - Error removing cache values for pattern: {Pattern}", pattern);
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
                    return false;

                return _memoryCache.TryGetValue(key, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من وجود المفتاح في التخزين المؤقت: {Key} - Error checking cache key existence: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// الحصول على إحصائيات التخزين المؤقت
        /// Get cache statistics
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            try
            {
                var totalItems = _cacheMetadata.Count;
                var totalSize = _cacheMetadata.Values.Sum(m => m.Size);
                var totalAccesses = _cacheMetadata.Values.Sum(m => m.AccessCount);
                var expiredItems = _cacheMetadata.Values.Count(m =>
                    m.ExpirationTime.HasValue && m.ExpirationTime.Value < DateTime.UtcNow);

                return new CacheStatistics
                {
                    TotalItems = totalItems,
                    TotalSize = totalSize,
                    TotalAccesses = totalAccesses,
                    ExpiredItems = expiredItems,
                    Tags = _taggedKeys.Keys.ToList(),
                    OldestItem = _cacheMetadata.Values.OrderBy(m => m.CreatedTime).FirstOrDefault()?.CreatedTime,
                    MostAccessedKey = _cacheMetadata.Values.OrderByDescending(m => m.AccessCount).FirstOrDefault()?.Key
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على إحصائيات التخزين المؤقت - Error getting cache statistics");
                return new CacheStatistics();
            }
        }

        /// <summary>
        /// معالج إزالة عنصر من التخزين المؤقت
        /// Cache item removal handler
        /// </summary>
        private void OnCacheItemRemoved(string key, EvictionReason reason)
        {
            _cacheMetadata.TryRemove(key, out _);

            _logger.LogDebug("تم إزالة عنصر من التخزين المؤقت: {Key}, السبب: {Reason} - Cache item removed: {Key}, Reason: {Reason}",
                key, reason);
        }

        /// <summary>
        /// تنظيف دوري للتخزين المؤقت
        /// Periodic cache cleanup
        /// </summary>
        private void PerformCleanup(object? state)
        {
            try
            {
                var expiredKeys = _cacheMetadata
                    .Where(kvp => kvp.Value.ExpirationTime.HasValue && kvp.Value.ExpirationTime.Value < DateTime.UtcNow)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _memoryCache.Remove(key);
                }

                if (expiredKeys.Any())
                {
                    _logger.LogDebug("تم تنظيف {Count} عنصر منتهي الصلاحية من التخزين المؤقت - Cleaned up {Count} expired items from cache",
                        expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تنظيف التخزين المؤقت - Error during cache cleanup");
            }
        }

        /// <summary>
        /// تحرير الموارد
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
    }

    /// <summary>
    /// خيارات التخزين المؤقت المتقدمة
    /// Advanced cache options
    /// </summary>
    public class CacheOptions
    {
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;
        public long? Size { get; set; }
        public IEnumerable<string>? Tags { get; set; }
    }

    /// <summary>
    /// البيانات الوصفية للتخزين المؤقت
    /// Cache metadata
    /// </summary>
    public class CacheMetadata
    {
        public string Key { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public HashSet<string> Tags { get; set; } = new();
        public long AccessCount { get; set; }
        public long Size { get; set; }
    }

    /// <summary>
    /// إحصائيات التخزين المؤقت
    /// Cache statistics
    /// </summary>
    public class CacheStatistics
    {
        public int TotalItems { get; set; }
        public long TotalSize { get; set; }
        public long TotalAccesses { get; set; }
        public int ExpiredItems { get; set; }
        public List<string> Tags { get; set; } = new();
        public DateTime? OldestItem { get; set; }
        public string? MostAccessedKey { get; set; }
    }
}
