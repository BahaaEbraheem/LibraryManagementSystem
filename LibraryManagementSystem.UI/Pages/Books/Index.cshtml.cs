using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages.Books
{
    /// <summary>
    /// نموذج صفحة البحث عن الكتب
    /// Books search page model
    /// </summary>
    public class IndexModel : BasePageModel
    {
        private readonly IBookService _bookService;
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<IndexModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public IndexModel(IBookService bookService, IBorrowingService borrowingService, IJwtService jwtService, ILogger<IndexModel> logger) : base(jwtService)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _borrowingService = borrowingService ?? throw new ArgumentNullException(nameof(borrowingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// معايير البحث الحالية
        /// Current search criteria
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public BookSearchDto SearchCriteria { get; set; } = new BookSearchDto();

        /// <summary>
        /// نتائج البحث
        /// Search results
        /// </summary>
        public PagedResult<Book>? SearchResults { get; set; }

        /// <summary>
        /// رسالة الخطأ في حالة حدوث مشكلة
        /// Error message in case of issues
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// هل المستخدم الحالي مدير
        /// Whether the current user is an admin
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// رسالة النجاح
        /// Success message
        /// </summary>
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// معالج طلب GET - تحميل الصفحة والبحث
        /// GET request handler - load page and search
        /// </summary>
        public async Task<IActionResult> OnGetAsync(
            string? searchTerm,
            string? genre,
            bool availableOnly = false,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "Title",
            bool sortDescending = false)
        {
            try
            {
                // التحقق من دور المستخدم
                // Check user role
                IsAdmin = IsAdmin();

                // تعيين معايير البحث
                // Set search criteria
                SearchCriteria = new BookSearchDto
                {
                    SearchTerm = searchTerm,
                    Genre = genre,
                    AvailableOnly = availableOnly,
                    PageNumber = Math.Max(1, pageNumber),
                    PageSize = Math.Max(1, Math.Min(50, pageSize)), // الحد الأقصى 50 عنصر في الصفحة، افتراضي 10
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                // تسجيل معايير البحث
                // Log search criteria
                _logger.LogDebug("البحث عن الكتب بالمعايير: البحث={SearchTerm}, النوع={Genre}, متاح فقط={AvailableOnly} - " +
                    "Searching books with criteria: SearchTerm={SearchTerm}, Genre={Genre}, AvailableOnly={AvailableOnly}",
                    searchTerm, genre, availableOnly, searchTerm, genre, availableOnly);

                // تنفيذ البحث (سيعرض جميع الكتب إذا لم تكن هناك معايير بحث)
                // Execute search (will show all books if no search criteria)
                var searchResult = await _bookService.SearchBooksAsync(SearchCriteria);

                if (searchResult.IsSuccess)
                {
                    SearchResults = searchResult.Data;

                    // تسجيل النتائج
                    // Log results
                    _logger.LogInformation("تم العثور على {Count} كتاب من أصل - Found {Count} books out of ",
                        SearchResults!.Items.Count(), SearchResults.TotalCount);

                    // إضافة رسالة معلوماتية إذا لم يتم العثور على نتائج مع وجود معايير بحث
                    // Add informational message if no results found with search criteria
                    if (SearchResults.TotalCount == 0 && HasSearchCriteria())
                    {
                        TempData["InfoMessage"] = "لم يتم العثور على كتب تطابق معايير البحث المحددة. جرب تعديل معايير البحث أو استخدام كلمات مختلفة.";
                    }
                    else if (SearchResults.TotalCount > 0 && HasSearchCriteria())
                    {
                        // إضافة رسالة نجاح عند وجود معايير بحث
                        // Add success message when search criteria exist
                        TempData["SuccessMessage"] = $"تم العثور على {SearchResults.TotalCount} كتاب يطابق معايير البحث.";
                    }
                }
                else
                {
                    ErrorMessage = searchResult.ErrorMessage ?? "حدث خطأ أثناء البحث";
                    SearchResults = new PagedResult<Book>
                    {
                        Items = new List<Book>(),
                        TotalCount = 0,
                        PageNumber = SearchCriteria.PageNumber,
                        PageSize = SearchCriteria.PageSize
                    };
                }

                // عرض رسائل من TempData
                // Display messages from TempData
                if (TempData["SuccessMessage"] != null)
                {
                    SuccessMessage = TempData["SuccessMessage"]?.ToString();
                }
                if (TempData["ErrorMessage"] != null)
                {
                    ErrorMessage = TempData["ErrorMessage"]?.ToString();
                }

                return Page();
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                // Log error
                _logger.LogError(ex, "خطأ في البحث عن الكتب - Error searching for books");

                // عرض رسالة خطأ للمستخدم
                // Display error message to user
                ErrorMessage = "حدث خطأ أثناء البحث عن الكتب. يرجى المحاولة مرة أخرى.";
                TempData["ErrorMessage"] = ErrorMessage;

                // إرجاع نتائج فارغة
                // Return empty results
                SearchResults = new PagedResult<Book>
                {
                    Items = new List<Book>(),
                    TotalCount = 0,
                    PageNumber = SearchCriteria.PageNumber,
                    PageSize = SearchCriteria.PageSize
                };

                return Page();
            }
        }

        /// <summary>
        /// معالج طلب POST - تنفيذ البحث الجديد
        /// POST request handler - execute new search
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // التحقق من صحة النموذج
                // Validate model
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "يرجى التحقق من صحة البيانات المدخلة.";
                    return Page();
                }

                // إعادة توجيه إلى GET مع معايير البحث
                // Redirect to GET with search criteria
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(SearchCriteria.SearchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchCriteria.SearchTerm)}");

                if (!string.IsNullOrWhiteSpace(SearchCriteria.Genre))
                    queryParams.Add($"genre={Uri.EscapeDataString(SearchCriteria.Genre)}");

                queryParams.Add($"availableOnly={SearchCriteria.AvailableOnly}");
                queryParams.Add($"pageNumber={SearchCriteria.PageNumber}");
                queryParams.Add($"pageSize={SearchCriteria.PageSize}");
                queryParams.Add($"sortBy={SearchCriteria.SortBy}");
                queryParams.Add($"sortDescending={SearchCriteria.SortDescending}");

                var queryString = string.Join("&", queryParams);
                var redirectUrl = $"/Books?{queryString}";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في معالجة طلب البحث - Error processing search request");
                TempData["ErrorMessage"] = "حدث خطأ أثناء معالجة طلب البحث. يرجى المحاولة مرة أخرى.";
                return Page();
            }
        }

        /// <summary>
        /// معالج طلب POST لاستعارة كتاب
        /// POST request handler for borrowing a book
        /// </summary>
        public async Task<IActionResult> OnPostBorrowAsync(int bookId, int userId, int borrowingDays = 14)
        {
            try
            {
                if (bookId <= 0 || userId <= 0)
                {
                    return new JsonResult(new { success = false, message = "معرفات غير صحيحة - Invalid IDs" });
                }

                var result = await _borrowingService.BorrowBookAsync(userId, bookId, borrowingDays);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم استعارة الكتاب {BookId} بواسطة المستخدم {UserId} ",
                        bookId, userId);

                    return new JsonResult(new { success = true, message = "تم استعارة الكتاب بنجاح - Book borrowed successfully" });
                }
                else
                {
                    _logger.LogWarning("فشل في استعارة الكتاب {BookId} بواسطة المستخدم {UserId}: {Error} - Failed to borrow book by user",
                        bookId, userId, result.ErrorMessage);

                    return new JsonResult(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استعارة الكتاب {BookId} - Error borrowing book {BookId}", bookId, bookId);
                return new JsonResult(new { success = false, message = "حدث خطأ أثناء الاستعارة - An error occurred during borrowing" });
            }
        }

        /// <summary>
        /// التحقق من وجود معايير بحث
        /// Check if search criteria exist
        /// </summary>
        /// <returns>true إذا كانت هناك معايير بحث</returns>
        private bool HasSearchCriteria()
        {
            return !string.IsNullOrWhiteSpace(SearchCriteria.SearchTerm) ||
                   !string.IsNullOrWhiteSpace(SearchCriteria.Genre) ||
                   SearchCriteria.AvailableOnly;
        }

        /// <summary>
        /// التحقق من وجود معايير بحث صالحة
        /// Check if valid search criteria exist
        /// </summary>
        /// <returns>true إذا كانت هناك معايير بحث صالحة</returns>
        private bool HasValidSearchCriteria()
        {
            return (!string.IsNullOrWhiteSpace(SearchCriteria.SearchTerm) && SearchCriteria.SearchTerm.Trim().Length >= 2) ||
                   (!string.IsNullOrWhiteSpace(SearchCriteria.Title) && SearchCriteria.Title.Trim().Length >= 2) ||
                   (!string.IsNullOrWhiteSpace(SearchCriteria.Author) && SearchCriteria.Author.Trim().Length >= 2) ||
                   (!string.IsNullOrWhiteSpace(SearchCriteria.ISBN) && SearchCriteria.ISBN.Trim().Length >= 3) ||
                   !string.IsNullOrWhiteSpace(SearchCriteria.Genre) ||
                   SearchCriteria.AvailableOnly ||
                   SearchCriteria.IsAvailable.HasValue;
        }

        /// <summary>
        /// استعارة كتاب
        /// Borrow a book
        /// </summary>
        public async Task<IActionResult> OnPostBorrowBookAsync(int bookId)
        {
            try
            {
                if (bookId <= 0)
                {
                    TempData["ErrorMessage"] = "معرف الكتاب غير صحيح - Invalid book ID";
                    return RedirectToPage();
                }

                // الحصول على معرف المستخدم الحالي من JWT
                // Get current user ID from JWT
                int userId = GetCurrentUserIdForBorrowing();

                if (userId <= 0)
                {
                    TempData["ErrorMessage"] = "يرجى تسجيل الدخول أولاً - Please login first";
                    return RedirectToPage();
                }

                _logger.LogInformation("محاولة استعارة الكتاب {BookId} للمستخدم {UserId} - Attempting to borrow book for user",
                    bookId, userId);

                // التحقق من أهلية الاستعارة أولاً
                // Check borrowing eligibility first
                var eligibilityResult = await _borrowingService.CheckBorrowingEligibilityAsync(userId, bookId);

                if (!eligibilityResult.IsSuccess)
                {
                    _logger.LogWarning("فشل في التحقق من أهلية الاستعارة: {Error} - Failed to check borrowing eligibility",
                        eligibilityResult.ErrorMessage);
                    TempData["ErrorMessage"] = eligibilityResult.ErrorMessage;
                    return RedirectToPage();
                }

                if (!eligibilityResult.Data!.CanBorrow)
                {
                    _logger.LogWarning("المستخدم غير مؤهل للاستعارة: {Reason} - User not eligible for borrowing",
                        eligibilityResult.Data.Reason);
                    TempData["ErrorMessage"] = eligibilityResult.Data.Reason;
                    return RedirectToPage();
                }

                // تنفيذ الاستعارة
                // Execute borrowing
                var borrowResult = await _borrowingService.BorrowBookAsync(userId, bookId, 14);

                if (borrowResult.IsSuccess)
                {
                    _logger.LogInformation("تم استعارة الكتاب بنجاح - Book borrowed successfully. BorrowingId: {BorrowingId}",
                        borrowResult.Data);

                    TempData["SuccessMessage"] = $"تم استعارة الكتاب بنجاح! رقم الاستعارة: {borrowResult.Data} - Book borrowed successfully! Borrowing ID: {borrowResult.Data}";

                    // إعادة توجيه مع الحفاظ على معايير البحث الحالية
                    // Redirect while preserving current search criteria
                    return RedirectToPage(new
                    {
                        searchTerm = SearchCriteria.SearchTerm,
                        genre = SearchCriteria.Genre,
                        availableOnly = SearchCriteria.AvailableOnly,
                        pageNumber = SearchCriteria.PageNumber,
                        pageSize = SearchCriteria.PageSize,
                        sortBy = SearchCriteria.SortBy,
                        sortDescending = SearchCriteria.SortDescending
                    });
                }
                else
                {
                    _logger.LogWarning("فشل في استعارة الكتاب: {Error} - Failed to borrow book",
                        borrowResult.ErrorMessage);
                    TempData["ErrorMessage"] = borrowResult.ErrorMessage;
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استعارة الكتاب {BookId} - Error borrowing book", bookId);
                TempData["ErrorMessage"] = "حدث خطأ في الخادم أثناء استعارة الكتاب - Server error occurred while borrowing book";
                return RedirectToPage();
            }
        }
        public async Task<IActionResult> OnPostDeleteBookAsync(int bookId)
        {
            try
            {
                if (bookId <= 0)
                {
                    TempData["ErrorMessage"] = "معرف الكتاب غير صحيح - Invalid book ID";
                    return RedirectToPage();
                }

                int userId = GetCurrentUserIdForBorrowing();
                if (userId <= 0)
                {
                    TempData["ErrorMessage"] = "يرجى تسجيل الدخول أولاً - Please login first";
                    return RedirectToPage();
                }

                _logger.LogInformation("محاولة حذف الكتاب {BookId} من قبل المستخدم {UserId} - Attempting to delete book",
                    bookId, userId);

                // تنفيذ الحذف
                var deleteResult = await _bookService.DeleteBookAsync(bookId, userId);

                if (deleteResult.IsSuccess)
                {
                    _logger.LogInformation("تم حذف الكتاب بنجاح - Book deleted successfully. BookId: {BookId}", bookId);

                    TempData["SuccessMessage"] = $"تم حذف الكتاب بنجاح! - Book deleted successfully!";

                    // إعادة التوجيه مع الحفاظ على معايير البحث
                    return RedirectToPage(new
                    {
                        searchTerm = SearchCriteria.SearchTerm,
                        genre = SearchCriteria.Genre,
                        availableOnly = SearchCriteria.AvailableOnly,
                        pageNumber = SearchCriteria.PageNumber,
                        pageSize = SearchCriteria.PageSize,
                        sortBy = SearchCriteria.SortBy,
                        sortDescending = SearchCriteria.SortDescending
                    });
                }
                else
                {
                    _logger.LogWarning("فشل في حذف الكتاب: {Error} - Failed to delete book", deleteResult.ErrorMessage);
                    TempData["ErrorMessage"] = deleteResult.ErrorMessage;
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الكتاب {BookId} - Error deleting book", bookId);
                TempData["ErrorMessage"] = "حدث خطأ في الخادم أثناء حذف الكتاب - Server error occurred while deleting book";
                return RedirectToPage();
            }
        }


        /// <summary>
        /// الحصول على معرف المستخدم الحالي
        /// Get current user ID
        /// </summary>
        /// <returns>معرف المستخدم</returns>
        private int GetCurrentUserIdForBorrowing()
        {
            var userId = GetCurrentUserId();
            return userId ?? 0;
        }



        /// <summary>
        /// الحصول على رابط الصفحة مع رقم صفحة محدد
        /// Get page URL with specific page number
        /// </summary>
        /// <param name="pageNumber">رقم الصفحة</param>
        /// <returns>رابط الصفحة</returns>
        public string GetPageUrl(int pageNumber)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(SearchCriteria.SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchCriteria.SearchTerm)}");

            if (!string.IsNullOrWhiteSpace(SearchCriteria.Genre))
                queryParams.Add($"genre={Uri.EscapeDataString(SearchCriteria.Genre)}");

            if (SearchCriteria.AvailableOnly)
                queryParams.Add($"availableOnly={SearchCriteria.AvailableOnly}");

            queryParams.Add($"pageNumber={pageNumber}");
            queryParams.Add($"pageSize={SearchCriteria.PageSize}");
            queryParams.Add($"sortBy={SearchCriteria.SortBy}");
            queryParams.Add($"sortDescending={SearchCriteria.SortDescending}");

            var queryString = string.Join("&", queryParams);
            return $"/Books?{queryString}";
        }
    }
}
