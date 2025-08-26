using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages.Auth
{
    /// <summary>
    /// نموذج صفحة تسجيل الخروج
    /// Logout page model
    /// </summary>
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// اسم المستخدم الحالي
        /// Current user name
        /// </summary>
        public string? CurrentUserName { get; set; }

        /// <summary>
        /// بريد المستخدم الحالي
        /// Current user email
        /// </summary>
        public string? CurrentUserEmail { get; set; }

        /// <summary>
        /// معالج طلب GET - عرض صفحة تأكيد تسجيل الخروج
        /// GET request handler - show logout confirmation page
        /// </summary>
        public IActionResult OnGet()
        {
            // التحقق من تسجيل الدخول
            // Check if user is logged in
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                // إذا لم يكن مسجل دخول، إعادة توجيه لصفحة تسجيل الدخول
                // If not logged in, redirect to login page
                return RedirectToPage("/Auth/Login");
            }

            // الحصول على بيانات المستخدم الحالي
            // Get current user data
            CurrentUserName = HttpContext.Session.GetString("UserName");
            CurrentUserEmail = HttpContext.Session.GetString("UserEmail");

            // عرض صفحة تأكيد تسجيل الخروج
            // Show logout confirmation page
            return Page();
        }

        /// <summary>
        /// معالج طلب POST - تسجيل الخروج
        /// POST request handler - logout
        /// </summary>
        public IActionResult OnPost()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");

                // مسح جميع بيانات الجلسة
                // Clear all session data
                HttpContext.Session.Clear();

                // مسح JWT cookie
                // Clear JWT cookie
                if (Request.Cookies.ContainsKey("jwt"))
                {
                    Response.Cookies.Delete("jwt");
                }

                _logger.LogInformation("تم تسجيل الخروج بنجاح للمستخدم: {Email} - Successful logout for user: {Email}",
                    userEmail ?? "Unknown", userEmail ?? "Unknown");

                TempData["SuccessMessage"] = "تم تسجيل الخروج بنجاح - Successfully logged out";

                return RedirectToPage("/Auth/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تسجيل الخروج - Error during logout");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تسجيل الخروج - An error occurred during logout";
                return RedirectToPage("/Auth/Login");
            }
        }
    }
}
