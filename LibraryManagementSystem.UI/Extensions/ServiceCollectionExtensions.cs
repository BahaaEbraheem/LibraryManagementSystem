using LibraryManagementSystem.DAL.Data;
using LibraryManagementSystem.UI.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LibraryManagementSystem.UI.Extensions
{
    /// <summary>
    /// امتدادات مجموعة الخدمات
    /// Service collection extensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// إضافة معالجة الأخطاء العامة
        /// Add global error handling
        /// </summary>
        public static IServiceCollection AddGlobalErrorHandling(this IServiceCollection services)
        {
            // إضافة خدمات معالجة الأخطاء
            // Add error handling services
            // Note: Middleware is not registered as a service, it's added to the pipeline

            return services;
        }

        /// <summary>
        /// إضافة معالجة أخطاء قاعدة البيانات
        /// Add database error handling
        /// </summary>
        public static IServiceCollection AddDatabaseErrorHandling(this IServiceCollection services, string connectionString)
        {
            // إضافة خدمات مراقبة قاعدة البيانات
            // Add database monitoring services
            services.AddHealthChecks()
                .AddCheck<DatabaseConnectionHealthCheck>("database-connection");

            // إضافة مدير اتصالات قاعدة البيانات
            // Add database connection manager
            services.AddSingleton(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<LibraryManagementSystem.DAL.Data.DatabaseConnectionManager>>();
                return new LibraryManagementSystem.DAL.Data.DatabaseConnectionManager(connectionString, logger);
            });

            return services;
        }
    }
}


