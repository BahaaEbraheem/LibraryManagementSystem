using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages.Users
{
    /// <summary>
    /// نموذج صفحة تفاصيل المستخدم
    /// User details page model
    /// </summary>
    public class DetailsModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<DetailsModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public DetailsModel(IUserService userService, IBorrowingService borrowingService, ILogger<DetailsModel> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _borrowingService = borrowingService ?? throw new ArgumentNullException(nameof(borrowingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// بيانات المستخدم
        /// User data
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// استعارات المستخدم النشطة
        /// User's active borrowings
        /// </summary>
        public IEnumerable<Borrowing>? ActiveBorrowings { get; set; }

        /// <summary>
        /// إجمالي استعارات المستخدم
        /// Total user borrowings
        /// </summary>
        public int TotalBorrowings { get; set; }

        /// <summary>
        /// عدد الكتب المتأخرة
        /// Number of overdue books
        /// </summary>
        public int OverdueBooks { get; set; }

        /// <summary>
        /// إجمالي الرسوم المتأخرة
        /// Total late fees
        /// </summary>
        public decimal TotalLateFees { get; set; }

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
        /// معالج طلب GET - تحميل تفاصيل المستخدم
        /// GET request handler - load user details
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // التحقق من أن المستخدم مدير
                // Check if user is admin
                if (HttpContext.Session.GetString("UserRole") != "Administrator")
                {
                    return RedirectToPage("/Auth/AccessDenied");
                }

                if (id <= 0)
                {
                    ErrorMessage = "معرف المستخدم غير صحيح - Invalid user ID";
                    return Page();
                }

                _logger.LogDebug("تحميل تفاصيل المستخدم {UserId} - Loading user details", id);

                // الحصول على بيانات المستخدم
                // Get user data
                var userResult = await _userService.GetUserByIdAsync(id);
                if (userResult.IsSuccess && userResult.Data != null)
                {
                    User = userResult.Data;
                }
                else
                {
                    ErrorMessage = userResult.ErrorMessage ?? "لم يتم العثور على المستخدم - User not found";
                    return Page();
                }

                // الحصول على استعارات المستخدم النشطة
                // Get user's active borrowings
                var activeBorrowingsResult = await _borrowingService.GetUserActiveBorrowingsAsync(id);
                if (activeBorrowingsResult.IsSuccess && activeBorrowingsResult.Data != null)
                {
                    ActiveBorrowings = activeBorrowingsResult.Data;
                    OverdueBooks = ActiveBorrowings.Count(b => b.IsOverdue);
                    TotalLateFees = ActiveBorrowings.Sum(b => b.LateFee);
                }

                // الحصول على إجمالي استعارات المستخدم
                // Get total user borrowings
                var allBorrowingsResult = await _borrowingService.GetUserBorrowingsAsync(id);
                if (allBorrowingsResult.IsSuccess && allBorrowingsResult.Data != null)
                {
                    TotalBorrowings = allBorrowingsResult.Data.Count();
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

                _logger.LogInformation("تم تحميل تفاصيل المستخدم {UserId} بنجاح - Successfully loaded user details", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل تفاصيل المستخدم {UserId} - Error loading user details", id);
                ErrorMessage = "حدث خطأ أثناء تحميل تفاصيل المستخدم - An error occurred while loading user details";
                return Page();
            }
        }
    }
}
