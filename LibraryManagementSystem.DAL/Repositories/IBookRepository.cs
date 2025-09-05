using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs;

namespace LibraryManagementSystem.DAL.Repositories
{
    /// <summary>
    /// واجهة مستودع الكتب
    /// Interface for book repository
    /// </summary>
    public interface IBookRepository
    {
        /// <summary>
        /// الحصول على جميع الكتب
        /// Get all books
        /// </summary>
        /// <returns>قائمة بجميع الكتب - List of all books</returns>
        Task<IEnumerable<Book>> GetAllAsync();

        /// <summary>
        /// الحصول على كتاب بالمعرف
        /// Get book by ID
        /// </summary>
        /// <param name="id">معرف الكتاب - Book ID</param>
        /// <returns>الكتاب أو null - Book or null</returns>
        Task<Book?> GetByIdAsync(int id);

        /// <summary>
        /// الحصول على كتاب بالرقم المعياري الدولي
        /// Get book by ISBN
        /// </summary>
        /// <param name="isbn">الرقم المعياري الدولي - ISBN</param>
        /// <returns>الكتاب أو null - Book or null</returns>
        Task<Book?> GetByIsbnAsync(string isbn);

        /// <summary>
        /// البحث عن الكتب مع التقسيم على صفحات
        /// Search books with pagination
        /// </summary>
        /// <param name="searchDto">معايير البحث - Search criteria</param>
        /// <returns>نتائج البحث المقسمة - Paginated search results</returns>
        Task<PagedResult<Book>> SearchAsync(BookSearchDto searchDto);

        /// <summary>
        /// الحصول على الكتب المتاحة
        /// Get available books
        /// </summary>
        /// <returns>قائمة بالكتب المتاحة - List of available books</returns>
        Task<IEnumerable<Book>> GetAvailableAsync();

        /// <summary>
        /// الحصول على الكتب حسب المؤلف
        /// Get books by author
        /// </summary>
        /// <param name="author">اسم المؤلف - Author name</param>
        /// <returns>قائمة بكتب المؤلف - List of author's books</returns>
        Task<IEnumerable<Book>> GetByAuthorAsync(string author);

        /// <summary>
        /// الحصول على الكتب حسب النوع
        /// Get books by genre
        /// </summary>
        /// <param name="genre">نوع الكتاب - Book genre</param>
        /// <returns>قائمة بكتب النوع - List of books in genre</returns>
        Task<IEnumerable<Book>> GetByGenreAsync(string genre);

        /// <summary>
        /// إضافة كتاب جديد
        /// Add a new book
        /// </summary>
        /// <param name="book">بيانات الكتاب - Book data</param>
        /// <returns>معرف الكتاب الجديد - New book ID</returns>
        Task<int> AddAsync(Book book);

        /// <summary>
        /// تحديث بيانات كتاب
        /// Update book data
        /// </summary>
        /// <param name="book">بيانات الكتاب المحدثة - Updated book data</param>
        /// <returns>true إذا تم التحديث بنجاح - true if update successful</returns>
        Task<bool> UpdateAsync(Book book);

        /// <summary>
        /// حذف كتاب
        /// Delete a book
        /// </summary>
        /// <param name="id">معرف الكتاب - Book ID</param>
        /// <returns>true إذا تم الحذف بنجاح - true if deletion successful</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// تحديث عدد النسخ المتاحة
        /// Update available copies count
        /// </summary>
        /// <param name="bookId">معرف الكتاب - Book ID</param>
        /// <param name="change">التغيير في العدد (موجب أو سالب) - Change in count (positive or negative)</param>
        /// <returns>true إذا تم التحديث بنجاح - true if update successful</returns>
        Task<bool> UpdateAvailableCopiesAsync(int bookId, int change);

        /// <summary>
        /// التحقق من توفر كتاب للاستعارة
        /// Check if book is available for borrowing
        /// </summary>
        /// <param name="bookId">معرف الكتاب - Book ID</param>
        /// <returns>true إذا كان متاحاً - true if available</returns>
        Task<bool> IsAvailableAsync(int bookId);
        /// <summary>
        /// الحصول على إحصائيات الكتب
        /// Get book statistics
        /// </summary>
        /// <returns>إحصائيات الكتب - Book statistics</returns>
        Task<BookStatistics> GetBookStatisticsAsync();
    }
}
