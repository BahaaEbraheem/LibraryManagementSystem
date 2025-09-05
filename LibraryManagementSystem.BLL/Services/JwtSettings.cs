using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.BLL.Services
{
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
