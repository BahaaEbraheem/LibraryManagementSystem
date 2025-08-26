using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LibraryManagementSystem.UI.Middleware;
using System.Diagnostics;

namespace LibraryManagementSystem.UI.Pages
{
    /// <summary>
    /// نموذج صفحة الخطأ
    /// Error page model
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        /// <summary>
        /// منشئ نموذج الصفحة
        /// Page model constructor
        /// </summary>
        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// معرف الطلب
        /// Request ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// عرض معرف الطلب
        /// Show request ID
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// استجابة الخطأ
        /// Error response
        /// </summary>
        public ErrorResponse? ErrorResponse { get; set; }

        /// <summary>
        /// معرف الخطأ
        /// Error ID
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? ErrorId { get; set; }

        /// <summary>
        /// رمز حالة HTTP
        /// HTTP status code
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int? StatusCode { get; set; }

        /// <summary>
        /// معالج طلب GET
        /// GET request handler
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // محاولة الحصول على تفاصيل الخطأ من HttpContext
            // Try to get error details from HttpContext
            if (HttpContext.Items.TryGetValue("ErrorResponse", out var errorResponseObj) && 
                errorResponseObj is ErrorResponse errorResponse)
            {
                ErrorResponse = errorResponse;
            }
            else if (!string.IsNullOrEmpty(ErrorId))
            {
                // إنشاء استجابة خطأ افتراضية بناءً على معرف الخطأ
                // Create default error response based on error ID
                ErrorResponse = new ErrorResponse
                {
                    ErrorId = ErrorId,
                    Title = GetErrorTitle(),
                    Message = GetErrorMessage(),
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                // خطأ عام
                // General error
                ErrorResponse = new ErrorResponse
                {
                    ErrorId = Guid.NewGuid().ToString(),
                    Title = "خطأ غير متوقع - Unexpected Error",
                    Message = "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى - An unexpected error occurred. Please try again",
                    Timestamp = DateTime.UtcNow
                };
            }

            _logger.LogInformation("عرض صفحة الخطأ - معرف الخطأ: {ErrorId} - رمز الحالة: {StatusCode} - Displaying error page - Error ID: {ErrorId} - Status Code: {StatusCode}",
                ErrorResponse.ErrorId, StatusCode);
        }

        /// <summary>
        /// الحصول على عنوان الخطأ بناءً على رمز الحالة
        /// Get error title based on status code
        /// </summary>
        private string GetErrorTitle()
        {
            return StatusCode switch
            {
                400 => "طلب غير صحيح - Bad Request",
                401 => "غير مصرح - Unauthorized",
                403 => "ممنوع - Forbidden",
                404 => "الصفحة غير موجودة - Page Not Found",
                408 => "انتهت المهلة الزمنية - Request Timeout",
                500 => "خطأ في الخادم - Server Error",
                502 => "بوابة سيئة - Bad Gateway",
                503 => "الخدمة غير متاحة - Service Unavailable",
                504 => "انتهت مهلة البوابة - Gateway Timeout",
                _ => "خطأ غير متوقع - Unexpected Error"
            };
        }

        /// <summary>
        /// الحصول على رسالة الخطأ بناءً على رمز الحالة
        /// Get error message based on status code
        /// </summary>
        private string GetErrorMessage()
        {
            return StatusCode switch
            {
                400 => "الطلب المرسل غير صحيح. يرجى التحقق من البيانات المدخلة - The request is invalid. Please check your input",
                401 => "يجب تسجيل الدخول للوصول لهذه الصفحة - You must log in to access this page",
                403 => "ليس لديك صلاحية للوصول لهذه الصفحة - You don't have permission to access this page",
                404 => "الصفحة التي تبحث عنها غير موجودة - The page you're looking for doesn't exist",
                408 => "انتهت المهلة الزمنية للطلب. يرجى المحاولة مرة أخرى - Request timeout. Please try again",
                500 => "حدث خطأ في الخادم. يرجى المحاولة مرة أخرى لاحقاً - A server error occurred. Please try again later",
                502 => "خطأ في البوابة. يرجى المحاولة مرة أخرى - Gateway error. Please try again",
                503 => "الخدمة غير متاحة حالياً. يرجى المحاولة مرة أخرى لاحقاً - Service is currently unavailable. Please try again later",
                504 => "انتهت مهلة البوابة. يرجى المحاولة مرة أخرى - Gateway timeout. Please try again",
                _ => "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى أو الاتصال بالدعم الفني - An unexpected error occurred. Please try again or contact support"
            };
        }
    }
}
