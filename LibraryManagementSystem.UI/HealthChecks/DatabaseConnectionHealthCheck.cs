using Microsoft.Extensions.Diagnostics.HealthChecks;
using LibraryManagementSystem.DAL.Data;

namespace LibraryManagementSystem.UI.HealthChecks
{
    /// <summary>
    /// فحص صحة اتصال قاعدة البيانات
    /// Database connection health check
    /// </summary>
    public class DatabaseConnectionHealthCheck : IHealthCheck
    {
        private readonly DatabaseConnectionManager _connectionManager;
        private readonly ILogger<DatabaseConnectionHealthCheck> _logger;

        /// <summary>
        /// منشئ فحص الصحة
        /// Health check constructor
        /// </summary>
        public DatabaseConnectionHealthCheck(
            DatabaseConnectionManager connectionManager,
            ILogger<DatabaseConnectionHealthCheck> logger)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// فحص صحة قاعدة البيانات
        /// Check database health
        /// </summary>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("بدء فحص صحة اتصال قاعدة البيانات - Starting database connection health check");

                var healthInfo = await _connectionManager.GetDatabaseHealthAsync();

                if (healthInfo.IsHealthy)
                {
                    var data = new Dictionary<string, object>
                    {
                        ["responseTime"] = $"{healthInfo.ResponseTimeMs}ms",
                        ["serverVersion"] = healthInfo.ServerVersion ?? "Unknown",
                        ["database"] = healthInfo.Database ?? "Unknown",
                        ["checkTime"] = healthInfo.CheckTime.ToString("yyyy-MM-dd HH:mm:ss UTC")
                    };

                    _logger.LogInformation("فحص صحة قاعدة البيانات نجح: زمن الاستجابة={ResponseTime}ms - Database health check succeeded: ResponseTime={ResponseTime}ms",
                        healthInfo.ResponseTimeMs);

                    // تحديد حالة الصحة بناءً على زمن الاستجابة
                    // Determine health status based on response time
                    if (healthInfo.ResponseTimeMs < 100)
                    {
                        return HealthCheckResult.Healthy("قاعدة البيانات تعمل بشكل ممتاز - Database is performing excellently", data);
                    }
                    else if (healthInfo.ResponseTimeMs < 500)
                    {
                        return HealthCheckResult.Healthy("قاعدة البيانات تعمل بشكل جيد - Database is performing well", data);
                    }
                    else if (healthInfo.ResponseTimeMs < 1000)
                    {
                        return HealthCheckResult.Degraded("قاعدة البيانات تعمل ببطء - Database is performing slowly", null, data);
                    }
                    else
                    {
                        return HealthCheckResult.Degraded("قاعدة البيانات تعمل ببطء شديد - Database is performing very slowly", null, data);
                    }
                }
                else
                {
                    var data = new Dictionary<string, object>
                    {
                        ["error"] = healthInfo.ErrorMessage ?? "Unknown error",
                        ["checkTime"] = healthInfo.CheckTime.ToString("yyyy-MM-dd HH:mm:ss UTC")
                    };

                    _logger.LogError("فحص صحة قاعدة البيانات فشل: {Error} - Database health check failed: {Error}",
                        healthInfo.ErrorMessage);

                    return HealthCheckResult.Unhealthy("قاعدة البيانات غير متاحة - Database is unavailable", data: data);
                }
            }
            catch (DatabaseConnectionException ex)
            {
                _logger.LogError(ex, "فشل في اتصال قاعدة البيانات أثناء فحص الصحة - Database connection failed during health check");

                var data = new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["errorType"] = "DatabaseConnectionException",
                    ["checkTime"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                };

                return HealthCheckResult.Unhealthy("فشل في اتصال قاعدة البيانات - Database connection failed", ex, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ غير متوقع أثناء فحص صحة قاعدة البيانات - Unexpected error during database health check");

                var data = new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["errorType"] = ex.GetType().Name,
                    ["checkTime"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                };

                return HealthCheckResult.Unhealthy("خطأ غير متوقع في فحص صحة قاعدة البيانات - Unexpected error in database health check", ex, data);
            }
        }
    }
}
