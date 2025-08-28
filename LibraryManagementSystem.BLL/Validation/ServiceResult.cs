using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.BLL.Validation
{
    /// <summary>
    /// نتيجة العملية مع البيانات
    /// Service operation result with data
    /// </summary>
    public class ServiceResult<T>
    {
        /// <summary>
        /// هل العملية نجحت
        /// Whether the operation succeeded
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// البيانات المرجعة
        /// Returned data
        /// </summary>
        public T? Data { get; set; }
        /// <summary>
        /// كود الخطأ لقواعد الأعمال أو أي خطأ محدد
        /// </summary>
        public string? ErrorCode { get; set; }
        /// <summary>
        /// رسالة الخطأ
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// رسائل التحقق من الصحة
        /// Validation messages
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new();

        /// <summary>
        /// إنشاء نتيجة ناجحة
        /// Create successful result
        /// </summary>
        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        /// <summary>
        /// إنشاء نتيجة فاشلة
        /// Create failed result
        /// </summary>
        public static ServiceResult<T> Failure(string errorMessage, string? errorCode = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }
        /// <summary>
        /// إنشاء نتيجة فاشلة مع أخطاء التحقق
        /// Create failed result with validation errors
        /// </summary>
        public static ServiceResult<T> ValidationFailure(List<string> validationErrors)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                ValidationErrors = validationErrors
            };
        }
    }
}
