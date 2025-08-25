using LibraryManagementSystem.DAL.Models;

namespace LibraryManagementSystem.DAL.Repositories
{
    /// <summary>
    /// واجهة مستودع المستخدمين
    /// User repository interface
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// الحصول على جميع المستخدمين
        /// Get all users
        /// </summary>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// الحصول على مستخدم بالمعرف
        /// Get user by ID
        /// </summary>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// الحصول على مستخدم بالبريد الإلكتروني
        /// Get user by email
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// الحصول على المستخدمين النشطين
        /// Get active users
        /// </summary>
        Task<IEnumerable<User>> GetActiveUsersAsync();

        /// <summary>
        /// البحث عن المستخدمين
        /// Search users
        /// </summary>
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);

        /// <summary>
        /// إضافة مستخدم جديد
        /// Add a new user
        /// </summary>
        Task<int> AddAsync(User user);

        /// <summary>
        /// تحديث مستخدم موجود
        /// Update an existing user
        /// </summary>
        Task<bool> UpdateAsync(User user);

        /// <summary>
        /// حذف مستخدم
        /// Delete a user
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// تفعيل أو إلغاء تفعيل مستخدم
        /// Activate or deactivate a user
        /// </summary>
        Task<bool> SetActiveStatusAsync(int id, bool isActive);

        /// <summary>
        /// التحقق من وجود مستخدم بالبريد الإلكتروني
        /// Check if user exists by email
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// الحصول على إحصائيات المستخدمين
        /// Get user statistics
        /// </summary>
        Task<UserStatistics> GetUserStatisticsAsync();

        /// <summary>
        /// الحصول على المستخدمين مع عدد الكتب المستعارة
        /// Get users with borrowed books count
        /// </summary>
        Task<IEnumerable<UserWithBorrowingCount>> GetUsersWithBorrowingCountAsync();
    }

    /// <summary>
    /// إحصائيات المستخدمين
    /// User statistics
    /// </summary>
    public class UserStatistics
    {
        /// <summary>
        /// إجمالي عدد المستخدمين
        /// Total number of users
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// عدد المستخدمين النشطين
        /// Number of active users
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// عدد المستخدمين غير النشطين
        /// Number of inactive users
        /// </summary>
        public int InactiveUsers { get; set; }

        /// <summary>
        /// عدد المستخدمين الجدد هذا الشهر
        /// Number of new users this month
        /// </summary>
        public int NewUsersThisMonth { get; set; }

        /// <summary>
        /// عدد المستخدمين الذين لديهم استعارات نشطة
        /// Number of users with active borrowings
        /// </summary>
        public int UsersWithActiveBorrowings { get; set; }
    }

    /// <summary>
    /// مستخدم مع عدد الاستعارات
    /// User with borrowing count
    /// </summary>
    public class UserWithBorrowingCount
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
        /// عدد الكتب المستعارة حالياً
        /// Current borrowed books count
        /// </summary>
        public int CurrentBorrowedBooks { get; set; }

        /// <summary>
        /// إجمالي عدد الكتب المستعارة
        /// Total borrowed books count
        /// </summary>
        public int TotalBorrowedBooks { get; set; }

        /// <summary>
        /// عدد الكتب المتأخرة
        /// Overdue books count
        /// </summary>
        public int OverdueBooks { get; set; }

        /// <summary>
        /// إجمالي الرسوم المتأخرة
        /// Total late fees
        /// </summary>
        public decimal TotalLateFees { get; set; }
    }
}
