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

        /// <summary>
        /// الحصول على الكتب الأكثر استعارة
        /// Get most borrowed books
        /// </summary>
        Task<ServiceResult<IEnumerable<MostBorrowedBook>>> GetMostBorrowedBooksAsync(int topCount = 10);

        /// <summary>
        /// الحصول على المستخدمين الأكثر نشاطاً
        /// Get most active users
        /// </summary>
        Task<ServiceResult<IEnumerable<MostActiveUser>>> GetMostActiveUsersAsync(int topCount = 10);

        /// <summary>
        /// تجديد استعارة
        /// Renew borrowing
        /// </summary>
        Task<ServiceResult<bool>> RenewBorrowingAsync(int borrowingId);

        /// <summary>
        /// إلغاء استعارة
        /// Cancel borrowing
        /// </summary>
        Task<ServiceResult<bool>> CancelBorrowingAsync(int borrowingId);
    }

    /// <summary>
    /// أهلية الاستعارة
    /// Borrowing eligibility
    /// </summary>
    public class BorrowingEligibility
    {
        /// <summary>
        /// هل يمكن الاستعارة
        /// Can borrow
        /// </summary>
        public bool CanBorrow { get; set; }

        /// <summary>
        /// سبب عدم الإمكانية
        /// Reason for inability
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// عدد الكتب المستعارة حالياً
        /// Current borrowed books count
        /// </summary>
        public int CurrentBorrowedBooks { get; set; }

        /// <summary>
        /// الحد الأقصى للاستعارة
        /// Maximum borrowing limit
        /// </summary>
        public int MaxBorrowingLimit { get; set; }

        /// <summary>
        /// عدد الكتب المتأخرة
        /// Overdue books count
        /// </summary>
        public int OverdueBooks { get; set; }

        /// <summary>
        /// هل الكتاب متاح
        /// Is book available
        /// </summary>
        public bool IsBookAvailable { get; set; }

        /// <summary>
        /// هل المستخدم نشط
        /// Is user active
        /// </summary>
        public bool IsUserActive { get; set; }

        /// <summary>
        /// إنشاء نتيجة إيجابية
        /// Create positive result
        /// </summary>
        public static BorrowingEligibility Eligible()
        {
            return new BorrowingEligibility
            {
                CanBorrow = true
            };
        }

        /// <summary>
        /// إنشاء نتيجة سلبية
        /// Create negative result
        /// </summary>
        public static BorrowingEligibility NotEligible(string reason)
        {
            return new BorrowingEligibility
            {
                CanBorrow = false,
                Reason = reason
            };
        }
    }

    /// <summary>
    /// إعدادات المكتبة
    /// Library settings
    /// </summary>
    public class LibrarySettings
    {
        /// <summary>
        /// الحد الأقصى لعدد الكتب المستعارة لكل مستخدم
        /// Maximum books per user
        /// </summary>
        public int MaxBooksPerUser { get; set; } = 5;

        /// <summary>
        /// عدد أيام الاستعارة الافتراضي
        /// Default borrowing days
        /// </summary>
        public int DefaultBorrowingDays { get; set; } = 14;

        /// <summary>
        /// رسوم التأخير لكل يوم
        /// Late fee per day
        /// </summary>
        public decimal LateFeePerDay { get; set; } = 1.00m;

        /// <summary>
        /// عدد أيام السماح قبل فرض الرسوم
        /// Grace days before charging fees
        /// </summary>
        public int GraceDays { get; set; } = 0;

        /// <summary>
        /// الحد الأقصى لعدد مرات التجديد
        /// Maximum renewal count
        /// </summary>
        public int MaxRenewalCount { get; set; } = 2;

        /// <summary>
        /// عدد أيام التجديد
        /// Renewal days
        /// </summary>
        public int RenewalDays { get; set; } = 14;
    }
}
