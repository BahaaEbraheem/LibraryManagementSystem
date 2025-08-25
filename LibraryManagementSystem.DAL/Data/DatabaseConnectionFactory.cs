using Microsoft.Data.SqlClient;
using System.Data;

namespace LibraryManagementSystem.DAL.Data
{
    /// <summary>
    /// مصنع لإنشاء اتصالات قاعدة البيانات
    /// Factory for creating database connections
    /// </summary>
    public interface IDatabaseConnectionFactory
    {
        /// <summary>
        /// إنشاء اتصال جديد بقاعدة البيانات
        /// Create a new database connection
        /// </summary>
        IDbConnection CreateConnection();

        /// <summary>
        /// إنشاء اتصال جديد بقاعدة البيانات بشكل غير متزامن
        /// Create a new database connection asynchronously
        /// </summary>
        Task<IDbConnection> CreateConnectionAsync();
    }

    /// <summary>
    /// تنفيذ مصنع اتصالات قاعدة البيانات باستخدام SQL Server
    /// Implementation of database connection factory using SQL Server
    /// </summary>
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly string _connectionString;

        /// <summary>
        /// منشئ الفئة مع سلسلة الاتصال
        /// Constructor with connection string
        /// </summary>
        /// <param name="connectionString">سلسلة الاتصال بقاعدة البيانات</param>
        public DatabaseConnectionFactory(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// إنشاء اتصال جديد وفتحه
        /// Create and open a new connection
        /// </summary>
        public IDbConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// إنشاء اتصال جديد وفتحه بشكل غير متزامن
        /// Create and open a new connection asynchronously
        /// </summary>
        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }

    /// <summary>
    /// فئة مساعدة لقاعدة البيانات للعمليات الشائعة
    /// Database helper class for common operations
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// تنفيذ استعلام scalar وإرجاع النتيجة
        /// Executes a scalar query and returns the result
        /// </summary>
        /// <typeparam name="T">نوع البيانات المطلوب إرجاعه</typeparam>
        /// <param name="connection">اتصال قاعدة البيانات</param>
        /// <param name="sql">استعلام SQL</param>
        /// <param name="parameters">معاملات الاستعلام</param>
        public static async Task<T?> ExecuteScalarAsync<T>(IDbConnection connection, string sql, object? parameters = null)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            var result = await ((SqlCommand)command).ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? default(T) : (T)result;
        }

        /// <summary>
        /// تنفيذ أمر غير استعلامي وإرجاع عدد الصفوف المتأثرة
        /// Executes a non-query command and returns the number of affected rows
        /// </summary>
        /// <param name="connection">اتصال قاعدة البيانات</param>
        /// <param name="sql">أمر SQL</param>
        /// <param name="parameters">معاملات الأمر</param>
        public static async Task<int> ExecuteNonQueryAsync(IDbConnection connection, string sql, object? parameters = null)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            return await ((SqlCommand)command).ExecuteNonQueryAsync();
        }

        /// <summary>
        /// تنفيذ استعلام وإرجاع قارئ البيانات
        /// Executes a query and returns a data reader
        /// </summary>
        /// <param name="connection">اتصال قاعدة البيانات</param>
        /// <param name="sql">استعلام SQL</param>
        /// <param name="parameters">معاملات الاستعلام</param>
        public static async Task<IDataReader> ExecuteReaderAsync(IDbConnection connection, string sql, object? parameters = null)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            return await ((SqlCommand)command).ExecuteReaderAsync();
        }

        /// <summary>
        /// إضافة معاملات إلى الأمر من كائن مجهول
        /// Adds parameters to a command from an anonymous object
        /// </summary>
        /// <param name="command">أمر قاعدة البيانات</param>
        /// <param name="parameters">كائن المعاملات</param>
        private static void AddParameters(IDbCommand command, object parameters)
        {
            var properties = parameters.GetType().GetProperties();
            foreach (var property in properties)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{property.Name}";
                parameter.Value = property.GetValue(parameters) ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// تحويل قارئ البيانات إلى كائن باستخدام الانعكاس
        /// Maps a data reader to an object using reflection
        /// </summary>
        /// <typeparam name="T">نوع الكائن المطلوب</typeparam>
        /// <param name="reader">قارئ البيانات</param>
        public static T MapToObject<T>(IDataReader reader) where T : new()
        {
            var obj = new T();
            var properties = typeof(T).GetProperties();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var property = properties.FirstOrDefault(p =>
                    string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

                if (property != null && !reader.IsDBNull(i))
                {
                    var value = reader.GetValue(i);
                    if (value != DBNull.Value)
                    {
                        // التعامل مع الأنواع القابلة للإلغاء - Handle nullable types
                        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        var convertedValue = Convert.ChangeType(value, targetType);
                        property.SetValue(obj, convertedValue);
                    }
                }
            }

            return obj;
        }
    }
}
