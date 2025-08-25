using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Repositories;
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
        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogDebug("تحميل الصفحة الرئيسية - Loading home page");

                // تحميل البيانات بشكل متوازي لتحسين الأداء
                // Load data in parallel for better performance
                var tasks = new List<Task>
                {
                    LoadBookStatisticsAsync(),
                    LoadBorrowingStatisticsAsync(),
                    LoadMostBorrowedBooksAsync(),
                    LoadMostActiveUsersAsync()
                };

                await Task.WhenAll(tasks);

                _logger.LogInformation("تم تحميل الصفحة الرئيسية بنجاح - Home page loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل الصفحة الرئيسية - Error loading home page");
                ErrorMessage = "حدث خطأ أثناء تحميل البيانات - An error occurred while loading data";
            }
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
                    _logger.LogDebug("تم تحميل إحصائيات الكتب - Book statistics loaded");
                }
                else
                {
                    _logger.LogWarning("فشل في تحميل إحصائيات الكتب: {Error} - Failed to load book statistics: {Error}",
                        result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل إحصائيات الكتب - Error loading book statistics");
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
                    _logger.LogDebug("تم تحميل إحصائيات الاستعارات - Borrowing statistics loaded");
                }
                else
                {
                    _logger.LogWarning("فشل في تحميل إحصائيات الاستعارات: {Error} - Failed to load borrowing statistics: {Error}",
                        result.ErrorMessage);
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
                    _logger.LogDebug("تم تحميل الكتب الأكثر استعارة - Most borrowed books loaded");
                }
                else
                {
                    _logger.LogWarning("فشل في تحميل الكتب الأكثر استعارة: {Error} - Failed to load most borrowed books: {Error}",
                        result.ErrorMessage);
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
                else
                {
                    _logger.LogWarning("فشل في تحميل المستخدمين الأكثر نشاطاً: {Error} - Failed to load most active users: {Error}",
                        result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل المستخدمين الأكثر نشاطاً - Error loading most active users");
            }
        }
    }
}
