using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Authorization;
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
                 IsAdmin = IsAdmin();
                // إذا لم يكن المستخدم مدير، عرض استعاراته فقط
                // If user is not admin, show only their borrowings
                if (!IsAdmin)
                {
                    var currentUserId = HttpContext.Session.GetInt32("UserId");
                    _logger.LogDebug("Current user ID from session: {UserId}", currentUserId);

                    if (currentUserId.HasValue)
                    {
                        _logger.LogDebug("Setting UserId to  for non-admin user");
                    }
                    else
                    {
                        _logger.LogWarning("No valid user ID found in session, redirecting to login");
                        return RedirectToPage("/Auth/Login");
                    }
                }

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
        /// استعارة كتاب
        /// Borrow a book
        /// </summary>
        public async Task<JsonResult> OnPostBorrowBookAsync(int bookId)
        {
            try
            {
                if (bookId <= 0)
                {
                    return new JsonResult(new { success = false, message = "معرف الكتاب غير صحيح - Invalid book ID" });
                }

                int userId = GetCurrentUserIdForBorrowing();
                if (userId <= 0)
                {
                    return new JsonResult(new { success = false, message = "يرجى تسجيل الدخول أولاً - Please login first" });
                }

                var borrowResult = await _borrowingService.BorrowBookAsync(userId, bookId, 14);
                if (borrowResult.IsSuccess)
                {
                    return new JsonResult(new { success = true, message = $"تم استعارة الكتاب بنجاح! رقم الاستعارة: {borrowResult.Data}" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = borrowResult.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استعارة الكتاب {BookId}", bookId);
                return new JsonResult(new { success = false, message = "حدث خطأ في الخادم أثناء استعارة الكتاب" });
            }
        }
   

        public async Task<IActionResult> OnPostDeleteBookAsync(int bookId)
        {
            try
            {
                if (bookId <= 0)
                {
                    return new JsonResult(new { success = false, message = "معرف الكتاب غير صحيح - Invalid book ID" });
                }

                int userId = GetCurrentUserIdForBorrowing();
                if (userId <= 0)
                {
                    return new JsonResult(new { success = false, message = "يرجى تسجيل الدخول أولاً - Please login first" });
                }

                var deleteResult = await _bookService.DeleteBookAsync(bookId, userId);

                if (deleteResult.IsSuccess)
                {
                    _logger.LogInformation("تم حذف الكتاب بنجاح - Book deleted successfully. BookId: {BookId}", bookId);
                    return new JsonResult(new { success = true, message = "تم حذف الكتاب بنجاح - Book deleted successfully" });
                }
                else
                {
                    _logger.LogWarning("فشل في حذف الكتاب: {Error} - Failed to delete book", deleteResult.ValidationErrors);
                    return new JsonResult(new { success = false, message = deleteResult.ValidationErrors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الكتاب {BookId} - Error deleting book", bookId);
                return new JsonResult(new { success = false, message = "حدث خطأ في الخادم أثناء حذف الكتاب - Server error occurred while deleting book" });
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

    }
}
