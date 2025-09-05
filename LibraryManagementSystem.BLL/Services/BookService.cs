using LibraryManagementSystem.BLL.Validation;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using static LibraryManagementSystem.DAL.Caching.CacheKeys;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// خدمة إدارة الكتب
    /// Book management service
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<BookService> _logger;
        private readonly IBusinessRuleValidator _validator;

        public BookService(
            IBookRepository bookRepository,
            IAuthorizationService authorizationService,
            IBusinessRuleValidator validator, 
            ILogger<BookService> logger)
        {
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region -------------------- Queries --------------------

        public async Task<ServiceResult<Book>> GetBookByIdAsync(int id)
        {
            if (id <= 0)
                return ServiceResult<Book>.Failure("معرف الكتاب غير صحيح");

            try
            {
                var book = await _bookRepository.GetByIdAsync(id);
                if (book == null)
                    return ServiceResult<Book>.Failure("لم يتم العثور على الكتاب");

                return ServiceResult<Book>.Success(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الكتاب بالمعرف {BookId}", id);
                return ServiceResult<Book>.Failure("حدث خطأ أثناء البحث عن الكتاب");
            }
        }

        public async Task<ServiceResult<PagedResult<Book>>> SearchBooksAsync(BookSearchDto searchDto)
        {
            if (searchDto == null)
                return ServiceResult<PagedResult<Book>>.Failure("معايير البحث مطلوبة");

            var validation = ValidatePaginationCriteria(searchDto);
            if (!validation.IsValid)
                return ServiceResult<PagedResult<Book>>.ValidationFailure(validation.ErrorMessages);

            try
            {
                var result = await _bookRepository.SearchAsync(searchDto);
                return ServiceResult<PagedResult<Book>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في البحث عن الكتب");
                return ServiceResult<PagedResult<Book>>.Failure("حدث خطأ أثناء البحث عن الكتب");
            }
        }

        public async Task<ServiceResult<IEnumerable<Book>>> GetAvailableBooksAsync()
        {
            try
            {
                var books = await _bookRepository.GetAvailableAsync();
                return ServiceResult<IEnumerable<Book>>.Success(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الكتب المتاحة");
                return ServiceResult<IEnumerable<Book>>.Failure("حدث خطأ أثناء الحصول على الكتب المتاحة");
            }
        }

        public async Task<ServiceResult<bool>> IsBookAvailableAsync(int bookId)
        {
            if (bookId <= 0)
                return ServiceResult<bool>.Failure("معرف الكتاب غير صحيح");

            try
            {
                var book = await _bookRepository.GetByIdAsync(bookId);
                if (book == null)
                    return ServiceResult<bool>.Failure("لم يتم العثور على الكتاب");

                return ServiceResult<bool>.Success(book.AvailableCopies > 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في فحص توفر الكتاب {BookId}", bookId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء فحص توفر الكتاب");
            }
        }

        #endregion

        #region -------------------- Commands --------------------
        public async Task<ServiceResult<int>> AddBookAsync(Book book, int userId)
        {
            if (book == null)
                return ServiceResult<int>.Failure("بيانات الكتاب مطلوبة");


            // التحقق من صلاحيات المستخدم
            var auth = await CheckAuthorizationAsync(userId);
            if (!auth.IsSuccess)
                return ServiceResult<int>.Failure(auth.ErrorMessage);


            // التحقق من صحة بيانات الكتاب
            var validation = await ValidateBookAsync(book, false);
            if (!validation.IsValid)
                return ServiceResult<int>.ValidationFailure(validation.ErrorMessages);

            // التحقق من قواعد الأعمال المخصصة
            validation = await _validator.ValidateBookAdditionAsync(book,false);
            if (!validation.IsValid)
                return ServiceResult<int>.ValidationFailure(validation.ErrorMessages);

            // التحقق من وجود كتاب بنفس ISBN
            var existingByIsbn = await _bookRepository.GetByIsbnAsync(book.ISBN);
            if (existingByIsbn != null)
                return ServiceResult<int>.Failure("يوجد كتاب بنفس الرقم المعياري", BusinessRuleErrorCodes.BookIsbnExists);

            try
            {
                var bookId = await _bookRepository.AddAsync(book);
                return ServiceResult<int>.Success(bookId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة كتاب");
                return ServiceResult<int>.Failure("حدث خطأ أثناء إضافة الكتاب");
            }
        }

        public async Task<ServiceResult<bool>> UpdateBookAsync(Book book, int userId)
        {
            if (book == null)
                return ServiceResult<bool>.Failure("بيانات الكتاب مطلوبة");


            var auth = await CheckAuthorizationAsync(userId);
            if (!auth.IsSuccess)
                return ServiceResult<bool>.Failure(auth.ErrorMessage);


            var validation = await ValidateBookAsync(book, true);
            if (!validation.IsValid)
                return ServiceResult<bool>.ValidationFailure(validation.ErrorMessages);

            // التحقق من قواعد الأعمال المخصصة
            validation = await _validator.ValidateBookUpdateAsync(book);
            if (!validation.IsValid)
                return ServiceResult<bool>.ValidationFailure(validation.ErrorMessages);

            var existing = await _bookRepository.GetByIdAsync(book.BookId);
            if (existing == null)
                return ServiceResult<bool>.Failure("لم يتم العثور على الكتاب");

            // تحقق من ISBN فقط إذا تغير عن النسخة الحالية
            if (!string.Equals(existing.ISBN, book.ISBN, StringComparison.OrdinalIgnoreCase))
            {
                var bookWithSameIsbn = await _bookRepository.GetByIsbnAsync(book.ISBN);
                if (bookWithSameIsbn != null && bookWithSameIsbn.BookId != book.BookId)
                    return ServiceResult<bool>.Failure("يوجد كتاب آخر بنفس الرقم المعياري");
            }

            try
            {
                var success = await _bookRepository.UpdateAsync(book);
                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث الكتاب");
                return ServiceResult<bool>.Failure("حدث خطأ أثناء تحديث الكتاب");
            }
        }

        public async Task<ServiceResult<bool>> DeleteBookAsync(int id, int userId)
        {
            if (id <= 0)
                return ServiceResult<bool>.Failure("معرف الكتاب غير صحيح");

            var auth = await CheckAuthorizationAsync(userId);
            if (!auth.IsSuccess)
                return ServiceResult<bool>.Failure(auth.ErrorMessage);

            // التحقق من قواعد الأعمال المخصصة
            var validation = await _validator.ValidateBookDeletionAsync(id);
            if (!validation.IsValid)
                return ServiceResult<bool>.ValidationFailure(validation.ErrorMessages);

            var existing = await _bookRepository.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult<bool>.Failure("لم يتم العثور على الكتاب");

            try
            {
                var success = await _bookRepository.DeleteAsync(id);
                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الكتاب {BookId}", id);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء حذف الكتاب");
            }
        }

        #endregion

        #region -------------------- Helpers --------------------

        private async Task<ServiceResult<bool>> CheckAuthorizationAsync(int userId)
        {
            var authResult = await _authorizationService.CanManageBooksAsync(userId);
            if (!authResult.IsSuccess)
                return ServiceResult<bool>.Failure(authResult.ErrorMessage ?? "فشل في التحقق من الصلاحيات");

            if (!authResult.Data)
                return ServiceResult<bool>.Failure("ليس لديك صلاحية لإدارة الكتب");

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ValidationResult> ValidateBookAsync(Book book, bool isUpdate = false)
        {
            var errors = new List<string>();

            try
            {
                if (string.IsNullOrWhiteSpace(book.Title)) errors.Add("عنوان الكتاب مطلوب");
                else if (book.Title.Length > 200) errors.Add("عنوان الكتاب يجب أن يكون أقل من 200 حرف");

                if (string.IsNullOrWhiteSpace(book.Author)) errors.Add("اسم المؤلف مطلوب");
                else if (book.Author.Length > 100) errors.Add("اسم المؤلف يجب أن يكون أقل من 100 حرف");

                if (string.IsNullOrWhiteSpace(book.ISBN)) errors.Add("الرقم المعياري مطلوب");
                else if (!IsValidIsbn(CleanIsbn(book.ISBN))) errors.Add("الرقم المعياري غير صحيح");

                if (!string.IsNullOrWhiteSpace(book.Publisher) && book.Publisher.Length > 100)
                    errors.Add("اسم الناشر يجب أن يكون أقل من 100 حرف");

                if (book.PublicationYear.HasValue)
                {
                    int year = DateTime.Now.Year;
                    if (book.PublicationYear < 1000 || book.PublicationYear > year)
                        errors.Add($"سنة النشر يجب أن تكون بين 1000 و {year}");
                }

                if (!string.IsNullOrWhiteSpace(book.Genre) && book.Genre.Length > 50)
                    errors.Add("نوع الكتاب يجب أن يكون أقل من 50 حرف");

                if (book.TotalCopies <= 0) errors.Add("عدد النسخ الإجمالي يجب أن يكون أكبر من صفر");
                if (book.AvailableCopies < 0) errors.Add("عدد النسخ المتاحة لا يمكن أن يكون سالب");
                if (book.AvailableCopies > book.TotalCopies)
                    errors.Add("عدد النسخ المتاحة لا يمكن أن يكون أكبر من العدد الإجمالي");

                if (!string.IsNullOrWhiteSpace(book.Description) && book.Description.Length > 500)
                    errors.Add("وصف الكتاب يجب أن يكون أقل من 500 حرف");

                if (isUpdate && book.BookId <= 0)
                    errors.Add("معرف الكتاب مطلوب للتحديث");

                return errors.Any()
                    ? ValidationResult.Failure(errors, BusinessRuleErrorCodes.InvalidBookData) // استخدام Failure مع قائمة الأخطاء
                    : ValidationResult.Success(); // استخدام Success إذا لا توجد أخطاء
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة بيانات الكتاب");
                errors.Add("حدث خطأ أثناء التحقق من صحة البيانات");
                return ValidationResult.Failure(errors, BusinessRuleErrorCodes.InvalidBookData);
            }
        }


        private static string CleanIsbn(string isbn) => Regex.Replace(isbn, @"[^\d]", "");
        private static bool IsValidIsbn(string isbn) => isbn.Length == 10 || isbn.Length == 13;

        private static ValidationResult ValidatePaginationCriteria(BookSearchDto searchDto)
        {
            var errors = new List<string>();

            if (searchDto.PageNumber <= 0)
                errors.Add("رقم الصفحة يجب أن يكون أكبر من صفر");

            if (searchDto.PageSize <= 0 || searchDto.PageSize > 100)
                errors.Add("حجم الصفحة يجب أن يكون بين 1 و 100");

            return errors.Any()
                ? ValidationResult.Failure(errors)   // استخدم Failure مع قائمة الأخطاء
                : ValidationResult.Success();        // استخدم Success إذا لا توجد أخطاء
        }

      




        #endregion
    }
}
