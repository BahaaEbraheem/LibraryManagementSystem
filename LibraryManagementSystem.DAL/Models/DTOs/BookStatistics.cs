namespace LibraryManagementSystem.DAL.Models.DTOs
{
    /// <summary>
    /// إحصائيات الكتب
    /// Book statistics
    /// </summary>
    public class BookStatistics
    {
        /// <summary>
        /// إجمالي عدد الكتب
        /// Total number of books
        /// </summary>
        public int TotalBooks { get; set; }

        /// <summary>
        /// عدد الكتب المتاحة
        /// Number of available books
        /// </summary>
        public int AvailableBooks { get; set; }

        /// <summary>
        /// عدد الكتب المستعارة
        /// Number of borrowed books
        /// </summary>
        public int BorrowedBooks { get; set; }

        /// <summary>
        /// إجمالي النسخ
        /// Total copies
        /// </summary>
        public int TotalCopies { get; set; }

        /// <summary>
        /// النسخ المتاحة
        /// Available copies
        /// </summary>
        public int AvailableCopies { get; set; }

        /// <summary>
        /// عدد المؤلفين المختلفين
        /// Number of different authors
        /// </summary>
        public int UniqueAuthors { get; set; }

        /// <summary>
        /// عدد الأنواع المختلفة
        /// Number of different genres
        /// </summary>
        public int UniqueGenres { get; set; }

        /// <summary>
        /// الكتب المضافة هذا الشهر
        /// Books added this month
        /// </summary>
        public int BooksAddedThisMonth { get; set; }
    }
}
