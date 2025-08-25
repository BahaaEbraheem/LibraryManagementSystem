namespace LibraryManagementSystem.DAL.Caching
{
    /// <summary>
    /// واجهة خدمة التخزين المؤقت
    /// Interface for caching service
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// الحصول على قيمة من التخزين المؤقت
        /// Get a value from cache
        /// </summary>
        /// <typeparam name="T">نوع البيانات - Data type</typeparam>
        /// <param name="key">مفتاح التخزين المؤقت - Cache key</param>
        /// <returns>القيمة المخزنة أو null - Cached value or null</returns>
        Task<T?> GetAsync<T>(string key) where T : class;
        
        /// <summary>
        /// تخزين قيمة في التخزين المؤقت
        /// Store a value in cache
        /// </summary>
        /// <typeparam name="T">نوع البيانات - Data type</typeparam>
        /// <param name="key">مفتاح التخزين المؤقت - Cache key</param>
        /// <param name="value">القيمة المراد تخزينها - Value to cache</param>
        /// <param name="expiration">مدة انتهاء الصلاحية - Expiration duration</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        
        /// <summary>
        /// إزالة قيمة من التخزين المؤقت
        /// Remove a value from cache
        /// </summary>
        /// <param name="key">مفتاح التخزين المؤقت - Cache key</param>
        Task RemoveAsync(string key);
        
        /// <summary>
        /// إزالة عدة قيم من التخزين المؤقت بناءً على نمط
        /// Remove multiple values from cache based on pattern
        /// </summary>
        /// <param name="pattern">نمط المفاتيح - Key pattern</param>
        Task RemoveByPatternAsync(string pattern);
        
        /// <summary>
        /// التحقق من وجود مفتاح في التخزين المؤقت
        /// Check if a key exists in cache
        /// </summary>
        /// <param name="key">مفتاح التخزين المؤقت - Cache key</param>
        Task<bool> ExistsAsync(string key);
        
        /// <summary>
        /// مسح جميع البيانات من التخزين المؤقت
        /// Clear all data from cache
        /// </summary>
        Task ClearAllAsync();
    }
}
