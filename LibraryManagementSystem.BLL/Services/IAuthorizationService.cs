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
        /// الحصول على دور المستخدم
        /// Get user role
        /// </summary>
        Task<ServiceResult<UserRole>> GetUserRoleAsync(int userId);

    
    }
}
