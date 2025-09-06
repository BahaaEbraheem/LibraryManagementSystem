using LibraryManagementSystem.BLL.Validation;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Repositories;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// واجهة خدمة الاستعارات
    /// Borrowing service interface
    /// </summary>
    public interface IBorrowingService
    {
        /// <summary>
        /// استعارة كتاب
        /// Borrow a book
        /// </summary>
        Task<ServiceResult<int>> BorrowBookAsync(int userId, int bookId, int borrowingDays = 14);

        /// <summary>
        /// إرجاع كتاب
        /// Return a book
        /// </summary>
        Task<ServiceResult<bool>> ReturnBookAsync(int borrowingId, int userId, string? notes = null);

        /// <summary>
        /// الحصول على استعارات المستخدم النشطة
        /// Get user's active borrowings
        /// </summary>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetUserActiveBorrowingsAsync(int userId);

        /// <summary>
        /// الحصول على جميع استعارات المستخدم
        /// Get all user borrowings
        /// </summary>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetUserBorrowingsAsync(int userId);

        /// <summary>
        /// الحصول على الاستعارات المتأخرة
        /// Get overdue borrowings
        /// </summary>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetOverdueBorrowingsAsync();

        /// <summary>
        /// الحصول على جميع الاستعارات النشطة
        /// Get all active borrowings
        /// </summary>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetActiveBorrowingsAsync();

        /// <summary>
        /// الحصول على جميع الاستعارات (نشطة ومرجعة)
        /// Get all borrowings (active and returned)
        /// </summary>
        Task<ServiceResult<IEnumerable<Borrowing>>> GetAllBorrowingsAsync();

        /// <summary>
        /// الحصول على استعارة بالمعرف
        /// Get borrowing by ID
        /// </summary>
        Task<ServiceResult<Borrowing>> GetBorrowingByIdAsync(int borrowingId);

        /// <summary>
        /// تمديد فترة الاستعارة
        /// Extend borrowing period
        /// </summary>
        Task<ServiceResult<bool>> ExtendBorrowingAsync(int borrowingId, int additionalDays);

        /// <summary>
        /// حساب الرسوم المتأخرة
        /// Calculate late fees
        /// </summary>
        Task<ServiceResult<decimal>> CalculateLateFeeAsync(int borrowingId);

        /// <summary>
        /// التحقق من إمكانية استعارة كتاب
        /// Check if user can borrow a book
        /// </summary>
        Task<ServiceResult<BorrowingEligibility>> CheckBorrowingEligibilityAsync(int userId, int bookId);

        /// <summary>
        /// الحصول على إحصائيات الاستعارات
        /// Get borrowing statistics
        /// </summary>
        Task<ServiceResult<BorrowingStatistics>> GetBorrowingStatisticsAsync();

      }



 
}
