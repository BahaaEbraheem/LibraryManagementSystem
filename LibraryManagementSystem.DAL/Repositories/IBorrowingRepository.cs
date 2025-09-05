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
        Task<bool> HasBorrowingsAsync(int bookId);
    }


}
