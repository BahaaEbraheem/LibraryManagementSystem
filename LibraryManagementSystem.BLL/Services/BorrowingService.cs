using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Repositories;
using LibraryManagementSystem.DAL.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// تنفيذ خدمة الاستعارات
    /// Borrowing service implementation
    /// </summary>
    public class BorrowingService : IBorrowingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<BorrowingService> _logger;
        private readonly LibrarySettings _librarySettings;

        public BorrowingService(
            IUnitOfWork unitOfWork,
            IBorrowingRepository borrowingRepository,
            IBookRepository bookRepository,
            IUserRepository userRepository,
            ILogger<BorrowingService> logger,
            IOptions<LibrarySettings> librarySettings)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _borrowingRepository = borrowingRepository ?? throw new ArgumentNullException(nameof(borrowingRepository));
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _librarySettings = librarySettings?.Value ?? throw new ArgumentNullException(nameof(librarySettings));
        }

        /// <summary>
        /// استعارة كتاب
        /// Borrow a book
        /// </summary>
        public async Task<ServiceResult<int>> BorrowBookAsync(int userId, int bookId, int borrowingDays = 14)
        {
            // استخدام Unit of Work للمعاملة
            // Use Unit of Work for transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.LogDebug("بدء عملية استعارة الكتاب {BookId} للمستخدم {UserId} - Starting book borrowing for user",
                    bookId, userId);

                // التحقق من أهلية الاستعارة
                // Check borrowing eligibility
                var eligibilityResult = await CheckBorrowingEligibilityAsync(userId, bookId);
                if (!eligibilityResult.IsSuccess || !eligibilityResult.Data!.CanBorrow)
                {
                    var reason = eligibilityResult.Data?.Reason ?? "غير مؤهل للاستعارة - Not eligible for borrowing";
                    _logger.LogWarning("المستخدم {UserId} غير مؤهل لاستعارة الكتاب {BookId}: {Reason} - User not eligible to borrow book",
                        userId, bookId, reason);
                    return ServiceResult<int>.Failure(reason);
                }

                // إنشاء سجل الاستعارة
                // Create borrowing record
                var borrowing = new Borrowing
                {
                    UserId = userId,
                    BookId = bookId,
                    BorrowDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(borrowingDays > 0 ? borrowingDays : _librarySettings.DefaultBorrowingDays),
                    IsReturned = false,
                    LateFee = 0,
                    Notes = null
                };

                var borrowingId = await _unitOfWork.Borrowings.AddAsync(borrowing);

                // تقليل عدد النسخ المتاحة
                // Decrease available copies
                var updateCopiesResult = await _unitOfWork.Books.UpdateAvailableCopiesAsync(bookId, -1);
                if (!updateCopiesResult)
                {
                    _logger.LogWarning("فشل في تحديث النسخ المتاحة للكتاب {BookId} - Failed to update available copies for book", bookId);
                    await _unitOfWork.RollbackAsync();
                    return ServiceResult<int>.Failure("فشل في تحديث النسخ المتاحة - Failed to update available copies");
                }

                // تأكيد المعاملة
                // Commit transaction
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("تم إنشاء استعارة جديدة بنجاح: {BorrowingId} للمستخدم {UserId} والكتاب {BookId} - Successfully created new borrowing for user and book",
                    borrowingId, userId, bookId);

                return ServiceResult<int>.Success(borrowingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استعارة الكتاب {BookId} للمستخدم {UserId} - Error borrowing book for user",
                    bookId, userId);

                // إلغاء المعاملة في حالة الخطأ
                // Rollback transaction on error
                try
                {
                    await _unitOfWork.RollbackAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "خطأ في إلغاء المعاملة - Error rolling back transaction");
                }

                return ServiceResult<int>.Failure("حدث خطأ أثناء عملية الاستعارة - An error occurred during the borrowing process");
            }
        }

        /// <summary>
        /// إرجاع كتاب
        /// Return a book
        /// </summary>
        public async Task<ServiceResult<bool>> ReturnBookAsync(int borrowingId, string? notes = null)
        {
            try
            {
                _logger.LogDebug("بدء عملية إرجاع الكتاب للاستعارة {BorrowingId} - Starting book return for borrowing",
                    borrowingId);

                // الحصول على سجل الاستعارة
                // Get borrowing record
                var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
                if (borrowing == null)
                {
                    _logger.LogWarning("لم يتم العثور على سجل الاستعارة {BorrowingId} - Borrowing record not found",
                        borrowingId);
                    return ServiceResult<bool>.Failure("لم يتم العثور على سجل الاستعارة - Borrowing record not found");
                }

                if (borrowing.IsReturned)
                {
                    _logger.LogWarning("الكتاب تم إرجاعه مسبقاً للاستعارة {BorrowingId} - Book already returned for borrowing",
                        borrowingId);
                    return ServiceResult<bool>.Failure("الكتاب تم إرجاعه مسبقاً - Book has already been returned");
                }

                // حساب الرسوم المتأخرة
                // Calculate late fees
                var lateFeeResult = await CalculateLateFeeAsync(borrowingId);
                var lateFee = lateFeeResult.IsSuccess ? lateFeeResult.Data : 0;

                // إرجاع الكتاب
                // Return the book
                var returnDate = DateTime.Now;
                var success = await _borrowingRepository.ReturnBookAsync(borrowingId, returnDate, lateFee, notes);

                if (success)
                {
                    // زيادة عدد النسخ المتاحة
                    // Increase available copies
                    var updateCopiesResult = await _bookRepository.UpdateAvailableCopiesAsync(borrowing.BookId, 1);
                    if (!updateCopiesResult)
                    {
                        _logger.LogWarning("فشل في تحديث النسخ المتاحة للكتاب {BookId} عند الإرجاع - Failed to update available copies for book on return", borrowing.BookId);
                    }

                    _logger.LogInformation("تم إرجاع الكتاب بنجاح للاستعارة {BorrowingId} برسوم تأخير {LateFee} - Successfully returned book for borrowing with late fee",
                        borrowingId, lateFee);
                }
                else
                {
                    _logger.LogWarning("فشل في إرجاع الكتاب للاستعارة {BorrowingId} - Failed to return book for borrowing",
                        borrowingId);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرجاع الكتاب للاستعارة {BorrowingId} - Error returning book for borrowing",
                    borrowingId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء عملية الإرجاع - An error occurred during the return process");
            }
        }

        /// <summary>
        /// الحصول على استعارات المستخدم النشطة
        /// Get user's active borrowings
        /// </summary>
        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetUserActiveBorrowingsAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    _logger.LogWarning("تم تمرير معرف مستخدم غير صحيح: {UserId} - Invalid user ID provided", userId);
                    return ServiceResult<IEnumerable<Borrowing>>.Failure("معرف المستخدم غير صحيح - Invalid user ID");
                }

                _logger.LogDebug("الحصول على الاستعارات النشطة للمستخدم {UserId} - Getting active borrowings for user", userId);

                var borrowings = await _borrowingRepository.GetActiveUserBorrowingsAsync(userId);

                _logger.LogDebug("تم الحصول على {Count} استعارة نشطة للمستخدم {UserId} - Retrieved active borrowings for user",
                    borrowings.Count(), userId);

                return ServiceResult<IEnumerable<Borrowing>>.Success(borrowings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الاستعارات النشطة للمستخدم {UserId} - Error getting active borrowings for user",
                    userId);
                return ServiceResult<IEnumerable<Borrowing>>.Failure("حدث خطأ أثناء الحصول على الاستعارات النشطة - An error occurred while retrieving active borrowings");
            }
        }

        /// <summary>
        /// الحصول على جميع استعارات المستخدم
        /// Get all user borrowings
        /// </summary>
        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetUserBorrowingsAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    _logger.LogWarning("تم تمرير معرف مستخدم غير صحيح: {UserId} - Invalid user ID provided", userId);
                    return ServiceResult<IEnumerable<Borrowing>>.Failure("معرف المستخدم غير صحيح - Invalid user ID");
                }

                _logger.LogDebug("الحصول على جميع استعارات المستخدم {UserId} - Getting all borrowings for user", userId);

                var borrowings = await _borrowingRepository.GetUserBorrowingsAsync(userId);

                _logger.LogDebug("تم الحصول على {Count} استعارة للمستخدم {UserId} - Retrieved borrowings for user",
                    borrowings.Count(), userId);

                return ServiceResult<IEnumerable<Borrowing>>.Success(borrowings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على استعارات المستخدم {UserId} - Error getting borrowings for user",
                    userId);
                return ServiceResult<IEnumerable<Borrowing>>.Failure("حدث خطأ أثناء الحصول على الاستعارات - An error occurred while retrieving borrowings");
            }
        }

        /// <summary>
        /// الحصول على الاستعارات المتأخرة
        /// Get overdue borrowings
        /// </summary>
        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetOverdueBorrowingsAsync()
        {
            try
            {
                _logger.LogDebug("الحصول على الاستعارات المتأخرة - Getting overdue borrowings");

                var borrowings = await _borrowingRepository.GetOverdueBorrowingsAsync();

                _logger.LogDebug("تم الحصول على {Count} استعارة متأخرة - Retrieved overdue borrowings",
                    borrowings.Count());

                return ServiceResult<IEnumerable<Borrowing>>.Success(borrowings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الاستعارات المتأخرة - Error getting overdue borrowings");
                return ServiceResult<IEnumerable<Borrowing>>.Failure("حدث خطأ أثناء الحصول على الاستعارات المتأخرة - An error occurred while retrieving overdue borrowings");
            }
        }

        /// <summary>
        /// الحصول على جميع الاستعارات النشطة
        /// Get all active borrowings
        /// </summary>
        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetActiveBorrowingsAsync()
        {
            try
            {
                _logger.LogDebug("الحصول على جميع الاستعارات النشطة - Getting all active borrowings");

                var borrowings = await _borrowingRepository.GetActiveBorrowingsAsync();

                _logger.LogDebug("تم الحصول على {Count} استعارة نشطة - Retrieved active borrowings",
                    borrowings.Count());

                return ServiceResult<IEnumerable<Borrowing>>.Success(borrowings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الاستعارات النشطة - Error getting active borrowings");
                return ServiceResult<IEnumerable<Borrowing>>.Failure("حدث خطأ أثناء الحصول على الاستعارات النشطة - An error occurred while retrieving active borrowings");
            }
        }

        /// <summary>
        /// الحصول على جميع الاستعارات (نشطة ومرجعة)
        /// Get all borrowings (active and returned)
        /// </summary>
        public async Task<ServiceResult<IEnumerable<Borrowing>>> GetAllBorrowingsAsync()
        {
            try
            {
                _logger.LogDebug("الحصول على جميع الاستعارات - Getting all borrowings");

                var borrowings = await _borrowingRepository.GetAllAsync();

                _logger.LogDebug("تم الحصول على {Count} استعارة - Retrieved borrowings",
                    borrowings.Count());

                return ServiceResult<IEnumerable<Borrowing>>.Success(borrowings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على جميع الاستعارات - Error getting all borrowings");
                return ServiceResult<IEnumerable<Borrowing>>.Failure("حدث خطأ أثناء الحصول على الاستعارات - An error occurred while retrieving borrowings");
            }
        }

        /// <summary>
        /// الحصول على استعارة بالمعرف
        /// Get borrowing by ID
        /// </summary>
        public async Task<ServiceResult<Borrowing>> GetBorrowingByIdAsync(int borrowingId)
        {
            try
            {
                if (borrowingId <= 0)
                {
                    _logger.LogWarning("تم تمرير معرف استعارة غير صحيح: {BorrowingId} - Invalid borrowing ID provided", borrowingId);
                    return ServiceResult<Borrowing>.Failure("معرف الاستعارة غير صحيح - Invalid borrowing ID");
                }

                _logger.LogDebug("الحصول على الاستعارة {BorrowingId} - Getting borrowing", borrowingId);

                var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);

                if (borrowing == null)
                {
                    _logger.LogWarning("لم يتم العثور على الاستعارة {BorrowingId} - Borrowing not found", borrowingId);
                    return ServiceResult<Borrowing>.Failure("لم يتم العثور على الاستعارة - Borrowing not found");
                }

                _logger.LogDebug("تم العثور على الاستعارة {BorrowingId} بنجاح - Successfully found borrowing", borrowingId);

                return ServiceResult<Borrowing>.Success(borrowing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الاستعارة {BorrowingId} - Error getting borrowing", borrowingId);
                return ServiceResult<Borrowing>.Failure("حدث خطأ أثناء الحصول على الاستعارة - An error occurred while retrieving the borrowing");
            }
        }

        /// <summary>
        /// حساب الرسوم المتأخرة
        /// Calculate late fees
        /// </summary>
        public async Task<ServiceResult<decimal>> CalculateLateFeeAsync(int borrowingId)
        {
            try
            {
                _logger.LogDebug("حساب الرسوم المتأخرة للاستعارة {BorrowingId} - Calculating late fees for borrowing", borrowingId);

                var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
                if (borrowing == null)
                {
                    _logger.LogWarning("لم يتم العثور على الاستعارة {BorrowingId} - Borrowing not found", borrowingId);
                    return ServiceResult<decimal>.Failure("لم يتم العثور على الاستعارة - Borrowing not found");
                }

                if (borrowing.IsReturned)
                {
                    // إذا تم الإرجاع، إرجاع الرسوم المحفوظة
                    // If returned, return saved fees
                    return ServiceResult<decimal>.Success(borrowing.LateFee);
                }

                var currentDate = DateTime.Now;
                var daysLate = (currentDate - borrowing.DueDate).Days - _librarySettings.GraceDays;

                if (daysLate <= 0)
                {
                    // لا توجد رسوم متأخرة
                    // No late fees
                    return ServiceResult<decimal>.Success(0);
                }

                var lateFee = daysLate * _librarySettings.LateFeePerDay;

                _logger.LogDebug("الرسوم المتأخرة للاستعارة {BorrowingId}: {LateFee} ({DaysLate} أيام) - Late fees for borrowing",
                    borrowingId, lateFee, daysLate);

                return ServiceResult<decimal>.Success(lateFee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حساب الرسوم المتأخرة للاستعارة {BorrowingId} - Error calculating late fees for borrowing", borrowingId);
                return ServiceResult<decimal>.Failure("حدث خطأ أثناء حساب الرسوم المتأخرة - An error occurred while calculating late fees");
            }
        }

        /// <summary>
        /// التحقق من إمكانية استعارة كتاب
        /// Check if user can borrow a book
        /// </summary>
        public async Task<ServiceResult<BorrowingEligibility>> CheckBorrowingEligibilityAsync(int userId, int bookId)
        {
            try
            {
                _logger.LogDebug("فحص أهلية الاستعارة للمستخدم {UserId} والكتاب {BookId} - Checking borrowing eligibility for user and book",
                    userId, bookId);

                var eligibility = new BorrowingEligibility();

                // التحقق من صحة المعرفات
                // Validate IDs
                if (userId <= 0 || bookId <= 0)
                {
                    return ServiceResult<BorrowingEligibility>.Success(BorrowingEligibility.NotEligible("معرفات غير صحيحة - Invalid IDs"));
                }

                // التحقق من وجود المستخدم وحالته
                // Check user existence and status
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<BorrowingEligibility>.Success(BorrowingEligibility.NotEligible("المستخدم غير موجود - User not found"));
                }

                eligibility.IsUserActive = user.IsActive;
                if (!user.IsActive)
                {
                    return ServiceResult<BorrowingEligibility>.Success(BorrowingEligibility.NotEligible("المستخدم غير نشط - User is inactive"));
                }

                // التحقق من وجود الكتاب وتوفره
                // Check book existence and availability
                var book = await _bookRepository.GetByIdAsync(bookId);
                if (book == null)
                {
                    return ServiceResult<BorrowingEligibility>.Success(BorrowingEligibility.NotEligible("الكتاب غير موجود - Book not found"));
                }

                eligibility.IsBookAvailable = book.AvailableCopies > 0;
                if (book.AvailableCopies <= 0)
                {
                    return ServiceResult<BorrowingEligibility>.Success(BorrowingEligibility.NotEligible("الكتاب غير متاح - Book not available"));
                }

                // التحقق من عدد الكتب المستعارة حالياً
                // Check current borrowed books count
                var currentBorrowedCount = await _borrowingRepository.GetCurrentBorrowedBooksCountAsync(userId);
                eligibility.CurrentBorrowedBooks = currentBorrowedCount;
                eligibility.MaxBorrowingLimit = _librarySettings.MaxBooksPerUser;

                if (currentBorrowedCount >= _librarySettings.MaxBooksPerUser)
                {
                    return ServiceResult<BorrowingEligibility>.Success(BorrowingEligibility.NotEligible($"تم الوصول للحد الأقصى للاستعارة ({_librarySettings.MaxBooksPerUser} كتب) - Maximum borrowing limit reached ({_librarySettings.MaxBooksPerUser} books)"));
                }

                // التحقق من وجود استعارة نشطة لنفس الكتاب
                // Check for active borrowing of the same book
                var canBorrow = await _borrowingRepository.CanBorrowBookAsync(userId, bookId);
                if (!canBorrow)
                {
                    return ServiceResult<BorrowingEligibility>.Success(BorrowingEligibility.NotEligible("لديك استعارة نشطة لنفس الكتاب - You have an active borrowing for the same book"));
                }

                // التحقق من الكتب المتأخرة
                // Check overdue books
                var userBorrowings = await _borrowingRepository.GetActiveUserBorrowingsAsync(userId);
                var overdueCount = userBorrowings.Count(b => b.IsOverdue);
                eligibility.OverdueBooks = overdueCount;

                if (overdueCount > 0)
                {
                    return ServiceResult<BorrowingEligibility>.Success(BorrowingEligibility.NotEligible($"لديك {overdueCount} كتاب متأخر، يجب إرجاعها أولاً - You have {overdueCount} overdue book(s), please return them first"));
                }

                // جميع الشروط مستوفاة
                // All conditions met
                eligibility.CanBorrow = true;

                _logger.LogDebug("المستخدم {UserId} مؤهل لاستعارة الكتاب {BookId} - User is eligible to borrow book",
                    userId, bookId);

                return ServiceResult<BorrowingEligibility>.Success(eligibility);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في فحص أهلية الاستعارة للمستخدم {UserId} والكتاب {BookId} - Error checking borrowing eligibility for user and book",
                    userId, bookId);
                return ServiceResult<BorrowingEligibility>.Failure("حدث خطأ أثناء فحص أهلية الاستعارة - An error occurred while checking borrowing eligibility");
            }
        }

        /// <summary>
        /// الحصول على إحصائيات الاستعارات
        /// Get borrowing statistics
        /// </summary>
        public async Task<ServiceResult<BorrowingStatistics>> GetBorrowingStatisticsAsync()
        {
            try
            {
                _logger.LogDebug("الحصول على إحصائيات الاستعارات - Getting borrowing statistics");

                var statistics = await _borrowingRepository.GetBorrowingStatisticsAsync();

                _logger.LogDebug("تم الحصول على إحصائيات الاستعارات بنجاح - Successfully retrieved borrowing statistics");

                return ServiceResult<BorrowingStatistics>.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على إحصائيات الاستعارات - Error getting borrowing statistics");
                return ServiceResult<BorrowingStatistics>.Failure("حدث خطأ أثناء الحصول على إحصائيات الاستعارات - An error occurred while retrieving borrowing statistics");
            }
        }

        /// <summary>
        /// الحصول على الكتب الأكثر استعارة
        /// Get most borrowed books
        /// </summary>
        public async Task<ServiceResult<IEnumerable<MostBorrowedBook>>> GetMostBorrowedBooksAsync(int topCount = 10)
        {
            try
            {
                _logger.LogDebug("الحصول على أكثر {TopCount} كتاب استعارة - Getting top most borrowed books", topCount);

                var books = await _borrowingRepository.GetMostBorrowedBooksAsync(topCount);

                _logger.LogDebug("تم الحصول على {Count} كتاب من الأكثر استعارة - Retrieved most borrowed books", books.Count());

                return ServiceResult<IEnumerable<MostBorrowedBook>>.Success(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على الكتب الأكثر استعارة - Error getting most borrowed books");
                return ServiceResult<IEnumerable<MostBorrowedBook>>.Failure("حدث خطأ أثناء الحصول على الكتب الأكثر استعارة - An error occurred while retrieving most borrowed books");
            }
        }

        /// <summary>
        /// الحصول على المستخدمين الأكثر نشاطاً
        /// Get most active users
        /// </summary>
        public async Task<ServiceResult<IEnumerable<MostActiveUser>>> GetMostActiveUsersAsync(int topCount = 10)
        {
            try
            {
                _logger.LogDebug("الحصول على أكثر {TopCount} مستخدم نشاطاً - Getting top most active users", topCount);

                var users = await _borrowingRepository.GetMostActiveUsersAsync(topCount);

                _logger.LogDebug("تم الحصول على {Count} مستخدم من الأكثر نشاطاً - Retrieved most active users", users.Count());

                return ServiceResult<IEnumerable<MostActiveUser>>.Success(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدمين الأكثر نشاطاً - Error getting most active users");
                return ServiceResult<IEnumerable<MostActiveUser>>.Failure("حدث خطأ أثناء الحصول على المستخدمين الأكثر نشاطاً - An error occurred while retrieving most active users");
            }
        }

        /// <summary>
        /// تمديد فترة الاستعارة
        /// Extend borrowing period
        /// </summary>
        public async Task<ServiceResult<bool>> ExtendBorrowingAsync(int borrowingId, int additionalDays)
        {
            try
            {
                _logger.LogDebug("تمديد فترة الاستعارة {BorrowingId} بـ {AdditionalDays} أيام - Extending borrowing by days",
                    borrowingId, additionalDays);

                if (additionalDays <= 0)
                {
                    return ServiceResult<bool>.Failure("عدد الأيام الإضافية يجب أن يكون أكبر من صفر - Additional days must be greater than zero");
                }

                var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
                if (borrowing == null)
                {
                    return ServiceResult<bool>.Failure("لم يتم العثور على الاستعارة - Borrowing not found");
                }

                if (borrowing.IsReturned)
                {
                    return ServiceResult<bool>.Failure("لا يمكن تمديد كتاب تم إرجاعه - Cannot extend a returned book");
                }

                borrowing.DueDate = borrowing.DueDate.AddDays(additionalDays);
                var success = await _borrowingRepository.UpdateAsync(borrowing);

                if (success)
                {
                    _logger.LogInformation("تم تمديد فترة الاستعارة {BorrowingId} بنجاح - Successfully extended borrowing", borrowingId);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تمديد فترة الاستعارة {BorrowingId} - Error extending borrowing", borrowingId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء تمديد فترة الاستعارة - An error occurred while extending the borrowing period");
            }
        }

        /// <summary>
        /// تجديد استعارة
        /// Renew borrowing
        /// </summary>
        public async Task<ServiceResult<bool>> RenewBorrowingAsync(int borrowingId)
        {
            try
            {
                _logger.LogDebug("تجديد الاستعارة {BorrowingId} - Renewing borrowing", borrowingId);

                return await ExtendBorrowingAsync(borrowingId, _librarySettings.RenewalDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تجديد الاستعارة {BorrowingId} - Error renewing borrowing", borrowingId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء تجديد الاستعارة - An error occurred while renewing the borrowing");
            }
        }

        /// <summary>
        /// إلغاء استعارة
        /// Cancel borrowing
        /// </summary>
        public async Task<ServiceResult<bool>> CancelBorrowingAsync(int borrowingId)
        {
            try
            {
                _logger.LogDebug("إلغاء الاستعارة {BorrowingId} - Cancelling borrowing", borrowingId);

                var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
                if (borrowing == null)
                {
                    return ServiceResult<bool>.Failure("لم يتم العثور على الاستعارة - Borrowing not found");
                }

                if (borrowing.IsReturned)
                {
                    return ServiceResult<bool>.Failure("لا يمكن إلغاء كتاب تم إرجاعه - Cannot cancel a returned book");
                }

                var success = await _borrowingRepository.DeleteAsync(borrowingId);

                if (success)
                {
                    _logger.LogInformation("تم إلغاء الاستعارة {BorrowingId} بنجاح - Successfully cancelled borrowing", borrowingId);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إلغاء الاستعارة {BorrowingId} - Error cancelling borrowing", borrowingId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء إلغاء الاستعارة - An error occurred while cancelling the borrowing");
            }
        }
    }
}
