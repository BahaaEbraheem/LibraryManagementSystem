using LibraryManagementSystem.BLL.Validation;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// واجهة خدمة الكتب
    /// Book service interface
    /// </summary>
    public interface IBookService
    {
        /// <summary>
        /// الحصول على كتاب بالمعرف
        /// Get book by ID
        /// </summary>
        Task<ServiceResult<Book>> GetBookByIdAsync(int id);

        /// <summary>
        /// البحث عن الكتب
        /// Search books
        /// </summary>
        Task<ServiceResult<PagedResult<Book>>> SearchBooksAsync(BookSearchDto searchDto);

        /// <summary>
        /// الحصول على الكتب المتاحة
        /// Get available books
        /// </summary>
        Task<ServiceResult<IEnumerable<Book>>> GetAvailableBooksAsync();

        /// <summary>
        /// إضافة كتاب جديد مع التحقق من الصلاحيات
        /// Add a new book with authorization check
        /// </summary>
        Task<ServiceResult<int>> AddBookAsync(Book book, int userId);

        /// <summary>
        /// تحديث كتاب موجود مع التحقق من الصلاحيات
        /// Update an existing book with authorization check
        /// </summary>
        Task<ServiceResult<bool>> UpdateBookAsync(Book book, int userId);

        /// <summary>
        /// حذف كتاب مع التحقق من الصلاحيات
        /// Delete a book with authorization check
        /// </summary>
        Task<ServiceResult<bool>> DeleteBookAsync(int id, int userId);

        /// <summary>
        /// التحقق من توفر كتاب للاستعارة
        /// Check if book is available for borrowing
        /// </summary>
        Task<ServiceResult<bool>> IsBookAvailableAsync(int bookId);

        /// <summary>
        /// التحقق من صحة بيانات الكتاب
        /// Validate book data
        /// </summary>
        Task<ValidationResult> ValidateBookAsync(Book book, bool isUpdate = false);
    }
}
