using LibraryManagementSystem.DAL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.DAL.Models.DTOs.AuthenticationDTOs
{
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
        public string Token { get; set; }

        /// <summary>
        /// هل المستخدم نشط
        /// Is user active
        /// </summary>
        public bool IsActive { get; set; }
    }
}
