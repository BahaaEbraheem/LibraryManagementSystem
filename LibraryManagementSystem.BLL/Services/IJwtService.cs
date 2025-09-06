using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs.AuthenticationDTOs;
using LibraryManagementSystem.DAL.Models.Enums;
using System.Security.Claims;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// واجهة خدمة JWT للمصادقة والتفويض
    /// JWT service interface for authentication and authorization
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// إنشاء رمز JWT للمستخدم المسجل دخوله
        /// Generate JWT token for logged in user
        /// </summary>
        /// <param name="loggedInUser">بيانات المستخدم المسجل دخوله</param>
        /// <returns>رمز JWT</returns>
        string GenerateToken(LoggedInUserDto loggedInUser);

        /// <summary>
        /// التحقق من صحة رمز JWT
        /// Validate JWT token
        /// </summary>
        /// <param name="token">رمز JWT</param>
        /// <returns>المطالبات إذا كان الرمز صحيحاً</returns>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// الحصول على معرف المستخدم من الرمز
        /// Get user ID from token
        /// </summary>
        /// <param name="token">رمز JWT</param>
        /// <returns>معرف المستخدم</returns>
        int? GetUserIdFromToken(string token);

        /// <summary>
        /// الحصول على دور المستخدم من الرمز
        /// Get user role from token
        /// </summary>
        /// <param name="token">رمز JWT</param>
        /// <returns>دور المستخدم</returns>
        UserRole? GetUserRoleFromToken(string token);


    }



 
}
