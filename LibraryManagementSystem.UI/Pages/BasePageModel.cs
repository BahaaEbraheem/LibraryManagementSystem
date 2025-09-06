using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages
{
    /// <summary>
    /// نموذج الصفحة الأساسي مع دعم JWT
    /// Base page model with JWT support
    /// </summary>
    public abstract class BasePageModel : PageModel
    {
        protected readonly IJwtService _jwtService;

        protected BasePageModel(IJwtService jwtService)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        /// <summary>
        /// الحصول على معرف المستخدم الحالي من JWT
        /// Get current user ID from JWT
        /// </summary>
        /// <returns>معرف المستخدم أو null إذا لم يكن مسجل الدخول - User ID or null if not logged in</returns>
        protected int? GetCurrentUserId()
        {
            try
            {
                // البحث عن JWT في الكوكيز أولاً ثم في الجلسة
                // Look for JWT in cookies first, then in session
                var jwtToken = Request.Cookies["jwt"] ;

                if (string.IsNullOrEmpty(jwtToken))
                {
                    return null;
                }

                return _jwtService.GetUserIdFromToken(jwtToken);
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// الحصول على دور المستخدم الحالي
        /// Get current user role
        /// </summary>
        /// <returns>دور المستخدم أو null - User role or null</returns>
        protected UserRole? GetCurrentUserRole()
        {
            try
            {
                var jwtToken = Request.Cookies["jwt"];

                if (string.IsNullOrEmpty(jwtToken))
                    return null;

                return _jwtService.GetUserRoleFromToken(jwtToken);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// التحقق من كون المستخدم مدير
        /// Check if current user is admin
        /// </summary>
        /// <returns>true إذا كان مدير - true if admin</returns>
        protected bool IsAdmin()
        {
            var role = GetCurrentUserRole();
            return role.HasValue && role.Value == UserRole.Administrator;
        }


    }
}
