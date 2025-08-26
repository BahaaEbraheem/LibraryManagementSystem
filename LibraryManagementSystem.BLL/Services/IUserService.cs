using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Repositories;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// واجهة خدمة المستخدمين
    /// User service interface
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// الحصول على جميع المستخدمين
        /// Get all users
        /// </summary>
        Task<IEnumerable<User>> GetAllUsersAsync();

        /// <summary>
        /// الحصول على مستخدم بالمعرف
        /// Get user by ID
        /// </summary>
        Task<ServiceResult<User>> GetUserByIdAsync(int id);

        /// <summary>
        /// الحصول على مستخدم بالبريد الإلكتروني
        /// Get user by email
        /// </summary>
        Task<ServiceResult<User>> GetUserByEmailAsync(string email);

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
        Task<ServiceResult<int>> AddUserAsync(User user);

        /// <summary>
        /// تحديث مستخدم موجود
        /// Update an existing user
        /// </summary>
        Task<ServiceResult<bool>> UpdateUserAsync(User user);

        /// <summary>
        /// حذف مستخدم
        /// Delete a user
        /// </summary>
        Task<ServiceResult<bool>> DeleteUserAsync(int id);

        /// <summary>
        /// حذف مستخدم مع التحقق من الصلاحيات
        /// Delete a user with authorization check
        /// </summary>
        Task<ServiceResult<bool>> DeleteUserAsync(int id, int currentUserId);

        /// <summary>
        /// تفعيل أو إلغاء تفعيل مستخدم
        /// Activate or deactivate a user
        /// </summary>
        Task<ServiceResult<bool>> SetActiveStatusAsync(int id, bool isActive);

        /// <summary>
        /// التحقق من وجود مستخدم بالبريد الإلكتروني
        /// Check if user exists by email
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email);
    }
}
