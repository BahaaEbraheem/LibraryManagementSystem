namespace LibraryManagementSystem.DAL.Models.DTOs
{
    /// <summary>
    /// كائن نقل البيانات لعمليات البحث عن الكتب
    /// Data Transfer Object for book search operations
    /// </summary>
    public class BookSearchDto
    {
        /// <summary>عنوان الكتاب - Book title</summary>
        public string? Title { get; set; }

        /// <summary>مؤلف الكتاب - Book author</summary>
        public string? Author { get; set; }

        /// <summary>الرقم المعياري الدولي للكتاب - ISBN</summary>
        public string? ISBN { get; set; }

        /// <summary>نوع الكتاب - Book genre</summary>
        public string? Genre { get; set; }

        /// <summary>حالة التوفر - Availability status</summary>
        public bool? IsAvailable { get; set; }

        /// <summary>رقم الصفحة - Page number</summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>حجم الصفحة - Page size</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>ترتيب حسب - Sort by field</summary>
        public string SortBy { get; set; } = "Title";

        /// <summary>ترتيب تنازلي - Sort descending</summary>
        public bool SortDescending { get; set; } = false;
        public string? SearchTerm { get; set; }   // ✅ كلمة البحث العامة
        public bool AvailableOnly { get; set; } = false;

    }

    /// <summary>
    /// كائن نقل البيانات لنتائج البحث المقسمة على صفحات
    /// Data Transfer Object for paginated search results
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>عناصر الصفحة الحالية - Current page items</summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>إجمالي عدد العناصر - Total count of items</summary>
        public int TotalCount { get; set; }

        /// <summary>رقم الصفحة الحالية - Current page number</summary>
        public int PageNumber { get; set; }

        /// <summary>حجم الصفحة - Page size</summary>
        public int PageSize { get; set; }

        /// <summary>إجمالي عدد الصفحات - Total number of pages</summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>وجود صفحة سابقة - Has previous page</summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>وجود صفحة تالية - Has next page</summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// كائن نقل البيانات لعمليات الاستعارة
    /// Data Transfer Object for borrowing operations
    /// </summary>
    public class BorrowingDto
    {
        /// <summary>معرف المستخدم - User ID</summary>
        public int UserId { get; set; }

        /// <summary>معرف الكتاب - Book ID</summary>
        public int BookId { get; set; }

        /// <summary>تاريخ الاستحقاق - Due date</summary>
        public DateTime? DueDate { get; set; }

        /// <summary>ملاحظات - Notes</summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// كائن نقل البيانات لعمليات الإرجاع
    /// Data Transfer Object for return operations
    /// </summary>
    public class ReturnDto
    {
        /// <summary>معرف عملية الاستعارة - Borrowing ID</summary>
        public int BorrowingId { get; set; }

        /// <summary>تاريخ الإرجاع - Return date</summary>
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        /// <summary>رسوم التأخير - Late fee</summary>
        public decimal? LateFee { get; set; }

        /// <summary>ملاحظات - Notes</summary>
        public string? Notes { get; set; }
    }
}
