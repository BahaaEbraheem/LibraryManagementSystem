using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages.Books
{
    /// <summary>
    /// نموذج صفحة تفاصيل الكتاب
    /// Book details page model
    /// </summary>
    public class DetailsModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<DetailsModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public DetailsModel(IBookService bookService, IBorrowingService borrowingService, ILogger<DetailsModel> logger)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _borrowingService = borrowingService ?? throw new ArgumentNullException(nameof(borrowingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الكتاب المحدد
        /// Selected book
        /// </summary>
        public Book? Book { get; set; }

        /// <summary>
        /// هل المستخدم الحالي مدير
        /// Whether the current user is an admin
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// رسالة الخطأ في حالة حدوث مشكلة
        /// Error message in case of issues
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// رسالة النجاح
        /// Success message
        /// </summary>
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// معالج طلب GET - تحميل تفاصيل الكتاب
        /// GET request handler - load book details
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // التحقق من دور المستخدم
                // Check user role
                IsAdmin = HttpContext.Session.GetString("UserRole") == "Administrator";

                if (id <= 0)
                {
                    ErrorMessage = "معرف الكتاب غير صحيح - Invalid book ID";
                    return Page();
                }

                _logger.LogDebug("تحميل تفاصيل الكتاب {BookId} - Loading book details", id);

                var result = await _bookService.GetBookByIdAsync(id);

                if (result.IsSuccess && result.Data != null)
                {
                    Book = result.Data;
                    _logger.LogInformation("تم تحميل تفاصيل الكتاب {BookId} بنجاح - Successfully loaded book details", id);
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "لم يتم العثور على الكتاب - Book not found";
                    _logger.LogWarning("فشل في تحميل تفاصيل الكتاب {BookId}: {Error} - Failed to load book details", 
                        id, ErrorMessage);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل تفاصيل الكتاب {BookId} - Error loading book details", id);
                ErrorMessage = "حدث خطأ أثناء تحميل تفاصيل الكتاب - An error occurred while loading book details";
                return Page();
            }
        }

        /// <summary>
        /// معالج طلب POST لاستعارة كتاب
        /// POST request handler for borrowing a book
        /// </summary>
        public async Task<IActionResult> OnPostBorrowAsync(int bookId)
        {
            try
            {
                if (bookId <= 0)
                {
                    ErrorMessage = "معرف الكتاب غير صحيح - Invalid book ID";
                    return RedirectToPage(new { id = bookId });
                }

                // الحصول على معرف المستخدم الحالي
                // Get current user ID
                var userIdString = HttpContext.Session.GetString("UserId");
                if (!int.TryParse(userIdString, out int userId) || userId <= 0)
                {
                    ErrorMessage = "يرجى تسجيل الدخول أولاً - Please login first";
                    return RedirectToPage(new { id = bookId });
                }

                _logger.LogInformation("محاولة استعارة الكتاب {BookId} للمستخدم {UserId} - Attempting to borrow book for user",
                    bookId, userId);

                var result = await _borrowingService.BorrowBookAsync(userId, bookId);

                if (result.IsSuccess)
                {
                    SuccessMessage = "تم استعارة الكتاب بنجاح! - Book borrowed successfully!";
                    _logger.LogInformation("تم استعارة الكتاب {BookId} بواسطة المستخدم {UserId} بنجاح - Book borrowed successfully by user",
                        bookId, userId);
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "فشل في استعارة الكتاب - Failed to borrow book";
                    _logger.LogWarning("فشل في استعارة الكتاب {BookId} للمستخدم {UserId}: {Error} - Failed to borrow book for user",
                        bookId, userId, ErrorMessage);
                }

                return RedirectToPage(new { id = bookId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استعارة الكتاب {BookId} - Error borrowing book", bookId);
                ErrorMessage = "حدث خطأ أثناء عملية الاستعارة - An error occurred during the borrowing process";
                return RedirectToPage(new { id = bookId });
            }
        }

        /// <summary>
        /// الحصول على معرف المستخدم الحالي
        /// Get current user ID
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            return int.TryParse(userIdString, out int userId) ? userId : 0;
        }
    }
}
