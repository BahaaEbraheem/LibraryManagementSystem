using LibraryManagementSystem.DAL.Models;

namespace LibraryManagementSystem.DAL.Repositories
{
    /// <summary>
    /// واجهة مستودع الاستعارات
    /// Borrowing repository interface
    /// </summary>
    public interface IBorrowingRepository
    {
        /// <summary>
        /// الحصول على جميع الاستعارات
        /// Get all borrowings
        /// </summary>
        Task<IEnumerable<Borrowing>> GetAllAsync();

        /// <summary>
        /// الحصول على استعارة بالمعرف
        /// Get borrowing by ID
        /// </summary>
        Task<Borrowing?> GetByIdAsync(int id);

        /// <summary>
        /// الحصول على الاستعارات النشطة
        /// Get active borrowings
        /// </summary>
        Task<IEnumerable<Borrowing>> GetActiveBorrowingsAsync();

        /// <summary>
        /// الحصول على الاستعارات المتأخرة
        /// Get overdue borrowings
        /// </summary>
        Task<IEnumerable<Borrowing>> GetOverdueBorrowingsAsync();

        /// <summary>
        /// الحصول على استعارات المستخدم
        /// Get user borrowings
        /// </summary>
        Task<IEnumerable<Borrowing>> GetUserBorrowingsAsync(int userId);

        /// <summary>
        /// الحصول على الاستعارات النشطة للمستخدم
        /// Get active user borrowings
        /// </summary>
        Task<IEnumerable<Borrowing>> GetActiveUserBorrowingsAsync(int userId);

        /// <summary>
        /// الحصول على استعارات الكتاب
        /// Get book borrowings
        /// </summary>
        Task<IEnumerable<Borrowing>> GetBookBorrowingsAsync(int bookId);

        /// <summary>
        /// إضافة استعارة جديدة
        /// Add a new borrowing
        /// </summary>
        Task<int> AddAsync(Borrowing borrowing);

        /// <summary>
        /// تحديث استعارة موجودة
        /// Update an existing borrowing
        /// </summary>
        Task<bool> UpdateAsync(Borrowing borrowing);

        /// <summary>
        /// إرجاع كتاب
        /// Return a book
        /// </summary>
        Task<bool> ReturnBookAsync(int borrowingId, DateTime returnDate, decimal lateFee = 0, string? notes = null);

        /// <summary>
        /// حذف استعارة
        /// Delete a borrowing
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// التحقق من إمكانية استعارة كتاب
        /// Check if a book can be borrowed
        /// </summary>
        Task<bool> CanBorrowBookAsync(int userId, int bookId);

        /// <summary>
        /// الحصول على عدد الكتب المستعارة حالياً للمستخدم
        /// Get current borrowed books count for user
        /// </summary>
        Task<int> GetCurrentBorrowedBooksCountAsync(int userId);

        /// <summary>
        /// الحصول على إحصائيات الاستعارات
        /// Get borrowing statistics
        /// </summary>
        Task<BorrowingStatistics> GetBorrowingStatisticsAsync();

        /// <summary>
        /// الحصول على الكتب الأكثر استعارة
        /// Get most borrowed books
        /// </summary>
        Task<IEnumerable<MostBorrowedBook>> GetMostBorrowedBooksAsync(int topCount = 10);

        /// <summary>
        /// الحصول على المستخدمين الأكثر نشاطاً في الاستعارة
        /// Get most active borrowing users
        /// </summary>
        Task<IEnumerable<MostActiveUser>> GetMostActiveUsersAsync(int topCount = 10);
    }

    /// <summary>
    /// إحصائيات الاستعارات
    /// Borrowing statistics
    /// </summary>
    public class BorrowingStatistics
    {
        /// <summary>
        /// إجمالي عدد الاستعارات
        /// Total number of borrowings
        /// </summary>
        public int TotalBorrowings { get; set; }

        /// <summary>
        /// عدد الاستعارات النشطة
        /// Number of active borrowings
        /// </summary>
        public int ActiveBorrowings { get; set; }

        /// <summary>
        /// عدد الاستعارات المتأخرة
        /// Number of overdue borrowings
        /// </summary>
        public int OverdueBorrowings { get; set; }

        /// <summary>
        /// عدد الاستعارات المرجعة
        /// Number of returned borrowings
        /// </summary>
        public int ReturnedBorrowings { get; set; }

        /// <summary>
        /// إجمالي الرسوم المتأخرة
        /// Total late fees
        /// </summary>
        public decimal TotalLateFees { get; set; }

        /// <summary>
        /// متوسط فترة الاستعارة بالأيام
        /// Average borrowing period in days
        /// </summary>
        public double AverageBorrowingPeriod { get; set; }

        /// <summary>
        /// عدد الاستعارات هذا الشهر
        /// Number of borrowings this month
        /// </summary>
        public int BorrowingsThisMonth { get; set; }

        /// <summary>
        /// عدد الإرجاعات هذا الشهر
        /// Number of returns this month
        /// </summary>
        public int ReturnsThisMonth { get; set; }
    }

    /// <summary>
    /// الكتاب الأكثر استعارة
    /// Most borrowed book
    /// </summary>
    public class MostBorrowedBook
    {
        /// <summary>
        /// معرف الكتاب
        /// Book ID
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// عنوان الكتاب
        /// Book title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// مؤلف الكتاب
        /// Book author
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// الرقم المعياري للكتاب
        /// Book ISBN
        /// </summary>
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// عدد مرات الاستعارة
        /// Number of times borrowed
        /// </summary>
        public int BorrowCount { get; set; }

        /// <summary>
        /// عدد الاستعارات النشطة حالياً
        /// Current active borrowings
        /// </summary>
        public int CurrentActiveBorrowings { get; set; }
    }

    /// <summary>
    /// المستخدم الأكثر نشاطاً في الاستعارة
    /// Most active borrowing user
    /// </summary>
    public class MostActiveUser
    {
        /// <summary>
        /// معرف المستخدم
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// الاسم الكامل
        /// Full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// البريد الإلكتروني
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// إجمالي عدد الاستعارات
        /// Total borrowings count
        /// </summary>
        public int TotalBorrowings { get; set; }

        /// <summary>
        /// عدد الاستعارات النشطة حالياً
        /// Current active borrowings
        /// </summary>
        public int CurrentActiveBorrowings { get; set; }

        /// <summary>
        /// عدد الكتب المتأخرة
        /// Overdue books count
        /// </summary>
        public int OverdueBooks { get; set; }
    }
}
