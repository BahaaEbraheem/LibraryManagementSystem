using LibraryManagementSystem.DAL.Models;
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
        /// إنشاء رمز JWT للمستخدم
        /// Generate JWT token for user
        /// </summary>
        /// <param name="user">بيانات المستخدم</param>
        /// <returns>رمز JWT</returns>
        string GenerateToken(User user);

        /// <summary>
        /// إنشاء رمز JWT للمستخدم المسجل دخوله
        /// Generate JWT token for logged in user
        /// </summary>
        /// <param name="loggedInUser">بيانات المستخدم المسجل دخوله</param>
        /// <returns>رمز JWT</returns>
        string GenerateToken(DAL.Models.DTOs.LoggedInUserDto loggedInUser);

        /// <summary>
        /// إنشاء رمز تحديث
        /// Generate refresh token
        /// </summary>
        /// <returns>رمز التحديث</returns>
        string GenerateRefreshToken();

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
        DAL.Models.Enums.UserRole? GetUserRoleFromToken(string token);

        /// <summary>
        /// التحقق من انتهاء صلاحية الرمز
        /// Check if token is expired
        /// </summary>
        /// <param name="token">رمز JWT</param>
        /// <returns>true إذا كان منتهي الصلاحية</returns>
        bool IsTokenExpired(string token);

        /// <summary>
        /// تحديث رمز JWT باستخدام رمز التحديث
        /// Refresh JWT token using refresh token
        /// </summary>
        /// <param name="token">رمز JWT الحالي</param>
        /// <param name="refreshToken">رمز التحديث</param>
        /// <returns>رمز JWT جديد</returns>
        Task<string?> RefreshTokenAsync(string token, string refreshToken);

        /// <summary>
        /// إبطال رمز JWT
        /// Revoke JWT token
        /// </summary>
        /// <param name="token">رمز JWT</param>
        /// <returns>true إذا تم الإبطال بنجاح</returns>
        Task<bool> RevokeTokenAsync(string token);
    }

    /// <summary>
    /// نتيجة المصادقة بـ JWT
    /// JWT authentication result
    /// </summary>
    public class JwtAuthenticationResult
    {
        /// <summary>
        /// رمز الوصول
        /// Access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// رمز التحديث
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// وقت انتهاء الصلاحية
        /// Expiration time
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// نوع الرمز
        /// Token type
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// بيانات المستخدم
        /// User information
        /// </summary>
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// معلومات المستخدم للـ JWT
    /// User information for JWT
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// معرف المستخدم
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// البريد الإلكتروني
        /// Email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// الاسم الكامل
        /// Full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// الدور
        /// Role
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// هل المستخدم نشط
        /// Is user active
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// إعدادات JWT
    /// JWT settings
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// المفتاح السري
        /// Secret key
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// الجهة المصدرة
        /// Issuer
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// الجمهور المستهدف
        /// Audience
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// مدة انتهاء الصلاحية بالدقائق
        /// Expiration time in minutes
        /// </summary>
        public int ExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// مدة انتهاء صلاحية رمز التحديث بالأيام
        /// Refresh token expiration in days
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// السماح بتذكر تسجيل الدخول
        /// Allow remember me
        /// </summary>
        public bool AllowRememberMe { get; set; } = true;

        /// <summary>
        /// مدة تذكر تسجيل الدخول بالأيام
        /// Remember me duration in days
        /// </summary>
        public int RememberMeDays { get; set; } = 30;
    }
}
