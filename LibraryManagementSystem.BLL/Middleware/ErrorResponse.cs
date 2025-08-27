using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.BLL.Middleware
{
    /// <summary>
    /// نموذج استجابة الخطأ
    /// Error response model
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// معرف الخطأ الفريد
        /// Unique error identifier
        /// </summary>
        public string ErrorId { get; set; } = string.Empty;

        /// <summary>
        /// عنوان الخطأ
        /// Error title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// رسالة الخطأ
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// تفاصيل الخطأ (للتطوير فقط)
        /// Error details (development only)
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// وقت حدوث الخطأ
        /// Error timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
