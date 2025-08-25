using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagementSystem.UI.Pages.Borrowings
{
    /// <summary>
    /// نموذج صفحة إدارة الاستعارات
    /// Borrowings management page model
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<IndexModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public IndexModel(IBorrowingService borrowingService, ILogger<IndexModel> logger)
        {
            _borrowingService = borrowingService ?? throw new ArgumentNullException(nameof(borrowingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// قائمة الاستعارات
        /// Borrowings list
        /// </summary>
        public IEnumerable<Borrowing>? Borrowings { get; set; }

        /// <summary>
        /// إحصائيات الاستعارات
        /// Borrowing statistics
        /// </summary>
        public BorrowingStatistics? Statistics { get; set; }

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
        /// معرف المستخدم للفلترة
        /// User ID for filtering
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int? UserId { get; set; }

        /// <summary>
        /// حالة الاستعارة للفلترة
        /// Borrowing status for filtering
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        /// <summary>
        /// ترتيب النتائج
        /// Sort results by
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "BorrowDate";

        /// <summary>
        /// معالج طلب GET - تحميل الصفحة
        /// GET request handler - load page
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogDebug("تحميل صفحة إدارة الاستعارات - Loading borrowings management page");

                // الحصول على الإحصائيات
                // Get statistics
                var statisticsResult = await _borrowingService.GetBorrowingStatisticsAsync();
                if (statisticsResult.IsSuccess)
                {
                    Statistics = statisticsResult.Data;
                }

                // الحصول على الاستعارات حسب الفلاتر
                // Get borrowings based on filters
                await LoadBorrowingsAsync();

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
                _logger.LogError(ex, "خطأ في تحميل صفحة الاستعارات - Error loading borrowings page");
                ErrorMessage = "حدث خطأ أثناء تحميل الصفحة - An error occurred while loading the page";
                return Page();
            }
        }

        /// <summary>
        /// معالج طلب POST لإرجاع كتاب
        /// POST request handler for returning a book
        /// </summary>
        public async Task<IActionResult> OnPostReturnAsync(int borrowingId, string? notes = null)
        {
            try
            {
                if (borrowingId <= 0)
                {
                    return new JsonResult(new { success = false, message = "معرف الاستعارة غير صحيح - Invalid borrowing ID" });
                }

                _logger.LogDebug("محاولة إرجاع الكتاب للاستعارة {BorrowingId} - Attempting to return book for borrowing {BorrowingId}", borrowingId, borrowingId);

                var result = await _borrowingService.ReturnBookAsync(borrowingId, notes);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم إرجاع الكتاب بنجاح للاستعارة {BorrowingId} - Successfully returned book for borrowing {BorrowingId}", borrowingId, borrowingId);
                    return new JsonResult(new { success = true, message = "تم إرجاع الكتاب بنجاح - Book returned successfully" });
                }
                else
                {
                    _logger.LogWarning("فشل في إرجاع الكتاب للاستعارة {BorrowingId}: {Error} - Failed to return book for borrowing {BorrowingId}: {Error}",
                        borrowingId, result.ErrorMessage, borrowingId, result.ErrorMessage);
                    return new JsonResult(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرجاع الكتاب للاستعارة {BorrowingId} - Error returning book for borrowing {BorrowingId}", borrowingId, borrowingId);
                return new JsonResult(new { success = false, message = "حدث خطأ أثناء إرجاع الكتاب - An error occurred while returning the book" });
            }
        }

        /// <summary>
        /// معالج طلب POST لتمديد الاستعارة
        /// POST request handler for extending borrowing
        /// </summary>
        public async Task<IActionResult> OnPostExtendAsync(int borrowingId, int additionalDays = 14)
        {
            try
            {
                if (borrowingId <= 0)
                {
                    return new JsonResult(new { success = false, message = "معرف الاستعارة غير صحيح - Invalid borrowing ID" });
                }

                if (additionalDays <= 0 || additionalDays > 30)
                {
                    return new JsonResult(new { success = false, message = "عدد الأيام يجب أن يكون بين 1 و 30 - Days must be between 1 and 30" });
                }

                _logger.LogDebug("محاولة تمديد الاستعارة {BorrowingId} بـ {AdditionalDays} أيام - Attempting to extend borrowing {BorrowingId} by {AdditionalDays} days",
                    borrowingId, additionalDays, borrowingId, additionalDays);

                var result = await _borrowingService.ExtendBorrowingAsync(borrowingId, additionalDays);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم تمديد الاستعارة {BorrowingId} بنجاح - Successfully extended borrowing {BorrowingId}", borrowingId, borrowingId);
                    return new JsonResult(new { success = true, message = "تم تمديد الاستعارة بنجاح - Borrowing extended successfully" });
                }
                else
                {
                    _logger.LogWarning("فشل في تمديد الاستعارة {BorrowingId}: {Error} - Failed to extend borrowing {BorrowingId}: {Error}",
                        borrowingId, result.ErrorMessage, borrowingId, result.ErrorMessage);
                    return new JsonResult(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تمديد الاستعارة {BorrowingId} - Error extending borrowing {BorrowingId}", borrowingId, borrowingId);
                return new JsonResult(new { success = false, message = "حدث خطأ أثناء تمديد الاستعارة - An error occurred while extending the borrowing" });
            }
        }

        /// <summary>
        /// تحميل الاستعارات حسب الفلاتر
        /// Load borrowings based on filters
        /// </summary>
        private async Task LoadBorrowingsAsync()
        {
            try
            {
                ServiceResult<IEnumerable<Borrowing>> result;

                if (UserId.HasValue)
                {
                    // الحصول على استعارات مستخدم محدد
                    // Get borrowings for specific user
                    if (Status == "active")
                    {
                        result = await _borrowingService.GetUserActiveBorrowingsAsync(UserId.Value);
                    }
                    else
                    {
                        result = await _borrowingService.GetUserBorrowingsAsync(UserId.Value);
                    }
                }
                else
                {
                    // الحصول على جميع الاستعارات حسب الحالة
                    // Get all borrowings based on status
                    result = Status switch
                    {
                        "active" => await _borrowingService.GetActiveBorrowingsAsync(),
                        "overdue" => await _borrowingService.GetOverdueBorrowingsAsync(),
                        _ => await _borrowingService.GetActiveBorrowingsAsync() // افتراضي: الاستعارات النشطة
                    };
                }

                if (result.IsSuccess && result.Data != null)
                {
                    var borrowings = result.Data.ToList();

                    // فلترة الاستعارات المرجعة إذا كانت الحالة "returned"
                    // Filter returned borrowings if status is "returned"
                    if (Status == "returned")
                    {
                        // هذا يتطلب إضافة دالة للحصول على الاستعارات المرجعة في الخدمة
                        // This requires adding a function to get returned borrowings in the service
                        borrowings = borrowings.Where(b => b.IsReturned).ToList();
                    }

                    // ترتيب النتائج
                    // Sort results
                    borrowings = SortBy switch
                    {
                        "DueDate" => borrowings.OrderBy(b => b.DueDate).ToList(),
                        "UserName" => borrowings.OrderBy(b => $"{b.User?.FirstName} {b.User?.LastName}").ToList(),
                        "BookTitle" => borrowings.OrderBy(b => b.Book?.Title).ToList(),
                        _ => borrowings.OrderByDescending(b => b.BorrowDate).ToList() // افتراضي: تاريخ الاستعارة
                    };

                    Borrowings = borrowings;

                    _logger.LogDebug("تم تحميل {Count} استعارة - Loaded {Count} borrowings", borrowings.Count, borrowings.Count);
                }
                else
                {
                    Borrowings = new List<Borrowing>();
                    if (!result.IsSuccess)
                    {
                        ErrorMessage = result.ErrorMessage ?? "حدث خطأ أثناء تحميل الاستعارات";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل الاستعارات - Error loading borrowings");
                Borrowings = new List<Borrowing>();
                ErrorMessage = "حدث خطأ أثناء تحميل الاستعارات - An error occurred while loading borrowings";
            }
        }
    }
}
