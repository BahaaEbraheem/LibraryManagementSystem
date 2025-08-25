using System.ComponentModel;

namespace LibraryManagementSystem.DAL.Models.Enums
{
    /// <summary>
    /// حالة توفر الكتاب
    /// Book availability status
    /// </summary>
    public enum BookAvailabilityStatus
    {
        /// <summary>
        /// متوفر - Available
        /// </summary>
        [Description("متوفر - Available")]
        Available = 1,

        /// <summary>
        /// مُعار بالكامل - Fully Borrowed
        /// </summary>
        [Description("مُعار بالكامل - Fully Borrowed")]
        FullyBorrowed = 2,

        /// <summary>
        /// غير متوفر - Not Available
        /// </summary>
        [Description("غير متوفر - Not Available")]
        NotAvailable = 3,

        /// <summary>
        /// متوفر جزئياً - Partially Available
        /// </summary>
        [Description("متوفر جزئياً - Partially Available")]
        PartiallyAvailable = 4,

        /// <summary>
        /// خارج الخدمة - Out of Service
        /// </summary>
        [Description("خارج الخدمة - Out of Service")]
        OutOfService = 5
    }

    /// <summary>
    /// امتدادات لـ BookAvailabilityStatus
    /// Extensions for BookAvailabilityStatus
    /// </summary>
    public static class BookAvailabilityStatusExtensions
    {
        /// <summary>
        /// الحصول على الوصف
        /// Get description
        /// </summary>
        public static string GetDescription(this BookAvailabilityStatus status)
        {
            var field = status.GetType().GetField(status.ToString());
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return attribute?.Description ?? status.ToString();
        }

        /// <summary>
        /// الحصول على CSS Class للحالة
        /// Get CSS class for status
        /// </summary>
        public static string GetCssClass(this BookAvailabilityStatus status)
        {
            return status switch
            {
                BookAvailabilityStatus.Available => "badge bg-success",
                BookAvailabilityStatus.PartiallyAvailable => "badge bg-warning",
                BookAvailabilityStatus.FullyBorrowed => "badge bg-danger",
                BookAvailabilityStatus.NotAvailable => "badge bg-secondary",
                BookAvailabilityStatus.OutOfService => "badge bg-dark",
                _ => "badge bg-secondary"
            };
        }

        /// <summary>
        /// الحصول على أيقونة للحالة
        /// Get icon for status
        /// </summary>
        public static string GetIcon(this BookAvailabilityStatus status)
        {
            return status switch
            {
                BookAvailabilityStatus.Available => "fas fa-check-circle",
                BookAvailabilityStatus.PartiallyAvailable => "fas fa-exclamation-circle",
                BookAvailabilityStatus.FullyBorrowed => "fas fa-times-circle",
                BookAvailabilityStatus.NotAvailable => "fas fa-ban",
                BookAvailabilityStatus.OutOfService => "fas fa-tools",
                _ => "fas fa-question-circle"
            };
        }

        /// <summary>
        /// تحديد حالة التوفر بناءً على عدد النسخ
        /// Determine availability status based on copy counts
        /// </summary>
        public static BookAvailabilityStatus DetermineStatus(int totalCopies, int availableCopies)
        {
            if (totalCopies <= 0)
                return BookAvailabilityStatus.NotAvailable;

            if (availableCopies <= 0)
                return BookAvailabilityStatus.FullyBorrowed;

            if (availableCopies == totalCopies)
                return BookAvailabilityStatus.Available;

            return BookAvailabilityStatus.PartiallyAvailable;
        }
    }
}
