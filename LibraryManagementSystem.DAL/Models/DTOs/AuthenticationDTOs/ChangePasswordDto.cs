using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.DAL.Models.DTOs.AuthenticationDTOs
{
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
}
