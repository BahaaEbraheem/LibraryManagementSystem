using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.DAL.Models
{
    /// <summary>
    /// يمثل كتاب في نظام المكتبة
    /// Represents a book in the library system
    /// </summary>
    public class Book
    {
        /// <summary>
        /// معرف الكتاب الفريد
        /// Unique book identifier
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// عنوان الكتاب
        /// Book title
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// مؤلف الكتاب
        /// Book author
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// الرقم المعياري الدولي للكتاب
        /// International Standard Book Number
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// ناشر الكتاب
        /// Book publisher
        /// </summary>
        [StringLength(100)]
        public string? Publisher { get; set; }

        /// <summary>
        /// سنة النشر
        /// Publication year
        /// </summary>
        [Range(1000, 9999)]
        public int? PublicationYear { get; set; }

        /// <summary>
        /// نوع الكتاب أو التصنيف
        /// Book genre or category
        /// </summary>
        [StringLength(50)]
        public string? Genre { get; set; }

        /// <summary>
        /// إجمالي عدد النسخ
        /// Total number of copies
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TotalCopies { get; set; }

        /// <summary>
        /// عدد النسخ المتاحة
        /// Number of available copies
        /// </summary>
        [Range(0, int.MaxValue)]
        public int AvailableCopies { get; set; }

        /// <summary>
        /// وصف الكتاب
        /// Book description
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

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
        /// خاصية محسوبة لتحديد ما إذا كان الكتاب متاحاً
        /// Computed property to check if book is available
        /// </summary>
        public bool IsAvailable => AvailableCopies > 0;

        /// <summary>
        /// خاصية محسوبة لعدد النسخ المستعارة
        /// Computed property for number of borrowed copies
        /// </summary>
        public int BorrowedCopies => TotalCopies - AvailableCopies;

        /// <summary>
        /// خاصية محسوبة لعرض عنوان الكتاب مع المؤلف
        /// Computed property for displaying title with author
        /// </summary>
        public string DisplayTitle => $"{Title} by {Author}";

        /// <summary>
        /// خاصية محسوبة لحالة التوفر
        /// Computed property for availability status
        /// </summary>
        public string AvailabilityStatus => IsAvailable
            ? $"Available ({AvailableCopies} of {TotalCopies})"
            : "Not Available";
    }
}
