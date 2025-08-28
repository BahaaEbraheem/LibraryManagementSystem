using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.BLL.Validation
{    /// <summary>
     /// نتيجة التحقق من صحة قواعد الأعمال
     /// Business rule validation result
     /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// هل النتيجة صحيحة
        /// Is the result valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// رسائل الخطأ
        /// Error messages
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new();

        /// <summary>
        /// رسائل التحذير
        /// Warning messages
        /// </summary>
        public List<string> WarningMessages { get; set; } = new();

        /// <summary>
        /// كود الخطأ
        /// Error code
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// بيانات إضافية
        /// Additional data
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new();

        /// <summary>
        /// إنشاء نتيجة صحيحة
        /// Create valid result
        /// </summary>
        public static ValidationResult Success() => new() { IsValid = true };

        /// <summary>
        /// إنشاء نتيجة خاطئة
        /// Create invalid result
        /// </summary>
        public static ValidationResult Failure(string errorMessage, string? errorCode = null)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessages = new List<string> { errorMessage },
                ErrorCode = errorCode
            };
        }

        /// <summary>
        /// إنشاء نتيجة خاطئة مع عدة رسائل
        /// Create invalid result with multiple messages
        /// </summary>
        public static ValidationResult Failure(IEnumerable<string> errorMessages, string? errorCode = null)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessages = errorMessages.ToList(),
                ErrorCode = errorCode
            };
        }

        /// <summary>
        /// إضافة رسالة خطأ
        /// Add error message
        /// </summary>
        public ValidationResult AddError(string message)
        {
            ErrorMessages.Add(message);
            IsValid = false;
            return this;
        }

        /// <summary>
        /// إضافة رسالة تحذير
        /// Add warning message
        /// </summary>
        public ValidationResult AddWarning(string message)
        {
            WarningMessages.Add(message);
            return this;
        }

        /// <summary>
        /// إضافة بيانات إضافية
        /// Add additional data
        /// </summary>
        public ValidationResult AddData(string key, object value)
        {
            AdditionalData[key] = value;
            return this;
        }
    }
}
