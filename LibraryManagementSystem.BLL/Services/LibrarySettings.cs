using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// إعدادات المكتبة
    /// Library settings
    /// </summary>
    public class LibrarySettings
    {
        /// <summary>
        /// الحد الأقصى لعدد الكتب المستعارة لكل مستخدم
        /// Maximum books per user
        /// </summary>
        public int MaxBooksPerUser { get; set; } = 5;

        /// <summary>
        /// عدد أيام الاستعارة الافتراضي
        /// Default borrowing days
        /// </summary>
        public int DefaultBorrowingDays { get; set; } = 14;

        /// <summary>
        /// رسوم التأخير لكل يوم
        /// Late fee per day
        /// </summary>
        public decimal LateFeePerDay { get; set; } = 1.00m;

        /// <summary>
        /// عدد أيام السماح قبل فرض الرسوم
        /// Grace days before charging fees
        /// </summary>
        public int GraceDays { get; set; } = 0;

        /// <summary>
        /// الحد الأقصى لعدد مرات التجديد
        /// Maximum renewal count
        /// </summary>
        public int MaxRenewalCount { get; set; } = 2;

        /// <summary>
        /// عدد أيام التجديد
        /// Renewal days
        /// </summary>
        public int RenewalDays { get; set; } = 14;
    }
}
