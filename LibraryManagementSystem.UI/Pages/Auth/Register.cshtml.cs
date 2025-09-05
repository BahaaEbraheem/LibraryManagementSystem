using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Models.DTOs.AuthenticationDTOs;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages.Auth
{
    /// <summary>
    /// نموذج صفحة التسجيل
    /// Register page model
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IAuthenticationService authenticationService, ILogger<RegisterModel> logger)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// نموذج التسجيل
        /// Register model
        /// </summary>
        [BindProperty]
        public RegisterDto RegisterData { get; set; } = new RegisterDto();

        /// <summary>
        /// رسالة الخطأ
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// رسالة النجاح
        /// Success message
        /// </summary>
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// هل المستخدم الحالي مدير (لإظهار خيار الدور)
        /// Is current user admin (to show role option)
        /// </summary>
        public bool IsCurrentUserAdmin { get; set; }

        /// <summary>
        /// معالج طلب GET - تحميل صفحة التسجيل
        /// GET request handler - load register page
        /// </summary>
        public IActionResult OnGet()
        {
            // التحقق من تسجيل الدخول المسبق
            // Check if already logged in
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId.HasValue)
            {
                var currentUserRole = HttpContext.Session.GetString("UserRole");
                IsCurrentUserAdmin = currentUserRole == UserRole.Administrator.ToString();

                // إذا لم يكن مدير ولديه حساب، إعادة توجيه للصفحة الرئيسية
                // If not admin and already logged in, redirect to home
                if (!IsCurrentUserAdmin)
                {
                    return RedirectToPage("/Index");
                }
            }

            // عرض رسائل من TempData
            // Display messages from TempData
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }
            if (TempData["ErrorMessage"] != null)
            {
                ErrorMessage = TempData["ErrorMessage"]?.ToString();
            }

            return Page();
        }

        /// <summary>
        /// معالج طلب POST - التسجيل
        /// POST request handler - register
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // التحقق من صلاحية المدير لتعيين الأدوار
                // Check admin permission for role assignment
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                var currentUserRole = HttpContext.Session.GetString("UserRole");
                IsCurrentUserAdmin = currentUserId.HasValue && currentUserRole == UserRole.Administrator.ToString();

                // إذا لم يكن مدير، تعيين الدور كمستخدم عادي
                // If not admin, set role as regular user
                if (!IsCurrentUserAdmin)
                {
                    RegisterData.Role = UserRole.User;
                }

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                _logger.LogDebug("محاولة تسجيل مستخدم جديد: {Email} - Attempting to register new user: {Email}",
                    RegisterData.Email, RegisterData.Email);

                var result = await _authenticationService.RegisterAsync(RegisterData);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم تسجيل مستخدم جديد بنجاح: {Email}, UserId: {UserId} - Successfully registered new user: {Email}, UserId: {UserId}",
                        RegisterData.Email, result.Data, RegisterData.Email, result.Data);

                    TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح! يمكنك الآن تسجيل الدخول - Account created successfully! You can now login";

                    // إذا كان مدير يسجل مستخدم جديد، البقاء في نفس الصفحة
                    // If admin is registering a new user, stay on the same page
                    if (IsCurrentUserAdmin)
                    {
                        // مسح النموذج للتسجيل التالي
                        // Clear form for next registration
                        RegisterData = new RegisterDto();
                        return RedirectToPage();
                    }
                    else
                    {
                        // إعادة توجيه لصفحة تسجيل الدخول
                        // Redirect to login page
                        return RedirectToPage("/Auth/Login");
                    }
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "فشل في إنشاء الحساب - Failed to create account";
                    _logger.LogWarning("فشل تسجيل المستخدم: {Email} - Registration failed for user: {Email}",
                        RegisterData.Email, RegisterData.Email);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تسجيل المستخدم - Error during user registration");
                ErrorMessage = "حدث خطأ أثناء إنشاء الحساب - An error occurred during account creation";
                return Page();
            }
        }
    }
}
