using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace LibraryManagementSystem.BLL.Middleware
{
    /// <summary>
    /// وسطاء معالجة الأخطاء العامة
    /// Global exception handling middleware
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// منشئ الوسطاء
        /// Middleware constructor
        /// </summary>
        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// معالجة الطلب
        /// Process request
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ غير متوقع في الطلب {RequestPath} - Unhandled exception in request",
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// معالجة الاستثناء
        /// Handle exception
        /// </summary>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse();

            switch (exception)
            {
                case SqlException sqlEx:
                    // أخطاء قاعدة البيانات
                    // Database errors
                    errorResponse = HandleDatabaseException(sqlEx);
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;

                case TimeoutException timeoutEx:
                    // أخطاء انتهاء المهلة الزمنية
                    // Timeout errors
                    errorResponse = new ErrorResponse
                    {
                        Title = "انتهت المهلة الزمنية - Request Timeout",
                        Message = "انتهت المهلة الزمنية للطلب. يرجى المحاولة مرة أخرى - Request timeout. Please try again",
                    };
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    // أخطاء عدم التفويض
                    // Unauthorized access errors
                    errorResponse = new ErrorResponse
                    {
                        Title = "غير مصرح - Unauthorized",
                        Message = "ليس لديك صلاحية للوصول لهذا المورد - You don't have permission to access this resource",
                    };
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case ArgumentException argEx:
                    // أخطاء المعاملات
                    // Argument errors
                    errorResponse = new ErrorResponse
                    {
                        Title = "معامل غير صحيح - Invalid Argument",
                        Message = "تم تمرير معامل غير صحيح - Invalid argument provided",
                    };
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case InvalidOperationException invalidOpEx:
                    // أخطاء العمليات غير الصحيحة
                    // Invalid operation errors
                    errorResponse = new ErrorResponse
                    {
                        Title = "عملية غير صحيحة - Invalid Operation",
                        Message = "لا يمكن تنفيذ هذه العملية في الوقت الحالي - Cannot perform this operation at this time",
                    };
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case FileNotFoundException fileNotFoundEx:
                    // أخطاء الملفات غير الموجودة
                    // File not found errors
                    errorResponse = new ErrorResponse
                    {
                        Title = "ملف غير موجود - File Not Found",
                        Message = "الملف المطلوب غير موجود - The requested file was not found",
                    };
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                default:
                    // أخطاء عامة
                    // General errors
                    errorResponse = new ErrorResponse
                    {
                        Title = "خطأ في الخادم - Server Error",
                        Message = "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى أو الاتصال بالدعم الفني - An unexpected error occurred. Please try again or contact support",
                    };
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            // إضافة معرف فريد للخطأ للتتبع
            // Add unique error ID for tracking
            errorResponse.ErrorId = Guid.NewGuid().ToString();
            errorResponse.Timestamp = DateTime.UtcNow;

            _logger.LogError("معرف الخطأ: {ErrorId} - نوع الاستثناء- الرسالة: - Error ID: - Exception Type: {ExceptionType} - Message: {Message}",
                errorResponse.ErrorId, exception.GetType().Name, exception.Message);

            // التحقق من نوع الطلب
            // Check request type
            if (IsAjaxRequest(context) || IsApiRequest(context))
            {
                // إرجاع JSON للطلبات AJAX/API
                // Return JSON for AJAX/API requests
                var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await response.WriteAsync(jsonResponse);
            }
            else
            {
                // إعادة توجيه لصفحة الخطأ للطلبات العادية
                // Redirect to error page for regular requests
                context.Items["ErrorResponse"] = errorResponse;
                response.Redirect($"/Error?errorId={errorResponse.ErrorId}");
            }
        }

        /// <summary>
        /// معالجة أخطاء قاعدة البيانات
        /// Handle database exceptions
        /// </summary>
        private ErrorResponse HandleDatabaseException(SqlException sqlException)
        {
            var errorResponse = new ErrorResponse
            {
                Title = "خطأ في قاعدة البيانات - Database Error"
            };

            switch (sqlException.Number)
            {
                case 2: // Timeout
                case -2:
                    errorResponse.Message = "انتهت المهلة الزمنية للاتصال بقاعدة البيانات. يرجى المحاولة مرة أخرى - Database connection timeout. Please try again";
                    break;
                case 18456: // Login failed
                    errorResponse.Message = "فشل في الاتصال بقاعدة البيانات - Failed to connect to database";
                    break;
                case 547: // Foreign key constraint
                    errorResponse.Message = "لا يمكن تنفيذ هذه العملية بسبب قيود البيانات - Cannot perform this operation due to data constraints";
                    break;
                case 2601: // Duplicate key
                case 2627:
                    errorResponse.Message = "البيانات موجودة مسبقاً - Data already exists";
                    break;
                case 8152: // String truncation
                    errorResponse.Message = "البيانات المدخلة طويلة جداً - Input data is too long";
                    break;
                default:
                    errorResponse.Message = "حدث خطأ في قاعدة البيانات. يرجى المحاولة مرة أخرى - A database error occurred. Please try again";
                    break;
            }


            return errorResponse;
        }

        /// <summary>
        /// التحقق من كون الطلب AJAX
        /// Check if request is AJAX
        /// </summary>
        private static bool IsAjaxRequest(HttpContext context)
        {
            return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        /// <summary>
        /// التحقق من كون الطلب API
        /// Check if request is API
        /// </summary>
        private static bool IsApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/api") ||
                   context.Request.Headers["Accept"].ToString().Contains("application/json");
        }
    }


}
