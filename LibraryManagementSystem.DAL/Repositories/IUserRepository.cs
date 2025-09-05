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
        Task<User?> GetByEmailAsync(string email, int? excludeUserId = null);

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
    }


}
