using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages.Auth
{
    /// <summary>
    /// نموذج صفحة تسجيل الدخول
    /// Login page model
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthenticationService authenticationService, IJwtService jwtService, ILogger<LoginModel> logger)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// نموذج تسجيل الدخول
        /// Login model
        /// </summary>
        [BindProperty]
        public LoginDto LoginData { get; set; } = new LoginDto();

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
        /// رابط الإرجاع
        /// Return URL
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// معالج طلب GET - تحميل صفحة تسجيل الدخول
        /// GET request handler - load login page
        /// </summary>
        public IActionResult OnGet()
        {
            // التحقق من تسجيل الدخول المسبق
            // Check if already logged in
            if (HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToPage("/Index");
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
        /// معالج طلب POST - تسجيل الدخول
        /// POST request handler - login
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                _logger.LogDebug("محاولة تسجيل دخول للمستخدم: {Email} - Login attempt for user: {Email}",
                    LoginData.Email, LoginData.Email);

                var result = await _authenticationService.LoginAsync(LoginData);

                if (result.IsSuccess && result.Data != null)
                {
                    var user = result.Data;

                    // إنشاء رمز JWT
                    // Generate JWT token
                    var jwtToken = _jwtService.GenerateToken(user);

                    // حفظ بيانات المستخدم في الجلسة
                    // Save user data in session
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role.ToString());

                    // حفظ JWT في الجلسة والكوكيز
                    // Save JWT in session and cookies
                    HttpContext.Session.SetString("jwt", jwtToken);

                    // إضافة JWT كـ cookie (اختياري - للأمان أكثر)
                    // Add JWT as cookie (optional - for better security)
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true, // منع الوصول من JavaScript
                        Secure = true,   // HTTPS فقط
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(1) // نفس مدة انتهاء JWT
                    };
                    Response.Cookies.Append("jwt", jwtToken, cookieOptions);

                    _logger.LogInformation("تم تسجيل الدخول بنجاح للمستخدم: {Email} - Successful login for user: {Email}",
                        LoginData.Email, LoginData.Email);

                    // إعادة التوجيه
                    // Redirect
                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }

                    // إعادة التوجيه إلى صفحة الكتب للجميع
                    // Redirect to Books page for everyone
                    return RedirectToPage("/Books/Index");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "فشل في تسجيل الدخول - Login failed";
                    _logger.LogWarning("فشل تسجيل الدخول للمستخدم: {Email} - Login failed for user: {Email}",
                        LoginData.Email, LoginData.Email);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تسجيل الدخول - Error during login");
                ErrorMessage = "حدث خطأ أثناء تسجيل الدخول - An error occurred during login";
                return Page();
            }
        }
    }
}
