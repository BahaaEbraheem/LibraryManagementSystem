using System.Data;
using LibraryManagementSystem.DAL.Data;

namespace LibraryManagementSystem.DAL.QueryOptimization
{
    /// <summary>
    /// أنماط الاستعلام المحسنة لمحاكاة AsNoTracking و AsSplitQuery
    /// Optimized query patterns to simulate AsNoTracking and AsSplitQuery
    /// </summary>
    public static class OptimizedQueryPatterns
    {
        /// <summary>
        /// تنفيذ استعلام للقراءة فقط (محاكاة AsNoTracking)
        /// Execute read-only query (simulating AsNoTracking)
        /// </summary>
        /// <typeparam name="T">نوع البيانات المُرجعة</typeparam>
        /// <param name="connection">اتصال قاعدة البيانات</param>
        /// <param name="sql">استعلام SQL</param>
        /// <param name="parameters">معاملات الاستعلام</param>
        /// <param name="commandTimeout">مهلة الاستعلام بالثواني</param>
        /// <returns>نتائج الاستعلام</returns>
        public static async Task<IEnumerable<T>> ExecuteReadOnlyQueryAsync<T>(
            IDbConnection connection,
            string sql,
            object? parameters = null,
            int? commandTimeout = null) where T : new()
        {
            // تحسينات للقراءة فقط:
            // Read-only optimizations:
            // 1. استخدام NOLOCK hint للقراءة بدون قفل
            // 2. تحسين timeout للاستعلامات الطويلة
            // 3. استخدام streaming للبيانات الكبيرة

            var optimizedSql = OptimizeForReadOnly(sql);

            return await DatabaseHelper.ExecuteQueryAsync<T>(
                connection,
                optimizedSql,
                parameters,
                commandTimeout ?? 30);
        }

        /// <summary>
        /// تنفيذ استعلام مقسم (محاكاة AsSplitQuery)
        /// Execute split query (simulating AsSplitQuery)
        /// </summary>
        /// <typeparam name="TParent">نوع الكائن الرئيسي</typeparam>
        /// <typeparam name="TChild">نوع الكائن الفرعي</typeparam>
        /// <param name="connection">اتصال قاعدة البيانات</param>
        /// <param name="parentSql">استعلام الكائن الرئيسي</param>
        /// <param name="childSql">استعلام الكائن الفرعي</param>
        /// <param name="parameters">معاملات الاستعلام</param>
        /// <param name="mapFunction">دالة ربط البيانات</param>
        /// <returns>النتائج المدمجة</returns>
        public static async Task<IEnumerable<TParent>> ExecuteSplitQueryAsync<TParent, TChild>(
            IDbConnection connection,
            string parentSql,
            string childSql,
            object? parameters,
            Func<TParent, IEnumerable<TChild>, TParent> mapFunction)
            where TParent : new()
            where TChild : new()
        {
            // تنفيذ الاستعلامات بشكل منفصل لتحسين الأداء
            // Execute queries separately for better performance

            var parentResults = await ExecuteReadOnlyQueryAsync<TParent>(connection, parentSql, parameters);
            var childResults = await ExecuteReadOnlyQueryAsync<TChild>(connection, childSql, parameters);

            // ربط النتائج
            // Map results
            var parentList = parentResults.ToList();
            var childList = childResults.ToList();

            return parentList.Select(parent => mapFunction(parent, childList));
        }

        /// <summary>
        /// تحسين استعلام للقراءة فقط
        /// Optimize query for read-only access
        /// </summary>
        /// <param name="sql">الاستعلام الأصلي</param>
        /// <returns>الاستعلام المحسن</returns>
        private static string OptimizeForReadOnly(string sql)
        {
            // إضافة NOLOCK hints للجداول في استعلامات SELECT
            // Add NOLOCK hints to tables in SELECT queries
            if (sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                // إضافة NOLOCK للجداول الرئيسية
                // Add NOLOCK to main tables
                sql = sql.Replace("FROM Books", "FROM Books WITH (NOLOCK)")
                        .Replace("FROM Users", "FROM Users WITH (NOLOCK)")
                        .Replace("FROM Borrowings", "FROM Borrowings WITH (NOLOCK)")
                        .Replace("JOIN Books", "JOIN Books WITH (NOLOCK)")
                        .Replace("JOIN Users", "JOIN Users WITH (NOLOCK)")
                        .Replace("JOIN Borrowings", "JOIN Borrowings WITH (NOLOCK)");
            }

            return sql;
        }

        /// <summary>
        /// إنشاء استعلام محسن للبحث مع فهرسة
        /// Create optimized search query with indexing
        /// </summary>
        /// <param name="baseQuery">الاستعلام الأساسي</param>
        /// <param name="searchTerm">مصطلح البحث</param>
        /// <param name="searchFields">حقول البحث</param>
        /// <returns>الاستعلام المحسن</returns>
        public static string CreateOptimizedSearchQuery(string baseQuery, string? searchTerm, params string[] searchFields)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || !searchFields.Any())
            {
                return baseQuery;
            }

