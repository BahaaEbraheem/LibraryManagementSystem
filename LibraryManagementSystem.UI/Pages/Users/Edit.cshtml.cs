using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.UI.Pages.Users
{
    /// <summary>
    /// نموذج صفحة تعديل المستخدم
    /// Edit user page model
    /// </summary>
    public class EditModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<EditModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public EditModel(IUserService userService, ILogger<EditModel> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// نموذج إدخال المستخدم
        /// User input model
        /// </summary>
        [BindProperty]
        public UserEditModel Input { get; set; } = new UserEditModel();

        /// <summary>
        /// رسالة الخطأ في حالة حدوث مشكلة
        /// Error message in case of issues
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// معالج طلب GET - تحميل بيانات المستخدم للتعديل
        /// GET request handler - load user data for editing
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // التحقق من أن المستخدم مدير
                // Check if user is admin
                if (HttpContext.Session.GetString("UserRole") != "Administrator")
                {
                    return RedirectToPage("/Account/AccessDenied");
                }

                if (id <= 0)
                {
                    ErrorMessage = "معرف المستخدم غير صحيح - Invalid user ID";
                    return Page();
                }

                _logger.LogDebug("تحميل بيانات المستخدم {UserId} للتعديل - Loading user data for editing", id);

                var userResult = await _userService.GetUserByIdAsync(id);

                if (userResult.IsSuccess && userResult.Data != null)
                {
                    var user = userResult.Data;
                    Input = new UserEditModel
                    {
                        UserId = user.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        IsActive = user.IsActive,
                        Role = user.Role
                    };

                    _logger.LogInformation("تم تحميل بيانات المستخدم {UserId} للتعديل بنجاح - Successfully loaded user data for editing", id);
                }
                else
                {
                    ErrorMessage = userResult.ErrorMessage ?? "لم يتم العثور على المستخدم - User not found";
                    _logger.LogWarning("فشل في تحميل بيانات المستخدم {UserId}: المستخدم غير موجود - Failed to load user data: User not found", id);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل بيانات المستخدم {UserId} للتعديل - Error loading user data for editing", id);
                ErrorMessage = "حدث خطأ أثناء تحميل بيانات المستخدم - An error occurred while loading user data";
                return Page();
            }
        }

        /// <summary>
        /// معالج طلب POST - تحديث بيانات المستخدم
        /// POST request handler - update user data
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // التحقق من أن المستخدم مدير
                // Check if user is admin
                if (HttpContext.Session.GetString("UserRole") != "Administrator")
                {
                    return RedirectToPage("/Account/AccessDenied");
                }

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                _logger.LogDebug("تحديث بيانات المستخدم {UserId}: {UserName} - Updating user data", Input.UserId, Input.FirstName + " " + Input.LastName);

                // إنشاء كائن المستخدم المحدث
                // Create updated user object
                var user = new User
                {
                    UserId = Input.UserId,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    Address = Input.Address,
                    IsActive = Input.IsActive,
                    Role = Input.Role,
                    ModifiedDate = DateTime.Now
                };

                var result = await _userService.UpdateUserAsync(user);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم تحديث بيانات المستخدم {UserId} بنجاح - Successfully updated user data", Input.UserId);

                    TempData["SuccessMessage"] = "تم تحديث بيانات المستخدم بنجاح! - User data updated successfully!";
                    return RedirectToPage("/Users/Index");
                }
                else
                {
                    ErrorMessage = "فشل في تحديث بيانات المستخدم - Failed to update user data";
                    _logger.LogWarning("فشل في تحديث بيانات المستخدم {UserId} - Failed to update user data", Input.UserId);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث بيانات المستخدم {UserId} - Error updating user data", Input.UserId);
                ErrorMessage = "حدث خطأ أثناء تحديث بيانات المستخدم - An error occurred while updating user data";
                return Page();
            }
        }
    }

    /// <summary>
    /// نموذج تعديل بيانات المستخدم
    /// User edit data model
    /// </summary>
    public class UserEditModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "الاسم الأول مطلوب - First name is required")]
        [StringLength(50, ErrorMessage = "الاسم الأول يجب أن يكون أقل من 50 حرف - First name must be less than 50 characters")]
        [Display(Name = "الاسم الأول - First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم الأخير مطلوب - Last name is required")]
        [StringLength(50, ErrorMessage = "الاسم الأخير يجب أن يكون أقل من 50 حرف - Last name must be less than 50 characters")]
        [Display(Name = "الاسم الأخير - Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب - Email is required")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح - Invalid email address")]
        [StringLength(100, ErrorMessage = "البريد الإلكتروني يجب أن يكون أقل من 100 حرف - Email must be less than 100 characters")]
        [Display(Name = "البريد الإلكتروني - Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "رقم الهاتف غير صحيح - Invalid phone number")]
        [StringLength(15, ErrorMessage = "رقم الهاتف يجب أن يكون أقل من 15 حرف - Phone number must be less than 15 characters")]
        [Display(Name = "رقم الهاتف - Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "العنوان يجب أن يكون أقل من 200 حرف - Address must be less than 200 characters")]
        [Display(Name = "العنوان - Address")]
        public string? Address { get; set; }

        [Display(Name = "نشط - Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "الدور - Role")]
        public UserRole Role { get; set; } = UserRole.User;
    }
}
