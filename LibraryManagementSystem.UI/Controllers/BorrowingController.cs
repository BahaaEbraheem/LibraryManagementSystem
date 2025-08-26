using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.UI.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.UI.Controllers
{
    /// <summary>
    /// تحكم في عمليات الاستعارة
    /// Borrowing operations controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowingController : ControllerBase
    {
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<BorrowingController> _logger;

        public BorrowingController(IBorrowingService borrowingService, ILogger<BorrowingController> logger)
        {
            _borrowingService = borrowingService ?? throw new ArgumentNullException(nameof(borrowingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// استعارة كتاب
        /// Borrow a book
        /// </summary>
        [HttpPost("borrow")]
        [JwtAuthenticatedOnly]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowBookRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "طلب غير صحيح - Invalid request" });
                }

                if (request.BookId <= 0)
                {
                    return BadRequest(new { success = false, message = "معرف الكتاب غير صحيح - Invalid book ID" });
                }

                if (request.UserId <= 0)
                {
                    return BadRequest(new { success = false, message = "معرف المستخدم غير صحيح - Invalid user ID" });
                }

                _logger.LogInformation("محاولة استعارة الكتاب {BookId} للمستخدم {UserId} - Attempting to borrow book for user",
                    request.BookId, request.UserId);

                // التحقق من أهلية الاستعارة أولاً
                // Check borrowing eligibility first
                var eligibilityResult = await _borrowingService.CheckBorrowingEligibilityAsync(request.UserId, request.BookId);

                if (!eligibilityResult.IsSuccess)
                {
                    _logger.LogWarning("فشل في التحقق من أهلية الاستعارة: {Error} - Failed to check borrowing eligibility",
                        eligibilityResult.ErrorMessage);
                    return BadRequest(new { success = false, message = eligibilityResult.ErrorMessage });
                }

                if (!eligibilityResult.Data!.CanBorrow)
                {
                    _logger.LogWarning("المستخدم غير مؤهل للاستعارة: {Reason} - User not eligible for borrowing",
                        eligibilityResult.Data.Reason);
                    return BadRequest(new { success = false, message = eligibilityResult.Data.Reason });
                }

                // تنفيذ الاستعارة
                // Execute borrowing
                var borrowResult = await _borrowingService.BorrowBookAsync(request.UserId, request.BookId, request.BorrowingDays);

                if (borrowResult.IsSuccess)
                {
                    _logger.LogInformation("تم استعارة الكتاب بنجاح - Book borrowed successfully. BorrowingId: {BorrowingId}",
                        borrowResult.Data);

                    return Ok(new
                    {
                        success = true,
                        message = "تم استعارة الكتاب بنجاح! - Book borrowed successfully!",
                        borrowingId = borrowResult.Data
                    });
                }
                else
                {
                    _logger.LogWarning("فشل في استعارة الكتاب: {Error} - Failed to borrow book",
                        borrowResult.ErrorMessage);
                    return BadRequest(new { success = false, message = borrowResult.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استعارة الكتاب - Error borrowing book");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم - Server error occurred" });
            }
        }

        /// <summary>
        /// إرجاع كتاب
        /// Return a book
        /// </summary>
        [HttpPost("return")]
        [JwtAuthenticatedOnly]
        public async Task<IActionResult> ReturnBook([FromBody] ReturnBookRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "طلب غير صحيح - Invalid request" });
                }

                if (request.BorrowingId <= 0)
                {
                    return BadRequest(new { success = false, message = "معرف الاستعارة غير صحيح - Invalid borrowing ID" });
                }

                _logger.LogInformation("محاولة إرجاع الكتاب للاستعارة {BorrowingId} - Attempting to return book for borrowing",
                    request.BorrowingId);

                var result = await _borrowingService.ReturnBookAsync(request.BorrowingId, request.Notes);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("تم إرجاع الكتاب بنجاح للاستعارة {BorrowingId} - Successfully returned book for borrowing",
                        request.BorrowingId);

                    return Ok(new
                    {
                        success = true,
                        message = "تم إرجاع الكتاب بنجاح! - Book returned successfully!"
                    });
                }
                else
                {
                    _logger.LogWarning("فشل في إرجاع الكتاب: {Error} - Failed to return book",
                        result.ErrorMessage);
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إرجاع الكتاب - Error returning book");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم - Server error occurred" });
            }
        }

        /// <summary>
        /// التحقق من إمكانية استعارة كتاب
        /// Check if a book can be borrowed
        /// </summary>
        [HttpGet("check-eligibility")]
        public async Task<IActionResult> CheckBorrowingEligibility(int userId, int bookId)
        {
            try
            {
                if (userId <= 0 || bookId <= 0)
                {
                    return BadRequest(new { success = false, message = "معرفات غير صحيحة - Invalid IDs" });
                }

                var result = await _borrowingService.CheckBorrowingEligibilityAsync(userId, bookId);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        success = true,
                        canBorrow = result.Data!.CanBorrow,
                        reason = result.Data.Reason,
                        currentBorrowedBooks = result.Data.CurrentBorrowedBooks,
                        maxBooksAllowed = result.Data.MaxBorrowingLimit,
                        overdueBooks = result.Data.OverdueBooks
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من أهلية الاستعارة - Error checking borrowing eligibility");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم - Server error occurred" });
            }
        }
    }

    /// <summary>
    /// طلب استعارة كتاب
    /// Borrow book request
    /// </summary>
    public class BorrowBookRequest
    {
        /// <summary>معرف المستخدم - User ID</summary>
        public int UserId { get; set; }

        /// <summary>معرف الكتاب - Book ID</summary>
        public int BookId { get; set; }

        /// <summary>عدد أيام الاستعارة - Borrowing days</summary>
        public int BorrowingDays { get; set; } = 14;
    }

    /// <summary>
    /// طلب إرجاع كتاب
    /// Return book request
    /// </summary>
    public class ReturnBookRequest
    {
        /// <summary>معرف الاستعارة - Borrowing ID</summary>
        public int BorrowingId { get; set; }

        /// <summary>ملاحظات - Notes</summary>
        public string? Notes { get; set; }
    }
}
