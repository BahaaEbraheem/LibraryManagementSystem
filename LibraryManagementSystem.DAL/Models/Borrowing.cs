using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DAL.Models
{
    /// <summary>
    /// يمثل عملية استعارة في نظام المكتبة
    /// Represents a borrowing transaction in the library system
    /// </summary>
    public class Borrowing
    {
        /// <summary>
        /// معرف عملية الاستعارة الفريد
        /// Unique borrowing transaction identifier
        /// </summary>
        public int BorrowingId { get; set; }

        /// <summary>
        /// معرف المستخدم المستعير
        /// ID of the borrowing user
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// معرف الكتاب المستعار
        /// ID of the borrowed book
        /// </summary>
        [Required]
        public int BookId { get; set; }

        /// <summary>
        /// تاريخ الاستعارة
        /// Date when book was borrowed
        /// </summary>
        public DateTime BorrowDate { get; set; }

        /// <summary>
        /// تاريخ الاستحقاق المتوقع للإرجاع
        /// Expected return due date
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// تاريخ الإرجاع الفعلي
        /// Actual return date
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// حالة ما إذا كان الكتاب تم إرجاعه
        /// Whether the book has been returned
        /// </summary>
        public bool IsReturned { get; set; }

        /// <summary>
        /// رسوم التأخير
        /// Late return fee
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal LateFee { get; set; }

        /// <summary>
        /// ملاحظات إضافية
        /// Additional notes
        /// </summary>
        [StringLength(200)]
        public string? Notes { get; set; }

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
        /// خاصية التنقل للمستخدم (لأغراض العرض)
        /// Navigation property for user (for display purposes)
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// خاصية التنقل للكتاب (لأغراض العرض)
        /// Navigation property for book (for display purposes)
        /// </summary>
        public Book? Book { get; set; }

        /// <summary>
        /// خاصية محسوبة لتحديد ما إذا كانت الاستعارة متأخرة
        /// Computed property to check if borrowing is overdue
        /// </summary>
        public bool IsOverdue => !IsReturned && DateTime.Now > DueDate;

        /// <summary>
        /// خاصية محسوبة لعدد أيام التأخير
        /// Computed property for number of overdue days
        /// </summary>
        public int DaysOverdue => IsOverdue ? (DateTime.Now - DueDate).Days : 0;

        /// <summary>
        /// خاصية محسوبة لعدد الأيام المتبقية
        /// Computed property for remaining days
        /// </summary>
        public int DaysRemaining => IsReturned ? 0 : Math.Max(0, (DueDate - DateTime.Now).Days);

        /// <summary>
        /// خاصية محسوبة لحالة الاستعارة
        /// Computed property for borrowing status
        /// </summary>
        public string Status => IsReturned ? "Returned" : (IsOverdue ? "Overdue" : "Active");

        /// <summary>
        /// خاصية محسوبة لفترة الاستعارة
        /// Computed property for borrowing period
        /// </summary>
        public string BorrowingPeriod => $"{BorrowDate:MMM dd, yyyy} - {(IsReturned ? ReturnDate?.ToString("MMM dd, yyyy") : DueDate.ToString("MMM dd, yyyy"))}";
    }
}
