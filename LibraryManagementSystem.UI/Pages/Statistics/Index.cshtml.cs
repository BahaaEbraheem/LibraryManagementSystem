using Microsoft.AspNetCore.Mvc.RazorPages;
using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.DAL.Repositories;

namespace LibraryManagementSystem.UI.Pages.Statistics
{
    public class IndexModel : PageModel
    {
        private readonly IBookService _bookService;
        private readonly IBorrowingService _borrowingService;
        private readonly IUserService _userService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IBookService bookService,
            IBorrowingService borrowingService,
            IUserService userService,
            ILogger<IndexModel> logger)
        {
            _bookService = bookService;
            _borrowingService = borrowingService;
            _userService = userService;
            _logger = logger;
        }

        public BookStatistics? BookStatistics { get; set; }
        public BorrowingStatistics? BorrowingStatistics { get; set; }
        public IEnumerable<MostBorrowedBook> MostBorrowedBooks { get; set; } = new List<MostBorrowedBook>();
        public IEnumerable<MostActiveUser> MostActiveUsers { get; set; } = new List<MostActiveUser>();
        public int UserCount { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogDebug("تحميل صفحة الإحصائيات - Loading statistics page");

                // تحميل إحصائيات الكتب
                var bookStatsResult = await _bookService.GetBookStatisticsAsync();
                BookStatistics = bookStatsResult.IsSuccess ? bookStatsResult.Data : new BookStatistics();

                // تحميل إحصائيات الاستعارات
                var borrowingStatsResult = await _borrowingService.GetBorrowingStatisticsAsync();
                BorrowingStatistics = borrowingStatsResult.IsSuccess ? borrowingStatsResult.Data : new BorrowingStatistics();

                // تحميل الكتب الأكثر استعارة
                var mostBorrowedResult = await _borrowingService.GetMostBorrowedBooksAsync(10);
                MostBorrowedBooks = mostBorrowedResult.IsSuccess ? mostBorrowedResult.Data! : new List<MostBorrowedBook>();

                // تحميل المستخدمين الأكثر نشاطاً
                var mostActiveResult = await _borrowingService.GetMostActiveUsersAsync(10);
                MostActiveUsers = mostActiveResult.IsSuccess ? mostActiveResult.Data! : new List<MostActiveUser>();

                // تحميل عدد المستخدمين
                var users = await _userService.GetAllUsersAsync();
                UserCount = users.Count();

                _logger.LogDebug("تم تحميل الإحصائيات بنجاح - Statistics loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل الإحصائيات - Error loading statistics");

                // تعيين قيم افتراضية في حالة الخطأ
                BookStatistics = new BookStatistics();
                BorrowingStatistics = new BorrowingStatistics();
                MostBorrowedBooks = new List<MostBorrowedBook>();
                MostActiveUsers = new List<MostActiveUser>();
                UserCount = 0;
            }
        }
    }
}
