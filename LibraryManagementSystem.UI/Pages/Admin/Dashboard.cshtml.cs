using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages.Admin
{
    /// <summary>
    /// نموذج لوحة تحكم المدير
    /// Admin dashboard page model
    /// </summary>
    public class DashboardModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IBookService _bookService;
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IUserService userService,
            IBookService bookService,
            IBorrowingService borrowingService,
            ILogger<DashboardModel> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _borrowingService = borrowingService ?? throw new ArgumentNullException(nameof(borrowingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// إحصائيات النظام
        /// System statistics
        /// </summary>
        public SystemStatistics? Statistics { get; set; }

        /// <summary>
        /// المستخدم الحالي
        /// Current user
        /// </summary>
        public User? CurrentUser { get; set; }

        /// <summary>
        /// رسالة الخطأ
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// رسالة النجاح
        /// Success message
        /// </summary>
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// معالج طلب GET - تحميل لوحة التحكم
        /// GET request handler - load dashboard
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // التحقق من تسجيل الدخول والصلاحيات
                // Check login and permissions
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                var currentUserRole = HttpContext.Session.GetString("UserRole");

                if (!currentUserId.HasValue)
                {
                    return RedirectToPage("/Auth/Login", new { ReturnUrl = "/Admin/Dashboard" });
                }

                if (currentUserRole != UserRole.Administrator.ToString())
                {
                    return RedirectToPage("/Index");
                }

                // الحصول على بيانات المستخدم الحالي
                // Get current user data
                var userResult = await _userService.GetUserByIdAsync(currentUserId.Value);
                if (userResult.IsSuccess)
                {
                    CurrentUser = userResult.Data;
                }

                // تحميل الإحصائيات
                // Load statistics
                await LoadStatisticsAsync();

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
                _logger.LogError(ex, "خطأ في تحميل لوحة تحكم المدير - Error loading admin dashboard");
                ErrorMessage = "حدث خطأ أثناء تحميل لوحة التحكم - An error occurred while loading the dashboard";
                return Page();
            }
        }

        /// <summary>
        /// تحميل إحصائيات النظام
        /// Load system statistics
        /// </summary>
        private async Task LoadStatisticsAsync()
        {
            try
            {
                Statistics = new SystemStatistics();

                // إحصائيات المستخدمين
                // User statistics
                var users = await _userService.GetAllUsersAsync();
                if (users != null)
                {
                    var usersList = users.ToList();
                    Statistics.TotalUsers = usersList.Count;
                    Statistics.ActiveUsers = usersList.Count(u => u.IsActive);
                    Statistics.AdminUsers = usersList.Count(u => u.Role == UserRole.Administrator);
                }

                // إحصائيات الكتب
                // Book statistics
                var booksResult = await _bookService.SearchBooksAsync(new DAL.Models.DTOs.BookSearchDto { PageSize = int.MaxValue });
                if (booksResult.IsSuccess && booksResult.Data != null)
                {
                    var books = booksResult.Data.Items.ToList();
                    Statistics.TotalBooks = books.Count;
                    Statistics.AvailableBooks = books.Count(b => b.IsAvailable);
                    Statistics.TotalCopies = books.Sum(b => b.TotalCopies);
                    Statistics.AvailableCopies = books.Sum(b => b.AvailableCopies);
                }

                // إحصائيات الاستعارات
                // Borrowing statistics
                var borrowingStatsResult = await _borrowingService.GetBorrowingStatisticsAsync();
                if (borrowingStatsResult.IsSuccess && borrowingStatsResult.Data != null)
                {
                    var borrowingStats = borrowingStatsResult.Data;
                    Statistics.ActiveBorrowings = borrowingStats.ActiveBorrowings;
                    Statistics.OverdueBorrowings = borrowingStats.OverdueBorrowings;
                    Statistics.TotalBorrowings = borrowingStats.TotalBorrowings;
                    Statistics.ReturnedBorrowings = borrowingStats.ReturnedBorrowings;
                    Statistics.TotalLateFees = borrowingStats.TotalLateFees;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل الإحصائيات - Error loading statistics");
                Statistics = new SystemStatistics(); // إحصائيات فارغة في حالة الخطأ
            }
        }
    }

    /// <summary>
    /// إحصائيات النظام
    /// System statistics
    /// </summary>
    public class SystemStatistics
    {
        // إحصائيات المستخدمين - User statistics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }

        // إحصائيات الكتب - Book statistics
        public int TotalBooks { get; set; }
        public int AvailableBooks { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }

        // إحصائيات الاستعارات - Borrowing statistics
        public int ActiveBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
        public int TotalBorrowings { get; set; }
        public int ReturnedBorrowings { get; set; }
        public decimal TotalLateFees { get; set; }

        // خصائص محسوبة - Computed properties
        public int InactiveUsers => TotalUsers - ActiveUsers;
        public int RegularUsers => TotalUsers - AdminUsers;
        public int UnavailableBooks => TotalBooks - AvailableBooks;
        public int BorrowedCopies => TotalCopies - AvailableCopies;
        public double BorrowingRate => TotalBooks > 0 ? (double)ActiveBorrowings / TotalBooks * 100 : 0;
        public double OverdueRate => ActiveBorrowings > 0 ? (double)OverdueBorrowings / ActiveBorrowings * 100 : 0;
    }
}
