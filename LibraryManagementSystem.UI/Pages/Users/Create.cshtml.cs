using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Models.Enums;
using LibraryManagementSystem.UI.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.UI.Pages.Users
{
    /// <summary>
    /// نموذج صفحة إنشاء مستخدم جديد
    /// Create new user page model
    /// </summary>
    [AuthorizeRole(UserRole.Administrator)]
    public class CreateModel : PageModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<CreateModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public CreateModel(IAuthenticationService authenticationService, ILogger<CreateModel> logger)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// نموذج إدخال المستخدم الجديد
        /// New user input model
        /// </summary>
        [BindProperty]
        public UserCreateModel Input { get; set; } = new UserCreateModel();

        /// <summary>
        /// رسالة الخطأ في حالة حدوث مشكلة
        /// Error message in case of issues
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// رسالة النجاح
        /// Success message
        /// </summary>
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// معالج طلب GET - تحميل صفحة إنشاء المستخدم
        /// GET request handler - load create user page
        /// </summary>
        public IActionResult OnGet()
        {
            try
            {
                // التحقق من أن المستخدم مدير
                // Check if user is admin
                if (HttpContext.Session.GetString("UserRole") != "Administrator")
                {
                    return RedirectToPage("/Auth/AccessDenied");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل صفحة إنشاء المستخدم - Error loading create user page");
                ErrorMessage = "حدث خطأ أثناء تحميل الصفحة - An error occurred while loading the page";
                return Page();
            }
        }

        /// <summary>
        /// معالج طلب POST - إنشاء المستخدم الجديد
        /// POST request handler - create new user
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // التحقق من أن المستخدم مدير
                // Check if user is admin
                if (HttpContext.Session.GetString("UserRole") != "Administrator")
                {
                    return RedirectToPage("/Auth/AccessDenied");
                }

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                _logger.LogDebug("محاولة إنشاء مستخدم جديد: {Email} - Attempting to create new user: {Email}",
                    Input.Email, Input.Email);

                // تحويل النموذج إلى RegisterDto
                // Convert model to RegisterDto
                var registerDto = new RegisterDto
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    Address = Input.Address,
                    Password = Input.Password,
                    ConfirmPassword = Input.ConfirmPassword,
                    Role = Input.Role
                };

                var result = await _authenticationService.RegisterAsync(registerDto);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم إنشاء مستخدم جديد بنجاح: {Email}, UserId: {UserId} - Successfully created new user: {Email}, UserId: {UserId}",
                        Input.Email, result.Data, Input.Email, result.Data);

                    TempData["SuccessMessage"] = "تم إنشاء المستخدم بنجاح! - User created successfully!";

                    // إعادة توجيه لصفحة تفاصيل المستخدم الجديد
                    // Redirect to new user details page
                    return RedirectToPage("/Users/Details", new { id = result.Data });
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "فشل في إنشاء المستخدم - Failed to create user";
                    _logger.LogWarning("فشل إنشاء المستخدم: {Email} - User creation failed: {Email}",
                        Input.Email, Input.Email);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إنشاء المستخدم - Error creating user");
                ErrorMessage = "حدث خطأ أثناء إنشاء المستخدم - An error occurred while creating the user";
                return Page();
            }
        }
    }

    /// <summary>
    /// نموذج إنشاء مستخدم جديد
    /// Create new user model
    /// </summary>
    public class UserCreateModel
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب - First name is required")]
        [StringLength(50, ErrorMessage = "الاسم الأول يجب أن يكون أقل من 50 حرف - First name must be less than 50 characters")]
        [Display(Name = "الاسم الأول - First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم العائلة مطلوب - Last name is required")]
        [StringLength(50, ErrorMessage = "اسم العائلة يجب أن يكون أقل من 50 حرف - Last name must be less than 50 characters")]
        [Display(Name = "اسم العائلة - Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب - Email is required")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح - Invalid email format")]
        [StringLength(100, ErrorMessage = "البريد الإلكتروني يجب أن يكون أقل من 100 حرف - Email must be less than 100 characters")]
        [Display(Name = "البريد الإلكتروني - Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "رقم الهاتف غير صحيح - Invalid phone number")]
        [StringLength(15, ErrorMessage = "رقم الهاتف يجب أن يكون أقل من 15 رقم - Phone number must be less than 15 digits")]
        [Display(Name = "رقم الهاتف - Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "العنوان يجب أن يكون أقل من 200 حرف - Address must be less than 200 characters")]
        [Display(Name = "العنوان - Address")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة - Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون بين 6 و 100 حرف - Password must be between 6 and 100 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور - Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب - Confirm password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور - Confirm Password")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقتين - Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "الدور - Role")]
        public UserRole Role { get; set; } = UserRole.User;
    }
}
