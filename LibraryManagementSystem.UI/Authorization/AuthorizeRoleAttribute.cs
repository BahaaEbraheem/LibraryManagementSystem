using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryManagementSystem.UI.Authorization
{
    /// <summary>
    /// خاصية التفويض حسب الدور
    /// Role-based authorization attribute
    /// </summary>
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly UserRole[] _allowedRoles;

        /// <summary>
        /// منشئ خاصية التفويض
        /// Authorization attribute constructor
        /// </summary>
        /// <param name="allowedRoles">الأدوار المسموحة</param>
        public AuthorizeRoleAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
        }

        /// <summary>
        /// تنفيذ التفويض
        /// Execute authorization
        /// </summary>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                // التحقق من تسجيل الدخول
                // Check if user is logged in
                var userIdClaim = context.HttpContext.Session.GetString("UserId");
                var userRoleClaim = context.HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(userRoleClaim))
                {
                    // إعادة توجيه لصفحة تسجيل الدخول
                    // Redirect to login page
                    context.Result = new RedirectToPageResult("/Auth/Login", new { returnUrl = context.HttpContext.Request.Path });
                    return;
                }

                // التحقق من الدور
                // Check user role
                if (Enum.TryParse<UserRole>(userRoleClaim, out var userRole))
                {
                    if (!_allowedRoles.Contains(userRole))
                    {
                        // إعادة توجيه لصفحة عدم التفويض
                        // Redirect to unauthorized page
                        context.Result = new RedirectToPageResult("/Error", new { statusCode = 403 });
                        return;
                    }
                }
                else
                {
                    // دور غير صحيح
                    // Invalid role
                    context.Result = new RedirectToPageResult("/Auth/Login");
                    return;
                }

                // التحقق من حالة المستخدم النشطة
                // Check if user is active
                var isActiveClaim = context.HttpContext.Session.GetString("IsActive");
                if (string.IsNullOrEmpty(isActiveClaim) || !bool.TryParse(isActiveClaim, out var isActive) || !isActive)
                {
                    // المستخدم غير نشط
                    // User is not active
                    context.HttpContext.Session.Clear();
                    context.Result = new RedirectToPageResult("/Auth/Login", new { message = "حسابك غير نشط - Your account is not active" });
                    return;
                }
            }
            catch (Exception)
            {
                // في حالة حدوث خطأ، إعادة توجيه لتسجيل الدخول
                // In case of error, redirect to login
                context.Result = new RedirectToPageResult("/Auth/Login");
            }
        }
    }

    /// <summary>
    /// خاصية تفويض المدير فقط
    /// Admin-only authorization attribute
    /// </summary>
    public class AdminOnlyAttribute : AuthorizeRoleAttribute
    {
        public AdminOnlyAttribute() : base(UserRole.Administrator) { }
    }

    /// <summary>
    /// خاصية تفويض المستخدمين المسجلين
    /// Authenticated users authorization attribute
    /// </summary>
    public class AuthenticatedOnlyAttribute : AuthorizeRoleAttribute
    {
        public AuthenticatedOnlyAttribute() : base(UserRole.Administrator, UserRole.User) { }
    }

    /// <summary>
    /// خاصية التفويض بـ JWT
    /// JWT authorization attribute
    /// </summary>
    public class JwtAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly UserRole[] _allowedRoles;

        /// <summary>
        /// منشئ خاصية التفويض بـ JWT
        /// JWT authorization attribute constructor
        /// </summary>
        public JwtAuthorizeAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? new[] { UserRole.Administrator, UserRole.User };
        }

        /// <summary>
        /// تنفيذ التفويض بـ JWT
        /// Execute JWT authorization
        /// </summary>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var jwtService = context.HttpContext.RequestServices.GetService<BLL.Services.IJwtService>();
                if (jwtService == null)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // الحصول على الرمز من الهيدر
                // Get token from header
                var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();

                // التحقق من صحة الرمز
                // Validate token
                var principal = jwtService.ValidateToken(token);
                if (principal == null)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // التحقق من الدور
                // Check role
                var userRole = jwtService.GetUserRoleFromToken(token);
                if (userRole == null || !_allowedRoles.Contains(userRole.Value))
                {
                    context.Result = new ForbidResult();
                    return;
                }

                // إضافة المطالبات للسياق
                // Add claims to context
                context.HttpContext.User = principal;
            }
            catch (Exception)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }

    /// <summary>
    /// خاصية تفويض المدير بـ JWT
    /// JWT admin authorization attribute
    /// </summary>
    public class JwtAdminOnlyAttribute : JwtAuthorizeAttribute
    {
        public JwtAdminOnlyAttribute() : base(UserRole.Administrator) { }
    }

    /// <summary>
    /// خاصية تفويض المستخدمين المصادق عليهم بـ JWT
    /// JWT authenticated users authorization attribute
    /// </summary>
    public class JwtAuthenticatedOnlyAttribute : JwtAuthorizeAttribute
    {
        public JwtAuthenticatedOnlyAttribute() : base(UserRole.Administrator, UserRole.User) { }
    }
}
