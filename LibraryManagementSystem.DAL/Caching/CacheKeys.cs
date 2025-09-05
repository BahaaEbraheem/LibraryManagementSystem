namespace LibraryManagementSystem.DAL.Caching
{
    /// <summary>
    /// ثوابت مفاتيح التخزين المؤقت
    /// Cache key constants
    /// </summary>
    public static class CacheKeys
    {
        /// <summary>
        /// مفاتيح تخزين الكتب المؤقت
        /// Book cache keys
        /// </summary>
        public static class Books
        {
            /// <summary>جميع الكتب - All books</summary>
            public const string All = "books:all";

            /// <summary>كتاب واحد بالمعرف - Single book by ID</summary>
            public static string ById(int id) => $"books:id:{id}";

            /// <summary>كتاب بالرقم المعياري - Book by ISBN</summary>
            public static string ByIsbn(string isbn) => $"books:isbn:{isbn}";

            /// <summary>الكتب المتاحة - Available books</summary>
            public const string Available = "books:available";

            /// <summary>البحث عن الكتب - Book search</summary>
            public static string Search(string searchTerm) => $"books:search:{searchTerm.ToLowerInvariant()}";

            /// <summary>الكتب حسب المؤلف - Books by author</summary>
            public static string ByAuthor(string author) => $"books:author:{author.ToLowerInvariant()}";

            /// <summary>الكتب حسب النوع - Books by genre</summary>
            public static string ByGenre(string genre) => $"books:genre:{genre.ToLowerInvariant()}";

            /// <summary>إحصائيات الكتب - Book statistics</summary>
            public const string Statistics = "books:statistics";
        }

        /// <summary>
        /// مفاتيح تخزين المستخدمين المؤقت
        /// User cache keys
        /// </summary>
        public static class Users
        {
            /// <summary>جميع المستخدمين - All users</summary>
            public const string All = "users:all";

            /// <summary>مستخدم واحد بالمعرف - Single user by ID</summary>
            public static string ById(int id) => $"users:id:{id}";

            /// <summary>مستخدم بالبريد الإلكتروني - User by email</summary>
            public static string ByEmail(string email) => $"users:email:{email.ToLowerInvariant()}";

            /// <summary>المستخدمين النشطين - Active users</summary>
            public const string Active = "users:active";

            /// <summary>إحصائيات المستخدمين - User statistics</summary>
            public const string Statistics = "users:statistics";

            /// <summary>المستخدمين مع عدد الاستعارات - Users with borrowing count</summary>
            public const string WithBorrowingCount = "users:with-borrowing-count";
        }

        /// <summary>
        /// مفاتيح تخزين الاستعارات المؤقت
        /// Borrowing cache keys
        /// </summary>
        public static class Borrowings
        {
            /// <summary>جميع الاستعارات - All borrowings</summary>
            public const string All = "borrowings:all";

            /// <summary>استعارة واحدة بالمعرف - Single borrowing by ID</summary>
            public static string ById(int id) => $"borrowings:id:{id}";

            /// <summary>الاستعارات النشطة - Active borrowings</summary>
            public const string Active = "borrowings:active";

            /// <summary>الاستعارات المتأخرة - Overdue borrowings</summary>
            public const string Overdue = "borrowings:overdue";

            /// <summary>استعارات المستخدم - User borrowings</summary>
            public static string ByUser(int userId) => $"borrowings:user:{userId}";

            /// <summary>استعارات الكتاب - Book borrowings</summary>
            public static string ByBook(int bookId) => $"borrowings:book:{bookId}";

            /// <summary>الاستعارات النشطة للمستخدم - Active user borrowings</summary>
            public static string ActiveByUser(int userId) => $"borrowings:active:user:{userId}";

            /// <summary>إحصائيات الاستعارات - Borrowing statistics</summary>
            public const string Statistics = "borrowings:statistics";
        }

        /// <summary>
        /// مفاتيح تخزين البحث المؤقت
        /// Search cache keys
        /// </summary>
        public static class Search
        {
            /// <summary>نتائج البحث العامة - General search results</summary>
            public static string General(string query, int page, int pageSize) =>
                $"search:general:{query.ToLowerInvariant()}:page:{page}:size:{pageSize}";

            /// <summary>البحث المتقدم - Advanced search</summary>
            public static string Advanced(string title, string author, string isbn, string genre, int page, int pageSize) =>
                $"search:advanced:t:{title?.ToLowerInvariant()}:a:{author?.ToLowerInvariant()}:i:{isbn}:g:{genre?.ToLowerInvariant()}:page:{page}:size:{pageSize}";

            /// <summary>الاقتراحات التلقائية - Auto suggestions</summary>
            public static string Suggestions(string query) => $"search:suggestions:{query.ToLowerInvariant()}";
        }

   

        /// <summary>
        /// أنماط مفاتيح التخزين المؤقت للإزالة الجماعية
        /// Cache key patterns for bulk removal
        /// </summary>
        public static class Patterns
        {
            /// <summary>جميع مفاتيح الكتب - All book keys</summary>
            public const string AllBooks = "books:";

            /// <summary>جميع مفاتيح المستخدمين - All user keys</summary>
            public const string AllUsers = "users:";

            /// <summary>جميع مفاتيح الاستعارات - All borrowing keys</summary>
            public const string AllBorrowings = "borrowings:";

            /// <summary>جميع مفاتيح البحث - All search keys</summary>
            public const string AllSearch = "search:";

            /// <summary>جميع مفاتيح الإحصائيات - All statistics keys</summary>
            public const string AllStatistics = "statistics:";
        }
    }
}