            // إنشاء شروط البحث المحسنة
            // Create optimized search conditions
            var searchConditions = searchFields
                .Select(field => $"{field} LIKE @SearchTerm")
                .ToList();

            // استخدام OPTION (RECOMPILE) لتحسين خطة التنفيذ
            // Use OPTION (RECOMPILE) for execution plan optimization
            var whereClause = $"WHERE ({string.Join(" OR ", searchConditions)})";

            if (baseQuery.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
            {
                whereClause = $"AND ({string.Join(" OR ", searchConditions)})";
            }

            return $"{baseQuery} {whereClause} OPTION (RECOMPILE)";
        }

        /// <summary>
        /// إنشاء استعلام مُحسن للصفحات
        /// Create optimized pagination query
        /// </summary>
        /// <param name="baseQuery">الاستعلام الأساسي</param>
        /// <param name="orderBy">ترتيب النتائج</param>
        /// <param name="pageNumber">رقم الصفحة</param>
        /// <param name="pageSize">حجم الصفحة</param>
        /// <returns>الاستعلام المحسن مع الصفحات</returns>
        public static string CreateOptimizedPaginationQuery(string baseQuery, string orderBy, int pageNumber, int pageSize)
        {
            var offset = (pageNumber - 1) * pageSize;

            // استخدام OFFSET/FETCH المحسن
            // Use optimized OFFSET/FETCH
            return $@"
                {baseQuery}
                ORDER BY {orderBy}
                OFFSET {offset} ROWS
                FETCH NEXT {pageSize} ROWS ONLY
                OPTION (OPTIMIZE FOR (@PageSize = {pageSize}))";
        }

        /// <summary>
        /// إنشاء استعلام إحصائيات محسن
        /// Create optimized statistics query
        /// </summary>
        /// <param name="tableName">اسم الجدول</param>
        /// <param name="conditions">الشروط</param>
        /// <returns>استعلام الإحصائيات المحسن</returns>
        public static string CreateOptimizedStatsQuery(string tableName, string? conditions = null)
        {
            var whereClause = string.IsNullOrWhiteSpace(conditions) ? "" : $"WHERE {conditions}";

            return $@"
                SELECT COUNT(*) as TotalCount
                FROM {tableName} WITH (NOLOCK)
                {whereClause}
                OPTION (MAXDOP 1)";
        }

        /// <summary>
        /// إنشاء استعلام مجمع للبيانات المترابطة
        /// Create aggregated query for related data
        /// </summary>
        /// <param name="mainTable">الجدول الرئيسي</param>
        /// <param name="relatedTables">الجداول المترابطة</param>
        /// <param name="joinConditions">شروط الربط</param>
        /// <param name="selectFields">الحقول المطلوبة</param>
        /// <returns>الاستعلام المجمع</returns>
        public static string CreateAggregatedQuery(
            string mainTable,
            Dictionary<string, string> relatedTables,
            Dictionary<string, string> joinConditions,
            string selectFields)
        {
            var joins = relatedTables
                .Select(rt => $"LEFT JOIN {rt.Value} WITH (NOLOCK) ON {joinConditions[rt.Key]}")
                .ToList();

            return $@"
                SELECT {selectFields}
                FROM {mainTable} WITH (NOLOCK)
                {string.Join("\n                ", joins)}";
        }

        /// <summary>
        /// تحسين استعلام للبحث النصي
        /// Optimize query for text search
        /// </summary>
        /// <param name="query">الاستعلام الأصلي</param>
        /// <param name="useFullTextSearch">استخدام البحث النصي الكامل</param>
        /// <returns>الاستعلام المحسن</returns>
        public static string OptimizeForTextSearch(string query, bool useFullTextSearch = false)
        {
            if (useFullTextSearch)
            {
                // استبدال LIKE بـ CONTAINS للبحث النصي الكامل
                // Replace LIKE with CONTAINS for full-text search
                query = query.Replace("LIKE @SearchTerm", "CONTAINS(*, @SearchTerm)");
            }
            else
            {
                // تحسين LIKE patterns
                // Optimize LIKE patterns
                query = query.Replace("LIKE @SearchTerm", "LIKE @SearchTerm + '%'");
            }

            return query;
        }

        /// <summary>
        /// إنشاء استعلام محسن للتقارير
        /// Create optimized reporting query
        /// </summary>
        /// <param name="baseQuery">الاستعلام الأساسي</param>
        /// <param name="groupByFields">حقول التجميع</param>
        /// <param name="aggregateFields">حقول التجميع الإحصائي</param>
        /// <returns>استعلام التقارير المحسن</returns>
        public static string CreateOptimizedReportQuery(
            string baseQuery,
            IEnumerable<string> groupByFields,
            IEnumerable<string> aggregateFields)
        {
            var groupBy = string.Join(", ", groupByFields);
            var aggregates = string.Join(", ", aggregateFields);

            return $@"
                {baseQuery}
                GROUP BY {groupBy}
                ORDER BY {groupBy}
                OPTION (HASH GROUP, MAXDOP 2)";
        }
    }
}
