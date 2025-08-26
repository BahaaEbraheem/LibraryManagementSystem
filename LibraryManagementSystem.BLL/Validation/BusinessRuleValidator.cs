using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using LibraryManagementSystem.DAL.Repositories;
using LibraryManagementSystem.BLL.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace LibraryManagementSystem.BLL.Validation
{
    /// <summary>
    /// مدقق قواعد الأعمال
    /// Business rule validator implementation
    /// </summary>
    public class BusinessRuleValidator : IBusinessRuleValidator
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly LibrarySettings _librarySettings;
        private readonly ILogger<BusinessRuleValidator> _logger;

        /// <summary>
        /// منشئ مدقق قواعد الأعمال
        /// Business rule validator constructor
        /// </summary>
        public BusinessRuleValidator(
            IBookRepository bookRepository,
            IUserRepository userRepository,
            IBorrowingRepository borrowingRepository,
            IOptions<LibrarySettings> librarySettings,
            ILogger<BusinessRuleValidator> logger)
        {
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _borrowingRepository = borrowingRepository ?? throw new ArgumentNullException(nameof(borrowingRepository));
            _librarySettings = librarySettings?.Value ?? throw new ArgumentNullException(nameof(librarySettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// التحقق من صحة استعارة كتاب
        /// Validate book borrowing
        /// </summary>
        public async Task<ValidationResult> ValidateBorrowingAsync(int userId, int bookId)
        {
            try
            {
                _logger.LogDebug("التحقق من صحة استعارة الكتاب {BookId} للمستخدم {UserId} - Validating borrowing of book {BookId} for user {UserId}",
                    bookId, userId);

                var result = new ValidationResult { IsValid = true };

                // التحقق من وجود المستخدم وحالته
                // Check user existence and status
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ValidationResult.Failure("المستخدم غير موجود - User not found", BusinessRuleErrorCodes.UserNotActive);
                }

                if (!user.IsActive)
                {
                    return ValidationResult.Failure("المستخدم غير نشط - User is not active", BusinessRuleErrorCodes.UserNotActive);
                }

                // التحقق من وجود الكتاب وتوفره
                // Check book existence and availability
                var book = await _bookRepository.GetByIdAsync(bookId);
                if (book == null)
                {
                    return ValidationResult.Failure("الكتاب غير موجود - Book not found", BusinessRuleErrorCodes.BookNotAvailable);
                }

                if (book.AvailableCopies <= 0)
                {
                    return ValidationResult.Failure("لا توجد نسخ متاحة من الكتاب - No available copies of the book", BusinessRuleErrorCodes.BookNotAvailable);
                }

                // التحقق من عدم استعارة المستخدم للكتاب مسبقاً
                // Check if user hasn't already borrowed this book
                var userActiveBorrowings = await _borrowingRepository.GetActiveUserBorrowingsAsync(userId);
                if (userActiveBorrowings.Any(b => b.BookId == bookId))
                {
                    return ValidationResult.Failure("لقد استعرت هذا الكتاب مسبقاً - You have already borrowed this book", BusinessRuleErrorCodes.BookAlreadyBorrowedByUser);
                }

                // التحقق من حد الاستعارة للمستخدم
                // Check user borrowing limit
                var currentBorrowingsCount = userActiveBorrowings.Count();
                if (currentBorrowingsCount >= _librarySettings.MaxBooksPerUser)
                {
                    return ValidationResult.Failure($"لقد وصلت للحد الأقصى من الكتب المستعارة ({_librarySettings.MaxBooksPerUser}) - You have reached the maximum borrowing limit ({_librarySettings.MaxBooksPerUser})",
                        BusinessRuleErrorCodes.UserBorrowingLimitExceeded)
                        .AddData("currentCount", currentBorrowingsCount)
                        .AddData("maxAllowed", _librarySettings.MaxBooksPerUser);
                }

                // التحقق من وجود كتب متأخرة
                // Check for overdue books
                var overdueBooks = userActiveBorrowings.Where(b => b.DueDate < DateTime.Now).ToList();
                if (overdueBooks.Any())
                {
                    result.AddWarning($"لديك {overdueBooks.Count()} كتاب متأخر. يرجى إرجاعها أولاً - You have {overdueBooks.Count()} overdue book(s). Please return them first");

                    // منع الاستعارة إذا كان هناك أكثر من كتابين متأخرين
                    // Prevent borrowing if more than 2 overdue books
                    if (overdueBooks.Count() > 2)
                    {
                        return ValidationResult.Failure("لا يمكن الاستعارة مع وجود أكثر من كتابين متأخرين - Cannot borrow with more than 2 overdue books",
                            BusinessRuleErrorCodes.UserHasOverdueBooks)
                            .AddData("overdueCount", overdueBooks.Count());
                    }
                }

                _logger.LogDebug("تم التحقق من صحة الاستعارة بنجاح - Borrowing validation successful");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة الاستعارة - Error validating borrowing");
                return ValidationResult.Failure("حدث خطأ أثناء التحقق من صحة الاستعارة - Error occurred while validating borrowing");
            }
        }

        /// <summary>
        /// التحقق من صحة إرجاع كتاب
        /// Validate book return
        /// </summary>
        public async Task<ValidationResult> ValidateReturnAsync(int borrowingId, int userId)
        {
            try
            {
                _logger.LogDebug("التحقق من صحة إرجاع الاستعارة {BorrowingId} للمستخدم {UserId} - Validating return of borrowing {BorrowingId} for user {UserId}",
                    borrowingId, userId);

                // التحقق من وجود الاستعارة
                // Check borrowing existence
                var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
                if (borrowing == null)
                {
                    return ValidationResult.Failure("الاستعارة غير موجودة - Borrowing not found", BusinessRuleErrorCodes.BorrowingNotFound);
                }

                // التحقق من ملكية الاستعارة
                // Check borrowing ownership
                if (borrowing.UserId != userId)
                {
                    return ValidationResult.Failure("غير مصرح لك بإرجاع هذا الكتاب - You are not authorized to return this book", BusinessRuleErrorCodes.UnauthorizedReturn);
                }

                // التحقق من عدم إرجاع الكتاب مسبقاً
                // Check if book is not already returned
                if (borrowing.IsReturned)
                {
                    return ValidationResult.Failure("تم إرجاع الكتاب مسبقاً - Book has already been returned", BusinessRuleErrorCodes.BorrowingAlreadyReturned);
                }

                var result = ValidationResult.Success();

                // التحقق من التأخير وحساب الغرامة
                // Check for lateness and calculate fine
                if (borrowing.DueDate < DateTime.Now)
                {
                    var daysLate = (DateTime.Now - borrowing.DueDate).Days;
                    var lateFee = Math.Max(0, (daysLate - _librarySettings.GraceDays)) * _librarySettings.LateFeePerDay;

                    if (lateFee > 0)
                    {
                        result.AddWarning($"الكتاب متأخر {daysLate} يوم. الغرامة: {lateFee:C} - Book is {daysLate} days late. Fine: {lateFee:C}")
                              .AddData("daysLate", daysLate)
                              .AddData("lateFee", lateFee);
                    }
                }

                _logger.LogDebug("تم التحقق من صحة الإرجاع بنجاح - Return validation successful");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة الإرجاع - Error validating return");
                return ValidationResult.Failure("حدث خطأ أثناء التحقق من صحة الإرجاع - Error occurred while validating return");
            }
        }

        /// <summary>
        /// التحقق من صحة تجديد استعارة
        /// Validate borrowing renewal
        /// </summary>
        public async Task<ValidationResult> ValidateRenewalAsync(int borrowingId, int userId)
        {
            try
            {
                _logger.LogDebug("التحقق من صحة تجديد الاستعارة {BorrowingId} للمستخدم {UserId} - Validating renewal of borrowing {BorrowingId} for user {UserId}",
                    borrowingId, userId);

                // التحقق من وجود الاستعارة
                // Check borrowing existence
                var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
                if (borrowing == null)
                {
                    return ValidationResult.Failure("الاستعارة غير موجودة - Borrowing not found", BusinessRuleErrorCodes.BorrowingNotFound);
                }

                // التحقق من ملكية الاستعارة
                // Check borrowing ownership
                if (borrowing.UserId != userId)
                {
                    return ValidationResult.Failure("غير مصرح لك بتجديد هذا الكتاب - You are not authorized to renew this book", BusinessRuleErrorCodes.UnauthorizedReturn);
                }

                // التحقق من عدم إرجاع الكتاب
                // Check if book is not returned
                if (borrowing.IsReturned)
                {
                    return ValidationResult.Failure("لا يمكن تجديد كتاب تم إرجاعه - Cannot renew a returned book", BusinessRuleErrorCodes.BorrowingAlreadyReturned);
                }

                // التحقق من حد التجديد
                // Check renewal limit
                // Note: RenewalCount property not implemented in current model
                // This validation can be added when renewal functionality is implemented

                // التحقق من عدم وجود تأخير كبير
                // Check for significant overdue
                var daysOverdue = (DateTime.Now - borrowing.DueDate).Days;
                if (daysOverdue > 7) // أكثر من أسبوع متأخر
                {
                    return ValidationResult.Failure("لا يمكن تجديد كتاب متأخر أكثر من أسبوع - Cannot renew a book that is more than a week overdue",
                        BusinessRuleErrorCodes.BorrowingOverdue)
                        .AddData("daysOverdue", daysOverdue);
                }

                var result = ValidationResult.Success();

                // تحذير في حالة التأخير البسيط
                // Warning for minor overdue
                if (daysOverdue > 0)
                {
                    result.AddWarning($"الكتاب متأخر {daysOverdue} يوم - Book is {daysOverdue} days overdue");
                }

                _logger.LogDebug("تم التحقق من صحة التجديد بنجاح - Renewal validation successful");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة التجديد - Error validating renewal");
                return ValidationResult.Failure("حدث خطأ أثناء التحقق من صحة التجديد - Error occurred while validating renewal");
            }
        }

        /// <summary>
        /// التحقق من صحة إضافة كتاب
        /// Validate book addition
        /// </summary>
        public async Task<ValidationResult> ValidateBookAdditionAsync(Book book)
        {
            try
            {
                _logger.LogDebug("التحقق من صحة إضافة الكتاب {Title} - Validating addition of book {Title}", book.Title);

                var result = new ValidationResult { IsValid = true };

                // التحقق من البيانات الأساسية
                // Check basic data
                if (string.IsNullOrWhiteSpace(book.Title))
                {
                    result.AddError("عنوان الكتاب مطلوب - Book title is required");
                }

                if (string.IsNullOrWhiteSpace(book.Author))
                {
                    result.AddError("مؤلف الكتاب مطلوب - Book author is required");
                }

                if (string.IsNullOrWhiteSpace(book.ISBN))
                {
                    result.AddError("الرقم المعياري للكتاب مطلوب - Book ISBN is required");
                }
                else
                {
                    // التحقق من صحة تنسيق ISBN
                    // Validate ISBN format
                    if (!IsValidIsbn(book.ISBN))
                    {
                        result.AddError("تنسيق الرقم المعياري غير صحيح - Invalid ISBN format");
                    }
                    else
                    {
                        // التحقق من عدم وجود ISBN مكرر
                        // Check for duplicate ISBN
                        var existingBook = await _bookRepository.GetByIsbnAsync(book.ISBN);
                        if (existingBook != null)
                        {
                            result.AddError("الرقم المعياري موجود مسبقاً - ISBN already exists")
                                  .AddData("existingBookId", existingBook.BookId)
                                  .AddData("existingBookTitle", existingBook.Title);
                        }
                    }
                }

                if (book.TotalCopies <= 0)
                {
                    result.AddError("عدد النسخ يجب أن يكون أكبر من صفر - Total copies must be greater than zero");
                }

                if (book.AvailableCopies < 0 || book.AvailableCopies > book.TotalCopies)
                {
                    result.AddError("عدد النسخ المتاحة غير صحيح - Invalid available copies count");
                }

                if (book.PublicationYear.HasValue && (book.PublicationYear < 1000 || book.PublicationYear > DateTime.Now.Year))
                {
                    result.AddError("سنة النشر غير صحيحة - Invalid publication year");
                }

                _logger.LogDebug("تم التحقق من صحة إضافة الكتاب: {IsValid} - Book addition validation completed: {IsValid}", result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة إضافة الكتاب - Error validating book addition");
                return ValidationResult.Failure("حدث خطأ أثناء التحقق من صحة إضافة الكتاب - Error occurred while validating book addition");
            }
        }

        /// <summary>
        /// التحقق من صحة تنسيق ISBN
        /// Validate ISBN format
        /// </summary>
        private static bool IsValidIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return false;

            // إزالة الشرطات والمسافات
            // Remove dashes and spaces
            var cleanIsbn = Regex.Replace(isbn, @"[\s\-]", "");

            // التحقق من طول ISBN (10 أو 13 رقم)
            // Check ISBN length (10 or 13 digits)
            return cleanIsbn.Length == 10 || cleanIsbn.Length == 13;
        }

        /// <summary>
        /// التحقق من صحة تحديث كتاب
        /// Validate book update
        /// </summary>
        public async Task<ValidationResult> ValidateBookUpdateAsync(Book book)
        {
            try
            {
                _logger.LogDebug("التحقق من صحة تحديث الكتاب {BookId} - Validating update of book {BookId}", book.BookId);

                // نفس التحقق من الإضافة مع التحقق من وجود الكتاب
                // Same validation as addition plus checking book existence
                var result = await ValidateBookAdditionAsync(book);

                // التحقق من وجود الكتاب
                // Check book existence
                var existingBook = await _bookRepository.GetByIdAsync(book.BookId);
                if (existingBook == null)
                {
                    return ValidationResult.Failure("الكتاب غير موجود - Book not found");
                }

                // التحقق من ISBN إذا تم تغييره
                // Check ISBN if changed
                if (existingBook.ISBN != book.ISBN)
                {
                    var bookWithSameIsbn = await _bookRepository.GetByIsbnAsync(book.ISBN);
                    if (bookWithSameIsbn != null && bookWithSameIsbn.BookId != book.BookId)
                    {
                        result.AddError("الرقم المعياري موجود مسبقاً - ISBN already exists");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة تحديث الكتاب - Error validating book update");
                return ValidationResult.Failure("حدث خطأ أثناء التحقق من صحة تحديث الكتاب - Error occurred while validating book update");
            }
        }

        /// <summary>
        /// التحقق من صحة حذف كتاب
        /// Validate book deletion
        /// </summary>
        public async Task<ValidationResult> ValidateBookDeletionAsync(int bookId)
        {
            try
            {
                _logger.LogDebug("التحقق من صحة حذف الكتاب {BookId} - Validating deletion of book {BookId}", bookId);

                // التحقق من وجود الكتاب
                // Check book existence
                var book = await _bookRepository.GetByIdAsync(bookId);
                if (book == null)
                {
                    return ValidationResult.Failure("الكتاب غير موجود - Book not found");
                }

                // التحقق من عدم وجود استعارات نشطة
                // Check for active borrowings
                var activeBorrowings = await _borrowingRepository.GetBookBorrowingsAsync(bookId);
                var activeBorrowingsList = activeBorrowings.Where(b => !b.IsReturned).ToList();

                if (activeBorrowingsList.Any())
                {
                    return ValidationResult.Failure($"لا يمكن حذف الكتاب لوجود {activeBorrowingsList.Count} استعارة نشطة - Cannot delete book with {activeBorrowingsList.Count} active borrowing(s)",
                        BusinessRuleErrorCodes.BookHasActiveBorrowings)
                        .AddData("activeBorrowingsCount", activeBorrowingsList.Count);
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة حذف الكتاب - Error validating book deletion");
                return ValidationResult.Failure("حدث خطأ أثناء التحقق من صحة حذف الكتاب - Error occurred while validating book deletion");
            }
        }

        /// <summary>
        /// التحقق من صحة إضافة مستخدم
        /// Validate user addition
        /// </summary>
        public async Task<ValidationResult> ValidateUserAdditionAsync(User user)
        {
            try
            {
                _logger.LogDebug("التحقق من صحة إضافة المستخدم {Email} - Validating addition of user {Email}", user.Email);

                var result = new ValidationResult { IsValid = true };

                // التحقق من البيانات الأساسية
                // Check basic data
                if (string.IsNullOrWhiteSpace(user.FirstName))
                {
                    result.AddError("الاسم الأول مطلوب - First name is required");
                }

                if (string.IsNullOrWhiteSpace(user.LastName))
                {
                    result.AddError("الاسم الأخير مطلوب - Last name is required");
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    result.AddError("البريد الإلكتروني مطلوب - Email is required");
                }
                else
                {
                    // التحقق من صحة تنسيق البريد الإلكتروني
                    // Validate email format
                    if (!IsValidEmail(user.Email))
                    {
                        result.AddError("تنسيق البريد الإلكتروني غير صحيح - Invalid email format");
                    }
                    else
                    {
                        // التحقق من عدم وجود بريد إلكتروني مكرر
                        // Check for duplicate email
                        var existingUser = await _userRepository.GetByEmailAsync(user.Email);
                        if (existingUser != null)
                        {
                            result.AddError("البريد الإلكتروني موجود مسبقاً - Email already exists")
                                  .AddData("existingUserId", existingUser.UserId);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(user.PhoneNumber) && !IsValidPhoneNumber(user.PhoneNumber))
                {
                    result.AddError("تنسيق رقم الهاتف غير صحيح - Invalid phone number format");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة إضافة المستخدم - Error validating user addition");
                return ValidationResult.Failure("حدث خطأ أثناء التحقق من صحة إضافة المستخدم - Error occurred while validating user addition");
            }
        }

        /// <summary>
        /// التحقق من صحة تحديث مستخدم
        /// Validate user update
        /// </summary>
        public async Task<ValidationResult> ValidateUserUpdateAsync(User user)
        {
            try
            {
                _logger.LogDebug("التحقق من صحة تحديث المستخدم {UserId} - Validating update of user {UserId}", user.UserId);

                // نفس التحقق من الإضافة مع التحقق من وجود المستخدم
                // Same validation as addition plus checking user existence
                var result = await ValidateUserAdditionAsync(user);

                // التحقق من وجود المستخدم
                // Check user existence
                var existingUser = await _userRepository.GetByIdAsync(user.UserId);
                if (existingUser == null)
                {
                    return ValidationResult.Failure("المستخدم غير موجود - User not found");
                }

                // التحقق من البريد الإلكتروني إذا تم تغييره
                // Check email if changed
                if (existingUser.Email != user.Email)
                {
                    var userWithSameEmail = await _userRepository.GetByEmailAsync(user.Email);
                    if (userWithSameEmail != null && userWithSameEmail.UserId != user.UserId)
                    {
                        result.AddError("البريد الإلكتروني موجود مسبقاً - Email already exists");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة تحديث المستخدم - Error validating user update");
                return ValidationResult.Failure("حدث خطأ أثناء التحقق من صحة تحديث المستخدم - Error occurred while validating user update");
            }
        }

        /// <summary>
        /// التحقق من صحة حذف مستخدم
        /// Validate user deletion
        /// </summary>
        public async Task<ValidationResult> ValidateUserDeletionAsync(int userId)
        {
            try
            {
                _logger.LogDebug("التحقق من صحة حذف المستخدم {UserId} - Validating deletion of user {UserId}", userId);

                // التحقق من وجود المستخدم
                // Check user existence
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ValidationResult.Failure("المستخدم غير موجود - User not found");
                }

                // منع حذف المدير الوحيد
                // Prevent deleting the only admin
                if (user.Role == UserRole.Administrator)
                {
                    var allUsers = await _userRepository.GetAllAsync();
                    var adminCount = allUsers.Count(u => u.Role == UserRole.Administrator && u.IsActive);
                    if (adminCount <= 1)
                    {
                        return ValidationResult.Failure("لا يمكن حذف المدير الوحيد في النظام - Cannot delete the only administrator in the system",
                            BusinessRuleErrorCodes.CannotDeleteAdminUser);
                    }
                }

                // التحقق من عدم وجود استعارات نشطة
                // Check for active borrowings
                var activeBorrowings = await _borrowingRepository.GetActiveUserBorrowingsAsync(userId);
                if (activeBorrowings.Any())
                {
                    return ValidationResult.Failure($"لا يمكن حذف المستخدم لوجود {activeBorrowings.Count()} استعارة نشطة - Cannot delete user with {activeBorrowings.Count()} active borrowing(s)",
                        BusinessRuleErrorCodes.UserHasActiveBorrowings)
                        .AddData("activeBorrowingsCount", activeBorrowings.Count());
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صحة حذف المستخدم - Error validating user deletion");
                return ValidationResult.Failure("حدث خطأ أثناء التحقق من صحة حذف المستخدم - Error occurred while validating user deletion");
            }
        }

        /// <summary>
        /// التحقق من صحة تنسيق البريد الإلكتروني
        /// Validate email format
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// التحقق من صحة تنسيق رقم الهاتف
        /// Validate phone number format
        /// </summary>
        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // تنسيق بسيط لرقم الهاتف (يمكن تحسينه حسب المتطلبات)
            // Simple phone number format (can be improved based on requirements)
            var phoneRegex = new Regex(@"^[\+]?[0-9\-\(\)\s]{7,15}$");
            return phoneRegex.IsMatch(phoneNumber);
        }
    }
}
