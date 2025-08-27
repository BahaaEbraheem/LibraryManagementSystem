using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// تهيئة قاعدة البيانات
        /// Initialize database
        /// </summary>
        Task InitializeDatabaseAsync();
    }
}
