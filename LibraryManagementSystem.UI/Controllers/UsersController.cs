using LibraryManagementSystem.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.UI.Controllers
{
    /// <summary>
    /// تحكم في عمليات المستخدمين
    /// Users operations controller
    /// </summary>
    [Route("Users")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        /// <summary>
        /// منشئ التحكم
        /// Controller constructor
        /// </summary>
        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// حذف مستخدم
        /// Delete user
        /// </summary>
        [HttpPost("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // التحقق من أن المستخدم مدير
                // Check if user is admin
                if (HttpContext.Session.GetString("UserRole") != "Administrator")
                {
                    return Json(new { success = false, message = "غير مصرح لك بهذا الإجراء - Not authorized for this action" });
                }

                if (id <= 0)
                {
                    return Json(new { success = false, message = "معرف المستخدم غير صحيح - Invalid user ID" });
                }

                // التحقق من عدم حذف المستخدم الحالي
                // Check not deleting current user
                var currentUserIdString = HttpContext.Session.GetString("UserId");
                if (int.TryParse(currentUserIdString, out int currentUserId) && currentUserId == id)
                {
                    return Json(new { success = false, message = "لا يمكنك حذف حسابك الخاص - You cannot delete your own account" });
                }

                _logger.LogDebug("حذف المستخدم {UserId} - Deleting user", id);

                var result = await _userService.DeleteUserAsync(id);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم حذف المستخدم {UserId} بنجاح - Successfully deleted user", id);
                    return Json(new { success = true, message = "تم حذف المستخدم بنجاح - User deleted successfully" });
                }
                else
                {
                    _logger.LogWarning("فشل في حذف المستخدم {UserId} - Failed to delete user", id);
                    return Json(new { success = false, message = result.ErrorMessage ?? "فشل في حذف المستخدم - Failed to delete user" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المستخدم {UserId} - Error deleting user", id);
                return Json(new { success = false, message = "حدث خطأ أثناء حذف المستخدم - An error occurred while deleting the user" });
            }
        }
    }
}
