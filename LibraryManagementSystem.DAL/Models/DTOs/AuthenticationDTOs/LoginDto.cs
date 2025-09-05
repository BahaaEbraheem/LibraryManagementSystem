using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.DAL.Models.DTOs.AuthenticationDTOs
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


}
