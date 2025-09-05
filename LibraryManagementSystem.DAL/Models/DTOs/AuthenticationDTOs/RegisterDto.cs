using LibraryManagementSystem.DAL.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.DAL.Models.DTOs.AuthenticationDTOs
{
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
}
