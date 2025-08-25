using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// تنفيذ خدمة الكتب
    /// Book service implementation
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<BookService> _logger;

        public BookService(
            IBookRepository bookRepository,
            IAuthorizationService authorizationService,
            ILogger<BookService> logger)
        {
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على جميع الكتب
        /// Get all books
        /// </summary>
        public async Task<ServiceResult<IEnumerable<Book>>> GetAllBooksAsync()
        {
            try
            {
                _logger.LogDebug("بدء الحصول على جميع الكتب - Starting to get all books");

                var books = await _bookRepository.GetAllAsync();

                _logger.LogDebug("تم الحصول على {Count} كتاب بنجاح - Successfully retrieved {Count} books", books.Count(), books.Count());
                return ServiceResult<IEnumerable<Book>>.Success(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على جميع الكتب - Error getting all books");
                return ServiceResult<IEnumerable<Book>>.Failure("حدث خطأ أثناء الحصول على الكتب - An error occurred while retrieving books");
            }
        }

        /// <summary>
        /// الحصول على كتاب بالمعرف
        /// Get book by ID
        /// </summary>
        public async Task<ServiceResult<Book>> GetBookByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("تم تمرير معرف كتاب غير صحيح: {BookId} - Invalid book ID provided: {BookId}", id, id);
                    return ServiceResult<Book>.Failure("معرف الكتاب غير صحيح - Invalid book ID");
                }

                _logger.LogDebug("بدء البحث عن الكتاب بالمعرف: {BookId} - Starting to search for book with ID: {BookId}", id, id);

                var book = await _bookRepository.GetByIdAsync(id);

                if (book == null)
                {
                    _logger.LogWarning("لم يتم العثور على كتاب بالمعرف: {BookId} - Book not found with ID: {BookId}", id, id);
                    return ServiceResult<Book>.Failure("لم يتم العثور على الكتاب - Book not found");
                }

                _logger.LogDebug("تم العثور على الكتاب بنجاح: {BookTitle} - Successfully found book: {BookTitle}", book.Title, book.Title);
                return ServiceResult<Book>.Success(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الكتاب بالمعرف {BookId} - Error getting book by ID {BookId}", id, id);
                return ServiceResult<Book>.Failure("حدث خطأ أثناء البحث عن الكتاب - An error occurred while searching for the book");
            }
        }

        /// <summary>
        /// الحصول على كتاب بالرقم المعياري
        /// Get book by ISBN
        /// </summary>
        public async Task<ServiceResult<Book>> GetBookByIsbnAsync(string isbn)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(isbn))
                {
                    _logger.LogWarning("تم تمرير رقم معياري فارغ - Empty ISBN provided");
                    return ServiceResult<Book>.Failure("الرقم المعياري مطلوب - ISBN is required");
                }

                // تنظيف الرقم المعياري
                // Clean ISBN
                isbn = CleanIsbn(isbn);

                if (!IsValidIsbn(isbn))
                {
                    _logger.LogWarning("تم تمرير رقم معياري غير صحيح: {ISBN} - Invalid ISBN provided: {ISBN}", isbn, isbn);
                    return ServiceResult<Book>.Failure("الرقم المعياري غير صحيح - Invalid ISBN format");
                }

                _logger.LogDebug("بدء البحث عن الكتاب بالرقم المعياري: {ISBN} - Starting to search for book with ISBN: {ISBN}", isbn, isbn);

                var book = await _bookRepository.GetByIsbnAsync(isbn);

                if (book == null)
                {
                    _logger.LogWarning("لم يتم العثور على كتاب بالرقم المعياري: {ISBN} - Book not found with ISBN: {ISBN}", isbn, isbn);
                    return ServiceResult<Book>.Failure("لم يتم العثور على الكتاب - Book not found");
                }

                _logger.LogDebug("تم العثور على الكتاب بنجاح: {BookTitle} - Successfully found book: {BookTitle}", book.Title, book.Title);
                return ServiceResult<Book>.Success(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الكتاب بالرقم المعياري {ISBN} - Error getting book by ISBN {ISBN}", isbn, isbn);
                return ServiceResult<Book>.Failure("حدث خطأ أثناء البحث عن الكتاب - An error occurred while searching for the book");
            }
        }

        /// <summary>
        /// البحث عن الكتب
        /// Search books
        /// </summary>
        public async Task<ServiceResult<PagedResult<Book>>> SearchBooksAsync(BookSearchDto searchDto)
        {
            try
            {
                if (searchDto == null)
                {
                    _logger.LogWarning("تم تمرير معايير بحث فارغة - Empty search criteria provided");
                    return ServiceResult<PagedResult<Book>>.Failure("معايير البحث مطلوبة - Search criteria is required");
                }

                // التحقق من صحة معايير التقسيم فقط
                // Validate pagination criteria only
                var validationResult = ValidatePaginationCriteria(searchDto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("معايير التقسيم غير صحيحة - Invalid pagination criteria");
                    return ServiceResult<PagedResult<Book>>.ValidationFailure(validationResult.Errors);
                }

                var result = await _bookRepository.SearchAsync(searchDto);


                return ServiceResult<PagedResult<Book>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في البحث عن الكتب - Error searching books");
                return ServiceResult<PagedResult<Book>>.Failure("حدث خطأ أثناء البحث عن الكتب - An error occurred while searching for books");
            }
        }

        /// <summary>
        /// الحصول على الكتب المتاحة
        /// Get available books
        /// </summary>
        public async Task<ServiceResult<IEnumerable<Book>>> GetAvailableBooksAsync()
        {
            try
            {
                _logger.LogDebug("بدء الحصول على الكتب المتاحة - Starting to get available books");

                var books = await _bookRepository.GetAvailableAsync();

                _logger.LogDebug("تم الحصول على {Count} كتاب متاح - Successfully retrieved {Count} available books", books.Count(), books.Count());
                return ServiceResult<IEnumerable<Book>>.Success(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الكتب المتاحة - Error getting available books");
                return ServiceResult<IEnumerable<Book>>.Failure("حدث خطأ أثناء الحصول على الكتب المتاحة - An error occurred while retrieving available books");
            }
        }

        /// <summary>
        /// إضافة كتاب جديد
        /// Add a new book
        /// </summary>
        public async Task<ServiceResult<int>> AddBookAsync(Book book)
        {
            try
            {
                if (book == null)
                {
                    _logger.LogWarning("تم تمرير كتاب فارغ للإضافة - Null book provided for addition");
                    return ServiceResult<int>.Failure("بيانات الكتاب مطلوبة - Book data is required");
                }

                // التحقق من صحة بيانات الكتاب
                // Validate book data
                var validationResult = await ValidateBookAsync(book, false);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("بيانات الكتاب غير صحيحة للإضافة - Invalid book data for addition");
                    return ServiceResult<int>.ValidationFailure(validationResult.Errors);
                }

                // التحقق من عدم وجود كتاب بنفس الرقم المعياري
                // Check if book with same ISBN already exists
                var existingBook = await _bookRepository.GetByIsbnAsync(book.ISBN);
                if (existingBook != null)
                {
                    _logger.LogWarning("يوجد كتاب بنفس الرقم المعياري: {ISBN} - Book with same ISBN already exists: {ISBN}", book.ISBN, book.ISBN);
                    return ServiceResult<int>.Failure("يوجد كتاب بنفس الرقم المعياري - A book with the same ISBN already exists");
                }

                _logger.LogDebug("بدء إضافة كتاب جديد: {BookTitle} - Starting to add new book: {BookTitle}", book.Title, book.Title);

                var bookId = await _bookRepository.AddAsync(book);

                _logger.LogInformation("تم إضافة كتاب جديد بنجاح: {BookTitle} بالمعرف {BookId} - Successfully added new book",
                    book.Title, bookId);

                return ServiceResult<int>.Success(bookId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة كتاب جديد - Error adding new book");
                return ServiceResult<int>.Failure("حدث خطأ أثناء إضافة الكتاب - An error occurred while adding the book");
            }
        }

        /// <summary>
        /// تنظيف الرقم المعياري
        /// Clean ISBN
        /// </summary>
        private static string CleanIsbn(string isbn)
        {
            return Regex.Replace(isbn, @"[^\d]", "");
        }

        /// <summary>
        /// التحقق من صحة الرقم المعياري
        /// Validate ISBN format
        /// </summary>
        private static bool IsValidIsbn(string isbn)
        {
            return isbn.Length == 10 || isbn.Length == 13;
        }

        /// <summary>
        /// تحديث كتاب موجود
        /// Update an existing book
        /// </summary>
        public async Task<ServiceResult<bool>> UpdateBookAsync(Book book)
        {
            try
            {
                if (book == null)
                {
                    _logger.LogWarning("تم تمرير كتاب فارغ للتحديث - Null book provided for update");
                    return ServiceResult<bool>.Failure("بيانات الكتاب مطلوبة - Book data is required");
                }

                // التحقق من صحة بيانات الكتاب
                // Validate book data
                var validationResult = await ValidateBookAsync(book, true);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("بيانات الكتاب غير صحيحة للتحديث - Invalid book data for update");
                    return ServiceResult<bool>.ValidationFailure(validationResult.Errors);
                }

                // التحقق من وجود الكتاب
                // Check if book exists
                var existingBook = await _bookRepository.GetByIdAsync(book.BookId);
                if (existingBook == null)
                {
                    _logger.LogWarning("لم يتم العثور على الكتاب للتحديث: {BookId} - Book not found for update: {BookId}", book.BookId, book.BookId);
                    return ServiceResult<bool>.Failure("لم يتم العثور على الكتاب - Book not found");
                }

                // التحقق من عدم وجود كتاب آخر بنفس الرقم المعياري
                // Check if another book with same ISBN exists
                var bookWithSameIsbn = await _bookRepository.GetByIsbnAsync(book.ISBN);
                if (bookWithSameIsbn != null && bookWithSameIsbn.BookId != book.BookId)
                {
                    _logger.LogWarning("يوجد كتاب آخر بنفس الرقم المعياري: {ISBN} - Another book with same ISBN exists: {ISBN}", book.ISBN, book.ISBN);
                    return ServiceResult<bool>.Failure("يوجد كتاب آخر بنفس الرقم المعياري - Another book with the same ISBN already exists");
                }

                _logger.LogDebug("بدء تحديث الكتاب: {BookTitle} - Starting to update book: {BookTitle}", book.Title, book.Title);

                var success = await _bookRepository.UpdateAsync(book);

                if (success)
                {
                    _logger.LogInformation("تم تحديث الكتاب بنجاح: {BookTitle} - Successfully updated book: {BookTitle}", book.Title, book.Title);
                }
                else
                {
                    _logger.LogWarning("فشل في تحديث الكتاب: {BookTitle} - Failed to update book: {BookTitle}", book.Title, book.Title);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث الكتاب - Error updating book");
                return ServiceResult<bool>.Failure("حدث خطأ أثناء تحديث الكتاب - An error occurred while updating the book");
            }
        }

        /// <summary>
        /// حذف كتاب
        /// Delete a book
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteBookAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("تم تمرير معرف كتاب غير صحيح للحذف: {BookId} - Invalid book ID provided for deletion: {BookId}", id, id);
                    return ServiceResult<bool>.Failure("معرف الكتاب غير صحيح - Invalid book ID");
                }

                // التحقق من وجود الكتاب
                // Check if book exists
                var existingBook = await _bookRepository.GetByIdAsync(id);
                if (existingBook == null)
                {
                    _logger.LogWarning("لم يتم العثور على الكتاب للحذف: {BookId} - Book not found for deletion: {BookId}", id, id);
                    return ServiceResult<bool>.Failure("لم يتم العثور على الكتاب - Book not found");
                }

                // يمكن إضافة فحص إضافي هنا للتأكد من عدم وجود استعارات نشطة
                // Additional check can be added here to ensure no active borrowings exist

                _logger.LogDebug("بدء حذف الكتاب: {BookTitle} - Starting to delete book: {BookTitle}", existingBook.Title, existingBook.Title);

                var success = await _bookRepository.DeleteAsync(id);

                if (success)
                {
                    _logger.LogInformation("تم حذف الكتاب بنجاح: {BookTitle} - Successfully deleted book: {BookTitle}", existingBook.Title, existingBook.Title);
                }
                else
                {
                    _logger.LogWarning("فشل في حذف الكتاب: {BookTitle} - Failed to delete book: {BookTitle}", existingBook.Title, existingBook.Title);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الكتاب {BookId} - Error deleting book {BookId}", id, id);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء حذف الكتاب - An error occurred while deleting the book");
            }
        }

        /// <summary>
        /// التحقق من توفر كتاب للاستعارة
        /// Check if book is available for borrowing
        /// </summary>
        public async Task<ServiceResult<bool>> IsBookAvailableAsync(int bookId)
        {
            try
            {
                if (bookId <= 0)
                {
                    _logger.LogWarning("تم تمرير معرف كتاب غير صحيح: {BookId} - Invalid book ID provided: {BookId}", bookId, bookId);
                    return ServiceResult<bool>.Failure("معرف الكتاب غير صحيح - Invalid book ID");
                }

                _logger.LogDebug("فحص توفر الكتاب للاستعارة: {BookId} - Checking book availability for borrowing: {BookId}", bookId, bookId);

                var book = await _bookRepository.GetByIdAsync(bookId);

                if (book == null)
                {
                    _logger.LogWarning("لم يتم العثور على الكتاب: {BookId} - Book not found: {BookId}", bookId, bookId);
                    return ServiceResult<bool>.Failure("لم يتم العثور على الكتاب - Book not found");
                }

                var isAvailable = book.AvailableCopies > 0;

                _logger.LogDebug("حالة توفر الكتاب {BookId}:  Book {BookId} availability status",
                    bookId, isAvailable);

                return ServiceResult<bool>.Success(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في فحص توفر الكتاب {BookId} - Error checking book availability {BookId}", bookId, bookId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء فحص توفر الكتاب - An error occurred while checking book availability");
            }
        }

        /// <summary>
        /// الحصول على إحصائيات الكتب
        /// Get book statistics
        /// </summary>
        public async Task<ServiceResult<BookStatistics>> GetBookStatisticsAsync()
        {
            try
            {
                _logger.LogDebug("بدء الحصول على إحصائيات الكتب - Starting to get book statistics");

                var statistics = await _bookRepository.GetBookStatisticsAsync();

                _logger.LogDebug("تم الحصول على إحصائيات الكتب بنجاح - Successfully retrieved book statistics");
                return ServiceResult<BookStatistics>.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على إحصائيات الكتب - Error getting book statistics");
                return ServiceResult<BookStatistics>.Failure("حدث خطأ أثناء الحصول على إحصائيات الكتب - An error occurred while retrieving book statistics");
            }
        }

        /// <summary>
        /// التحقق من صحة بيانات الكتاب
        /// Validate book data
        /// </summary>
        public async Task<ValidationResult> ValidateBookAsync(Book book, bool isUpdate = false)
        {
            var errors = new List<string>();

            try
            {
                // التحقق من البيانات الأساسية
                // Validate basic data
                if (string.IsNullOrWhiteSpace(book.Title))
                {
                    errors.Add("عنوان الكتاب مطلوب - Book title is required");
                }
                else if (book.Title.Length > 200)
                {
                    errors.Add("عنوان الكتاب يجب أن يكون أقل من 200 حرف - Book title must be less than 200 characters");
                }

                if (string.IsNullOrWhiteSpace(book.Author))
                {
                    errors.Add("اسم المؤلف مطلوب - Author name is required");
                }
                else if (book.Author.Length > 100)
                {
                    errors.Add("اسم المؤلف يجب أن يكون أقل من 100 حرف - Author name must be less than 100 characters");
                }

                if (string.IsNullOrWhiteSpace(book.ISBN))
                {
                    errors.Add("الرقم المعياري مطلوب - ISBN is required");
                }
                else
                {
                    var cleanIsbn = CleanIsbn(book.ISBN);
                    if (!IsValidIsbn(cleanIsbn))
                    {
                        errors.Add("الرقم المعياري غير صحيح - Invalid ISBN format");
                    }
                    else
                    {
                        book.ISBN = cleanIsbn; // تنظيف الرقم المعياري
                    }
                }

                if (!string.IsNullOrWhiteSpace(book.Publisher) && book.Publisher.Length > 100)
                {
                    errors.Add("اسم الناشر يجب أن يكون أقل من 100 حرف - Publisher name must be less than 100 characters");
                }

                if (book.PublicationYear.HasValue)
                {
                    var currentYear = DateTime.Now.Year;
                    if (book.PublicationYear < 1000 || book.PublicationYear > currentYear)
                    {
                        errors.Add($"سنة النشر يجب أن تكون بين 1000 و {currentYear} - Publication year must be between 1000 and {currentYear}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(book.Genre) && book.Genre.Length > 50)
                {
                    errors.Add("نوع الكتاب يجب أن يكون أقل من 50 حرف - Genre must be less than 50 characters");
                }

                if (book.TotalCopies <= 0)
                {
                    errors.Add("عدد النسخ الإجمالي يجب أن يكون أكبر من صفر - Total copies must be greater than zero");
                }

                if (book.AvailableCopies < 0)
                {
                    errors.Add("عدد النسخ المتاحة لا يمكن أن يكون سالب - Available copies cannot be negative");
                }

                if (book.AvailableCopies > book.TotalCopies)
                {
                    errors.Add("عدد النسخ المتاحة لا يمكن أن يكون أكبر من العدد الإجمالي - Available copies cannot be greater than total copies");
                }

                if (!string.IsNullOrWhiteSpace(book.Description) && book.Description.Length > 500)
                {
                    errors.Add("وصف الكتاب يجب أن يكون أقل من 500 حرف - Book description must be less than 500 characters");
                }

                // التحقق من المعرف في حالة التحديث
                // Validate ID for updates
                if (isUpdate && book.BookId <= 0)
                {
                    errors.Add("معرف الكتاب مطلوب للتحديث - Book ID is required for update");
                }

                return errors.Any() ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة بيانات الكتاب - Error validating book data");
                errors.Add("حدث خطأ أثناء التحقق من صحة البيانات - An error occurred while validating data");
                return ValidationResult.Invalid(errors);
            }
        }

        /// <summary>
        /// التحقق من صحة معايير التقسيم
        /// Validate pagination criteria
        /// </summary>
        private static ValidationResult ValidatePaginationCriteria(BookSearchDto searchDto)
        {
            var errors = new List<string>();

            if (searchDto.PageNumber <= 0)
            {
                errors.Add("رقم الصفحة يجب أن يكون أكبر من صفر - Page number must be greater than zero");
            }

            if (searchDto.PageSize <= 0 || searchDto.PageSize > 100)
            {
                errors.Add("حجم الصفحة يجب أن يكون بين 1 و 100 - Page size must be between 1 and 100");
            }

            return errors.Any() ? ValidationResult.Invalid(errors) : ValidationResult.Valid();
        }

        /// <summary>
        /// إضافة كتاب جديد مع التحقق من الصلاحيات
        /// Add a new book with authorization check
        /// </summary>
        public async Task<ServiceResult<int>> AddBookAsync(Book book, int userId)
        {
            try
            {
                _logger.LogDebug("التحقق من صلاحية إضافة كتاب للمستخدم {UserId} - Checking book addition permission for user", userId);

                // التحقق من الصلاحيات
                // Check permissions
                var authResult = await _authorizationService.CanManageBooksAsync(userId);
                if (!authResult.IsSuccess)
                {
                    _logger.LogWarning("فشل في التحقق من صلاحيات المستخدم {UserId} - Failed to check user permissions", userId);
                    return ServiceResult<int>.Failure(authResult.ErrorMessage ?? "فشل في التحقق من الصلاحيات");
                }

                if (!authResult.Data)
                {
                    _logger.LogWarning("المستخدم {UserId} ليس لديه صلاحية إضافة الكتب - User does not have permission to add books", userId);
                    return ServiceResult<int>.Failure("ليس لديك صلاحية لإضافة الكتب");
                }

                // إضافة الكتاب
                // Add the book
                return await AddBookAsync(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة كتاب مع التحقق من الصلاحيات للمستخدم {UserId} - Error adding book with authorization check", userId);
                return ServiceResult<int>.Failure("حدث خطأ أثناء إضافة الكتاب");
            }
        }

        /// <summary>
        /// تحديث كتاب موجود مع التحقق من الصلاحيات
        /// Update an existing book with authorization check
        /// </summary>
        public async Task<ServiceResult<bool>> UpdateBookAsync(Book book, int userId)
        {
            try
            {
                _logger.LogDebug("التحقق من صلاحية تحديث كتاب للمستخدم {UserId} - Checking book update permission for user", userId);

                // التحقق من الصلاحيات
                // Check permissions
                var authResult = await _authorizationService.CanManageBooksAsync(userId);
                if (!authResult.IsSuccess)
                {
                    _logger.LogWarning("فشل في التحقق من صلاحيات المستخدم {UserId} - Failed to check user permissions", userId);
                    return ServiceResult<bool>.Failure(authResult.ErrorMessage ?? "فشل في التحقق من الصلاحيات");
                }

                if (!authResult.Data)
                {
                    _logger.LogWarning("المستخدم {UserId} ليس لديه صلاحية تحديث الكتب - User does not have permission to update books", userId);
                    return ServiceResult<bool>.Failure("ليس لديك صلاحية لتحديث الكتب");
                }

                // تحديث الكتاب
                // Update the book
                return await UpdateBookAsync(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث كتاب مع التحقق من الصلاحيات للمستخدم {UserId} - Error updating book with authorization check", userId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء تحديث الكتاب");
            }
        }

        /// <summary>
        /// حذف كتاب مع التحقق من الصلاحيات
        /// Delete a book with authorization check
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteBookAsync(int id, int userId)
        {
            try
            {
                _logger.LogDebug("التحقق من صلاحية حذف كتاب للمستخدم {UserId} - Checking book deletion permission for user", userId);

                // التحقق من الصلاحيات
                // Check permissions
                var authResult = await _authorizationService.CanManageBooksAsync(userId);
                if (!authResult.IsSuccess)
                {
                    _logger.LogWarning("فشل في التحقق من صلاحيات المستخدم {UserId} - Failed to check user permissions", userId);
                    return ServiceResult<bool>.Failure(authResult.ErrorMessage ?? "فشل في التحقق من الصلاحيات");
                }

                if (!authResult.Data)
                {
                    _logger.LogWarning("المستخدم {UserId} ليس لديه صلاحية حذف الكتب - User does not have permission to delete books", userId);
                    return ServiceResult<bool>.Failure("ليس لديك صلاحية لحذف الكتب");
                }

                // حذف الكتاب
                // Delete the book
                return await DeleteBookAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف كتاب مع التحقق من الصلاحيات للمستخدم {UserId} - Error deleting book with authorization check", userId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء حذف الكتاب");
            }
        }
    }
}
