using LibraryManagementSystem.DAL.Data;
using LibraryManagementSystem.UI.HealthChecks;
using LibraryManagementSystem.UI.Middleware;
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

namespace LibraryManagementSystem.UI.Extensions
{
    /// <summary>
    /// امتدادات تطبيق الويب
    /// Web application extensions
    /// </summary>
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// استخدام معالجة الأخطاء العامة
        /// Use global error handling
        /// </summary>
        public static WebApplication UseGlobalErrorHandling(this WebApplication app)
        {
            // إضافة وسطاء معالجة الأخطاء العامة
            // Add global error handling middleware
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            return app;
        }

        /// <summary>
        /// استخدام مراقبة الصحة
        /// Use health monitoring
        /// </summary>
        public static WebApplication UseHealthMonitoring(this WebApplication app)
        {
            // إضافة نقاط فحص الصحة
            // Add health check endpoints
            app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(x => new
                        {
                            name = x.Key,
                            status = x.Value.Status.ToString(),
                            exception = x.Value.Exception?.Message,
                            duration = x.Value.Duration.ToString()
                        }),
                        duration = report.TotalDuration.ToString()
                    };
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
                }
            });

            return app;
        }
    }
}
