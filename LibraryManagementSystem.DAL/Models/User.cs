using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DAL.Models
{
    /// <summary>
    /// يمثل مستخدم في نظام المكتبة
    /// Represents a user in the library system
    /// </summary>
    public class User
    {
        /// <summary>
        /// معرف المستخدم الفريد
        /// Unique user identifier
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// الاسم الأول للمستخدم
        /// User's first name
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// اسم العائلة للمستخدم
        /// User's last name
        /// </summary>
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// البريد الإلكتروني للمستخدم
        /// User's email address
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// رقم الهاتف للمستخدم
        /// User's phone number
        /// </summary>
        [Phone]
        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// عنوان المستخدم
        /// User's address
        /// </summary>
        [StringLength(200)]
        public string? Address { get; set; }

        /// <summary>
        /// تاريخ انضمام المستخدم للمكتبة
        /// Date when user joined the library
        /// </summary>
        public DateTime MembershipDate { get; set; }

        /// <summary>
        /// حالة نشاط المستخدم
        /// User's active status
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تاريخ إنشاء السجل
        /// Record creation date
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// تاريخ آخر تعديل للسجل
        /// Record last modification date
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// خاصية محسوبة للاسم الكامل
        /// Computed property for full name display
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";
    }
}
