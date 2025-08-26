using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryManagementSystem.UI.Authorization
{


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
