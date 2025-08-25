using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages
{
    /// <summary>
    /// نموذج الصفحة الرئيسية
    /// Home page model
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<IndexModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public IndexModel(
            IBookService bookService,
            IBorrowingService borrowingService,
            ILogger<IndexModel> logger)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _borrowingService = borrowingService ?? throw new ArgumentNullException(nameof(borrowingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// إحصائيات الكتب
        /// Book statistics
        /// </summary>
        public BookStatistics? BookStatistics { get; set; }

        /// <summary>
        /// إحصائيات الاستعارات
        /// Borrowing statistics
        /// </summary>
        public BorrowingStatistics? BorrowingStatistics { get; set; }

        /// <summary>
        /// الكتب الأكثر استعارة
        /// Most borrowed books
        /// </summary>
        public IEnumerable<MostBorrowedBook>? MostBorrowedBooks { get; set; }

        /// <summary>
        /// المستخدمين الأكثر نشاطاً
        /// Most active users
        /// </summary>
        public IEnumerable<MostActiveUser>? MostActiveUsers { get; set; }

        /// <summary>
        /// رسالة الخطأ
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// معالج طلب GET - تحميل الصفحة الرئيسية
        /// GET request handler - load home page
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // التحقق من تسجيل الدخول وإعادة التوجيه
            // Check login status and redirect
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Auth/Login");
            }

            // إعادة التوجيه إلى صفحة الكتب للمستخدمين المسجلين
            // Redirect logged-in users to Books page
            return RedirectToPage("/Books/Index");
        }

        /// <summary>
        /// تحميل إحصائيات الكتب
        /// Load book statistics
        /// </summary>
        private async Task LoadBookStatisticsAsync()
        {
            try
            {
                var result = await _bookService.GetBookStatisticsAsync();
                if (result.IsSuccess)
                {
                    BookStatistics = result.Data;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "خطأ في تحميل إحصائيات الكتب - Error loading book statistics");
            }
        }

        /// <summary>
        /// تحميل إحصائيات الاستعارات
        /// Load borrowing statistics
        /// </summary>
        private async Task LoadBorrowingStatisticsAsync()
        {
            try
            {
                var result = await _borrowingService.GetBorrowingStatisticsAsync();
                if (result.IsSuccess)
                {
                    BorrowingStatistics = result.Data;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل إحصائيات الاستعارات - Error loading borrowing statistics");
            }
        }

        /// <summary>
        /// تحميل الكتب الأكثر استعارة
        /// Load most borrowed books
        /// </summary>
        private async Task LoadMostBorrowedBooksAsync()
        {
            try
            {
                var result = await _borrowingService.GetMostBorrowedBooksAsync(10);
                if (result.IsSuccess)
                {
                    MostBorrowedBooks = result.Data;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل الكتب الأكثر استعارة - Error loading most borrowed books");
            }
        }

        /// <summary>
        /// تحميل المستخدمين الأكثر نشاطاً
        /// Load most active users
        /// </summary>
        private async Task LoadMostActiveUsersAsync()
        {
            try
            {
                var result = await _borrowingService.GetMostActiveUsersAsync(5);
                if (result.IsSuccess)
                {
                    MostActiveUsers = result.Data;
                    _logger.LogDebug("تم تحميل المستخدمين الأكثر نشاطاً - Most active users loaded");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل المستخدمين الأكثر نشاطاً - Error loading most active users");
            }
        }
    }
}
