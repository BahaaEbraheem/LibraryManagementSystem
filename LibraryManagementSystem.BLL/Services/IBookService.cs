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
        /// الحصول على جميع الكتب
        /// Get all books
        /// </summary>
        Task<ServiceResult<IEnumerable<Book>>> GetAllBooksAsync();

        /// <summary>
        /// الحصول على كتاب بالمعرف
        /// Get book by ID
        /// </summary>
        Task<ServiceResult<Book>> GetBookByIdAsync(int id);

        /// <summary>
        /// الحصول على كتاب بالرقم المعياري
        /// Get book by ISBN
        /// </summary>
        Task<ServiceResult<Book>> GetBookByIsbnAsync(string isbn);

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
        /// إضافة كتاب جديد
        /// Add a new book
        /// </summary>
        Task<ServiceResult<int>> AddBookAsync(Book book);

        /// <summary>
        /// إضافة كتاب جديد مع التحقق من الصلاحيات
        /// Add a new book with authorization check
        /// </summary>
        Task<ServiceResult<int>> AddBookAsync(Book book, int userId);

        /// <summary>
        /// تحديث كتاب موجود
        /// Update an existing book
        /// </summary>
        Task<ServiceResult<bool>> UpdateBookAsync(Book book);

        /// <summary>
        /// تحديث كتاب موجود مع التحقق من الصلاحيات
        /// Update an existing book with authorization check
        /// </summary>
        Task<ServiceResult<bool>> UpdateBookAsync(Book book, int userId);

        /// <summary>
        /// حذف كتاب
        /// Delete a book
        /// </summary>
        Task<ServiceResult<bool>> DeleteBookAsync(int id);

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
        /// الحصول على إحصائيات الكتب
        /// Get book statistics
        /// </summary>
        Task<ServiceResult<BookStatistics>> GetBookStatisticsAsync();

        /// <summary>
        /// التحقق من صحة بيانات الكتاب
        /// Validate book data
        /// </summary>
        Task<ValidationResult> ValidateBookAsync(Book book, bool isUpdate = false);
    }

    /// <summary>
    /// نتيجة العملية مع البيانات
    /// Service operation result with data
    /// </summary>
    public class ServiceResult<T>
    {
        /// <summary>
        /// هل العملية نجحت
        /// Whether the operation succeeded
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// البيانات المرجعة
        /// Returned data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// رسالة الخطأ
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// رسائل التحقق من الصحة
        /// Validation messages
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new();

        /// <summary>
        /// إنشاء نتيجة ناجحة
        /// Create successful result
        /// </summary>
        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        /// <summary>
        /// إنشاء نتيجة فاشلة
        /// Create failed result
        /// </summary>
        public static ServiceResult<T> Failure(string errorMessage)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// إنشاء نتيجة فاشلة مع أخطاء التحقق
        /// Create failed result with validation errors
        /// </summary>
        public static ServiceResult<T> ValidationFailure(List<string> validationErrors)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                ValidationErrors = validationErrors
            };
        }
    }

    /// <summary>
    /// نتيجة التحقق من صحة البيانات
    /// Data validation result
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// هل البيانات صحيحة
        /// Whether the data is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// رسائل الأخطاء
        /// Error messages
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// إنشاء نتيجة صحيحة
        /// Create valid result
        /// </summary>
        public static ValidationResult Valid()
        {
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// إنشاء نتيجة غير صحيحة
        /// Create invalid result
        /// </summary>
        public static ValidationResult Invalid(List<string> errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors
            };
        }

        /// <summary>
        /// إضافة خطأ
        /// Add error
        /// </summary>
        public void AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
        }
    }


}
