using LibraryManagementSystem.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.UI.Controllers
{
    /// <summary>
    /// تحكم في عمليات الكتب
    /// Books operations controller
    /// </summary>
    [Route("Books")]
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        /// <summary>
        /// منشئ التحكم
        /// Controller constructor
        /// </summary>
        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// حذف كتاب
        /// Delete book
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
                    return Json(new { success = false, message = "معرف الكتاب غير صحيح - Invalid book ID" });
                }

                _logger.LogDebug("حذف الكتاب {BookId} - Deleting book", id);

                var result = await _bookService.DeleteBookAsync(id);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم حذف الكتاب {BookId} بنجاح - Successfully deleted book", id);
                    return Json(new { success = true, message = "تم حذف الكتاب بنجاح - Book deleted successfully" });
                }
                else
                {
                    _logger.LogWarning("فشل في حذف الكتاب {BookId}: {Error} - Failed to delete book", id, result.ErrorMessage);
                    return Json(new { success = false, message = result.ErrorMessage ?? "فشل في حذف الكتاب - Failed to delete book" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الكتاب {BookId} - Error deleting book", id);
                return Json(new { success = false, message = "حدث خطأ أثناء حذف الكتاب - An error occurred while deleting the book" });
            }
        }
    }
}
