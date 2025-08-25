using Microsoft.AspNetCore.Mvc.RazorPages;
using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models;

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

        public IEnumerable<User> Users { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogDebug("تحميل صفحة المستخدمين - Loading users page");
                Users = await _userService.GetAllUsersAsync();
                _logger.LogDebug("تم تحميل {Count} مستخدم - Loaded users", Users.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل المستخدمين - Error loading users");
                Users = new List<User>();
            }
        }
    }
}
