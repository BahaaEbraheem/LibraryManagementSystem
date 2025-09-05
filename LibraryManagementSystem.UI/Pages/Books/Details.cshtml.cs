using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages.Books
{
    [Authorize]
    /// <summary>
    /// نموذج صفحة تفاصيل الكتاب
    /// Book details page model
    /// </summary>
    public class DetailsModel : BasePageModel
    {
        private readonly IBookService _bookService;
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<DetailsModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public DetailsModel(IBookService bookService, IBorrowingService borrowingService, IJwtService jwtService, ILogger<DetailsModel> logger) : base(jwtService)
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
        /// معالج طلب POST - حذف الكتاب
        /// POST request handler - delete book
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "معرف الكتاب غير صحيح - Invalid book ID";
                    return Page();
                }

                // التحقق من تسجيل الدخول والصلاحيات
                // Check login and permissions
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    TempData["ErrorMessage"] = "يجب تسجيل الدخول أولاً - Please login first";
                    return RedirectToPage("/Auth/Login");
                }

                // التحقق من صلاحيات المدير
                // Check admin permissions
                if (!IsAdmin())
                {
                    TempData["ErrorMessage"] = "ليس لديك صلاحية لحذف الكتب - You don't have permission to delete books";
                    return Page();
                }

                _logger.LogDebug("محاولة حذف الكتاب {BookId} بواسطة المستخدم {UserId} - Attempting to delete book by user", id, currentUserId.Value);

                var result = await _bookService.DeleteBookAsync(id, currentUserId.Value);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم حذف الكتاب {BookId} بنجاح - Successfully deleted book", id);
                    TempData["SuccessMessage"] = "تم حذف الكتاب بنجاح! - Book deleted successfully!";
                    return RedirectToPage("/Books/Index");
                }
                else
                {
                    _logger.LogWarning("فشل في حذف الكتاب {BookId}: {Error} - Failed to delete book", id, result.ErrorMessage);
                    TempData["ErrorMessage"] = result.ErrorMessage ?? "فشل في حذف الكتاب - Failed to delete book";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الكتاب {BookId} - Error deleting book", id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف الكتاب - An error occurred while deleting the book";
                return Page();
            }
        }
    }
}
