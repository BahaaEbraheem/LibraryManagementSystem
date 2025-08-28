using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using LibraryManagementSystem.DAL.Models.DTOs;

namespace LibraryManagementSystem.UI.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IUserService userService, ILogger<IndexModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public PagedResult<User> Users { get; set; } = new PagedResult<User>
        {
            Items = new List<User>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        /// <summary>
        /// رقم الصفحة الحالية
        /// Current page number
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// حجم الصفحة
        /// Page size
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

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

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // التحقق من دور المستخدم
                // Check user role
                IsAdmin = HttpContext.Session.GetString("UserRole") == "Administrator";

                if (!IsAdmin)
                {
                    return RedirectToPage("/Account/AccessDenied");
                }

                // تعيين معايير الصفحات
                // Set pagination criteria
                PageNumber = Math.Max(1, pageNumber);
                PageSize = Math.Max(1, Math.Min(50, pageSize)); // الحد الأقصى 50 عنصر في الصفحة

                _logger.LogDebug("تحميل صفحة المستخدمين - Loading users page");

                // الحصول على جميع المستخدمين ثم تطبيق التقسيم
                // Get all users then apply pagination
                var allUsers = await _userService.GetAllUsersAsync();
                var usersList = allUsers.ToList();

                // تطبيق التقسيم
                // Apply pagination
                var totalCount = usersList.Count;
                var skip = (PageNumber - 1) * PageSize;
                var pagedUsers = usersList.Skip(skip).Take(PageSize).ToList();

                Users = new PagedResult<User>
                {
                    Items = pagedUsers,
                    TotalCount = totalCount,
                    PageNumber = PageNumber,
                    PageSize = PageSize
                };

                _logger.LogDebug("تم تحميل {Count} مستخدم من أصل {Total} - Loaded users out of total",
                    pagedUsers.Count, totalCount);

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
                _logger.LogError(ex, "خطأ في تحميل المستخدمين - Error loading users");
                Users = new PagedResult<User>
                {
                    Items = new List<User>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                };
                ErrorMessage = "حدث خطأ أثناء تحميل المستخدمين - An error occurred while loading users";
                return Page();
            }
        }

        // داخل IndexModel
        public async Task<IActionResult> OnPostDeleteAsync(int userId)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(userId);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "تم حذف المستخدم بنجاح!";
                    return RedirectToPage(); // إعادة تحميل الصفحة
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المستخدم {UserId}", userId);
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف المستخدم";
                return RedirectToPage();
            }
        }


    }
}
