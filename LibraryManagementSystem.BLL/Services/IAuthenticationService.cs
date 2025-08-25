using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Repositories;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// واجهة خدمة المصادقة
    /// Authentication service interface
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// تسجيل الدخول
        /// Login
        /// </summary>
        Task<ServiceResult<LoggedInUserDto>> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// تسجيل مستخدم جديد
        /// Register new user
        /// </summary>
        Task<ServiceResult<int>> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// تغيير كلمة المرور
        /// Change password
        /// </summary>
        Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);

        /// <summary>
        /// التحقق من وجود البريد الإلكتروني
        /// Check if email exists
        /// </summary>
        Task<ServiceResult<bool>> EmailExistsAsync(string email);

        /// <summary>
        /// تشفير كلمة المرور
        /// Hash password
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// التحقق من كلمة المرور
        /// Verify password hash
        /// </summary>
        bool VerifyPassword(string password, string hash);
    }
}
