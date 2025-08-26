using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace LibraryManagementSystem.DAL.Data
{
    /// <summary>
    /// مدير اتصالات قاعدة البيانات مع تسجيل شامل للأخطاء وآليات الاستعادة
    /// Database connection manager with comprehensive error logging and recovery mechanisms
    /// </summary>
    public class DatabaseConnectionManager
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseConnectionManager> _logger;
        private readonly int _maxRetryAttempts;
        private readonly TimeSpan _retryDelay;

        /// <summary>
        /// منشئ مدير الاتصالات
        /// Connection manager constructor
        /// </summary>
        public DatabaseConnectionManager(
            string connectionString,
            ILogger<DatabaseConnectionManager> logger,
            int maxRetryAttempts = 3,
            TimeSpan? retryDelay = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maxRetryAttempts = maxRetryAttempts;
            _retryDelay = retryDelay ?? TimeSpan.FromSeconds(2);
        }

        /// <summary>
        /// إنشاء اتصال مع إعادة المحاولة والتسجيل الشامل
        /// Create connection with retry and comprehensive logging
        /// </summary>
        public async Task<IDbConnection> CreateConnectionWithRetryAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            Exception? lastException = null;

            for (int attempt = 1; attempt <= _maxRetryAttempts; attempt++)
            {
                try
                {
                    _logger.LogDebug("محاولة الاتصال بقاعدة البيانات - المحاولة {Attempt}/{MaxAttempts} - Database connection attempt {Attempt}/{MaxAttempts}",
                        attempt, _maxRetryAttempts);

                    var connection = new SqlConnection(_connectionString);

                    // تسجيل معلومات الاتصال
                    // Log connection information
                    LogConnectionInfo(connection, attempt);

                    await connection.OpenAsync();

                    stopwatch.Stop();
                    _logger.LogInformation("تم الاتصال بقاعدة البيانات بنجاح في المحاولة {Attempt} خلال {ElapsedMs}ms - Database connected successfully on attempt {Attempt} in {ElapsedMs}ms",
                        attempt, stopwatch.ElapsedMilliseconds);

                    // تسجيل إحصائيات الاتصال
                    // Log connection statistics
                    LogConnectionStatistics(connection);

                    return connection;
                }
                catch (SqlException sqlEx)
                {
                    lastException = sqlEx;
                    LogSqlException(sqlEx, attempt);

                    // تحديد ما إذا كان يجب إعادة المحاولة
                    // Determine if retry should be attempted
                    if (!ShouldRetry(sqlEx) || attempt == _maxRetryAttempts)
                    {
                        break;
                    }

                    await Task.Delay(_retryDelay);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogError(ex, "خطأ غير متوقع في الاتصال بقاعدة البيانات - المحاولة {Attempt} - Unexpected database connection error - Attempt {Attempt}",
                        attempt);

                    if (attempt == _maxRetryAttempts)
                    {
                        break;
                    }

                    await Task.Delay(_retryDelay);
                }
            }

            stopwatch.Stop();
            _logger.LogCritical(lastException, "فشل في الاتصال بقاعدة البيانات بعد {MaxAttempts} محاولات خلال {ElapsedMs}ms - Failed to connect to database after {MaxAttempts} attempts in {ElapsedMs}ms",
                _maxRetryAttempts, stopwatch.ElapsedMilliseconds);

            throw new DatabaseConnectionException(
                $"فشل في الاتصال بقاعدة البيانات بعد {_maxRetryAttempts} محاولات - Failed to connect to database after {_maxRetryAttempts} attempts",
                lastException);
        }

        /// <summary>
        /// تسجيل معلومات الاتصال
        /// Log connection information
        /// </summary>
        private void LogConnectionInfo(SqlConnection connection, int attempt)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connection.ConnectionString);

                _logger.LogDebug("معلومات الاتصال - المحاولة {Attempt}: الخادم={Server}, قاعدة البيانات={Database}, مهلة الاتصال={Timeout}s - Connection info - Attempt {Attempt}: Server={Server}, Database={Database}, Timeout={Timeout}s",
                    attempt, builder.DataSource, builder.InitialCatalog, builder.ConnectTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل في تسجيل معلومات الاتصال - Failed to log connection information");
            }
        }

        /// <summary>
        /// تسجيل إحصائيات الاتصال
        /// Log connection statistics
        /// </summary>
        private void LogConnectionStatistics(SqlConnection connection)
        {
            try
            {
                _logger.LogDebug("إحصائيات الاتصال: الحالة={State}, إصدار الخادم={ServerVersion}, قاعدة البيانات={Database} - Connection statistics: State={State}, ServerVersion={ServerVersion}, Database={Database}",
                    connection.State, connection.ServerVersion, connection.Database);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل في تسجيل إحصائيات الاتصال - Failed to log connection statistics");
            }
        }

        /// <summary>
        /// تسجيل استثناءات SQL
        /// Log SQL exceptions
        /// </summary>
        private void LogSqlException(SqlException sqlEx, int attempt)
        {
            var errorDetails = new
            {
                ErrorNumber = sqlEx.Number,
                Severity = sqlEx.Class,
                State = sqlEx.State,
                Procedure = sqlEx.Procedure,
                LineNumber = sqlEx.LineNumber,
                Server = sqlEx.Server,
                Message = sqlEx.Message,
                Attempt = attempt
            };

            switch (sqlEx.Number)
            {
                case 2: // Timeout
                case -2:
                    _logger.LogWarning("انتهت المهلة الزمنية للاتصال بقاعدة البيانات - المحاولة {Attempt}: {ErrorDetails} - Database connection timeout - Attempt {Attempt}: {ErrorDetails}",
                        attempt, errorDetails);
                    break;

                case 18456: // Login failed
                    _logger.LogError("فشل في تسجيل الدخول لقاعدة البيانات - المحاولة {Attempt}: {ErrorDetails} - Database login failed - Attempt {Attempt}: {ErrorDetails}",
                        attempt, errorDetails);
                    break;

                case 53: // Network path not found
                case 233: // Connection forcibly closed
                case 10054: // Connection reset by peer
                    _logger.LogWarning("خطأ في الشبكة أثناء الاتصال بقاعدة البيانات - المحاولة {Attempt}: {ErrorDetails} - Network error during database connection - Attempt {Attempt}: {ErrorDetails}",
                        attempt, errorDetails);
                    break;

                case 4060: // Invalid database name
                    _logger.LogError("اسم قاعدة البيانات غير صحيح - المحاولة {Attempt}: {ErrorDetails} - Invalid database name - Attempt {Attempt}: {ErrorDetails}",
                        attempt, errorDetails);
                    break;

                case 18452: // Login timeout
                    _logger.LogWarning("انتهت مهلة تسجيل الدخول - المحاولة {Attempt}: {ErrorDetails} - Login timeout - Attempt {Attempt}: {ErrorDetails}",
                        attempt, errorDetails);
                    break;

                default:
                    _logger.LogError(sqlEx, "خطأ SQL غير متوقع - المحاولة {Attempt}: رقم الخطأ={ErrorNumber}, الشدة={Severity}, الحالة={State} - Unexpected SQL error - Attempt {Attempt}: ErrorNumber={ErrorNumber}, Severity={Severity}, State={State}",
                        attempt, sqlEx.Number, sqlEx.Class, sqlEx.State);
                    break;
            }
        }

        /// <summary>
        /// تحديد ما إذا كان يجب إعادة المحاولة
        /// Determine if retry should be attempted
        /// </summary>
        private bool ShouldRetry(SqlException sqlEx)
        {
            // أخطاء قابلة للإعادة
            // Retryable errors
            var retryableErrors = new[]
            {
                2,      // Timeout
                -2,     // Timeout
                53,     // Network path not found
                233,    // Connection forcibly closed
                10054,  // Connection reset by peer
                10053,  // Connection aborted
                18452,  // Login timeout
                20,     // Instance not found
                64,     // Connection failed
                258,    // Wait timeout
                1222,   // Lock request timeout
                1205    // Deadlock
            };

            bool shouldRetry = retryableErrors.Contains(sqlEx.Number);

            _logger.LogDebug("تحديد إعادة المحاولة لخطأ SQL {ErrorNumber}: {ShouldRetry} - Determining retry for SQL error {ErrorNumber}: {ShouldRetry}",
                sqlEx.Number, shouldRetry);

            return shouldRetry;
        }

        /// <summary>
        /// اختبار الاتصال بقاعدة البيانات
        /// Test database connection
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = await CreateConnectionWithRetryAsync();

                // تنفيذ استعلام بسيط للتأكد من عمل الاتصال
                // Execute simple query to verify connection works
                using var command = new SqlCommand("SELECT 1", (SqlConnection)connection);
                var result = await command.ExecuteScalarAsync();

                _logger.LogInformation("اختبار الاتصال بقاعدة البيانات نجح - Database connection test succeeded");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل اختبار الاتصال بقاعدة البيانات - Database connection test failed");
                return false;
            }
        }

        /// <summary>
        /// الحصول على معلومات حالة قاعدة البيانات
        /// Get database health information
        /// </summary>
        public async Task<DatabaseHealthInfo> GetDatabaseHealthAsync()
        {
            var healthInfo = new DatabaseHealthInfo();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var connection = await CreateConnectionWithRetryAsync();

                // قياس زمن الاستجابة
                // Measure response time
                using var command = new SqlCommand("SELECT GETDATE()", (SqlConnection)connection);
                await command.ExecuteScalarAsync();

                stopwatch.Stop();

                healthInfo.IsHealthy = true;
                healthInfo.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                healthInfo.ServerVersion = ((SqlConnection)connection).ServerVersion;
                healthInfo.Database = connection.Database;
                healthInfo.CheckTime = DateTime.UtcNow;

                _logger.LogInformation("فحص صحة قاعدة البيانات نجح: زمن الاستجابة={ResponseTime}ms - Database health check succeeded: ResponseTime={ResponseTime}ms",
                    healthInfo.ResponseTimeMs);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                healthInfo.IsHealthy = false;
                healthInfo.ErrorMessage = ex.Message;
                healthInfo.CheckTime = DateTime.UtcNow;

                _logger.LogError(ex, "فشل فحص صحة قاعدة البيانات - Database health check failed");
            }

            return healthInfo;
        }
    }

    /// <summary>
    /// معلومات صحة قاعدة البيانات
    /// Database health information
    /// </summary>
    public class DatabaseHealthInfo
    {
        public bool IsHealthy { get; set; }
        public long ResponseTimeMs { get; set; }
        public string? ServerVersion { get; set; }
        public string? Database { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CheckTime { get; set; }
    }

    /// <summary>
    /// استثناء اتصال قاعدة البيانات
    /// Database connection exception
    /// </summary>
    public class DatabaseConnectionException : Exception
    {
        public DatabaseConnectionException(string message) : base(message) { }
        public DatabaseConnectionException(string message, Exception? innerException) : base(message, innerException) { }
    }
}
