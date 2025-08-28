using LibraryManagementSystem.BLL.Validation;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using LibraryManagementSystem.DAL.Repositories;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// واجهة خدمة التحقق من الصلاحيات
    /// Authorization service interface
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// التحقق من صلاحية إدارة الكتب
        /// Check if user can manage books
        /// </summary>
        Task<ServiceResult<bool>> CanManageBooksAsync(int userId);

        /// <summary>
        /// التحقق من صلاحية إدارة المستخدمين
        /// Check if user can manage users
        /// </summary>
        Task<ServiceResult<bool>> CanManageUsersAsync(int userId);

        /// <summary>
        /// التحقق من صلاحية إدارة الاستعارات
        /// Check if user can manage borrowings
        /// </summary>
        Task<ServiceResult<bool>> CanManageBorrowingsAsync(int userId);

        /// <summary>
        /// التحقق من صلاحية عرض الإحصائيات
        /// Check if user can view statistics
        /// </summary>
        Task<ServiceResult<bool>> CanViewStatisticsAsync(int userId);

        /// <summary>
        /// التحقق من صلاحية استعارة الكتب
        /// Check if user can borrow books
        /// </summary>
        Task<ServiceResult<bool>> CanBorrowBooksAsync(int userId);

        /// <summary>
        /// التحقق من صلاحية الوصول للاستعارة المحددة
        /// Check if user can access specific borrowing
        /// </summary>
        Task<ServiceResult<bool>> CanAccessBorrowingAsync(int userId, int borrowingId);

        /// <summary>
        /// الحصول على دور المستخدم
        /// Get user role
        /// </summary>
        Task<ServiceResult<UserRole>> GetUserRoleAsync(int userId);

        /// <summary>
        /// التحقق من وجود المستخدم ونشاطه
        /// Check if user exists and is active
        /// </summary>
        Task<ServiceResult<bool>> IsUserActiveAsync(int userId);
    }
}
