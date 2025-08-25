using System.ComponentModel.DataAnnotations;
using LibraryManagementSystem.DAL.Models.Enums;

namespace LibraryManagementSystem.DAL.Models.DTOs
{
    /// <summary>
    /// نموذج تسجيل الدخول
    /// Login model
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// البريد الإلكتروني
        /// Email address
        /// </summary>
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب - Email is required")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح - Invalid email format")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// كلمة المرور
        /// Password
        /// </summary>
        [Required(ErrorMessage = "كلمة المرور مطلوبة - Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// تذكرني
        /// Remember me
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// نموذج تسجيل مستخدم جديد
    /// Register new user model
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// الاسم الأول
        /// First name
        /// </summary>
        [Required(ErrorMessage = "الاسم الأول مطلوب - First name is required")]
        [StringLength(50, ErrorMessage = "الاسم الأول يجب أن يكون أقل من 50 حرف - First name must be less than 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// الاسم الأخير
        /// Last name
        /// </summary>
        [Required(ErrorMessage = "الاسم الأخير مطلوب - Last name is required")]
        [StringLength(50, ErrorMessage = "الاسم الأخير يجب أن يكون أقل من 50 حرف - Last name must be less than 50 characters")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// البريد الإلكتروني
        /// Email address
        /// </summary>
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب - Email is required")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح - Invalid email format")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// رقم الهاتف
        /// Phone number
        /// </summary>
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح - Invalid phone number")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// العنوان
        /// Address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// كلمة المرور
        /// Password
        /// </summary>
        [Required(ErrorMessage = "كلمة المرور مطلوبة - Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون بين 6 و 100 حرف - Password must be between 6 and 100 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// تأكيد كلمة المرور
        /// Confirm password
        /// </summary>
        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب - Password confirmation is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقتين - Password and confirmation do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// دور المستخدم (للمديرين فقط)
        /// User role (for administrators only)
        /// </summary>
        public UserRole Role { get; set; } = UserRole.User;
    }

    /// <summary>
    /// نموذج تغيير كلمة المرور
    /// Change password model
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// كلمة المرور الحالية
        /// Current password
        /// </summary>
        [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة - Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// كلمة المرور الجديدة
        /// New password
        /// </summary>
        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة - New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون بين 6 و 100 حرف - Password must be between 6 and 100 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// تأكيد كلمة المرور الجديدة
        /// Confirm new password
        /// </summary>
        [Required(ErrorMessage = "تأكيد كلمة المرور الجديدة مطلوب - New password confirmation is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة وتأكيدها غير متطابقتين - New password and confirmation do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// نموذج المستخدم المسجل دخوله
    /// Logged in user model
    /// </summary>
    public class LoggedInUserDto
    {
        /// <summary>
        /// معرف المستخدم
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// الاسم الكامل
        /// Full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// البريد الإلكتروني
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// دور المستخدم
        /// User role
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// الصلاحيات
        /// Permissions
        /// </summary>
        public List<string> Permissions { get; set; } = new();

        /// <summary>
        /// هل المستخدم نشط
        /// Is user active
        /// </summary>
        public bool IsActive { get; set; }
    }
}
