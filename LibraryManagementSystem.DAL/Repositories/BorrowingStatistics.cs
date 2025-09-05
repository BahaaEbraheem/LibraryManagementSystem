using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.DAL.Repositories
{
    /// <summary>
    /// إحصائيات الاستعارات
    /// Borrowing statistics
    /// </summary>
    public class BorrowingStatistics
    {
        /// <summary>
        /// إجمالي عدد الاستعارات
        /// Total number of borrowings
        /// </summary>
        public int TotalBorrowings { get; set; }

        /// <summary>
        /// عدد الاستعارات النشطة
        /// Number of active borrowings
        /// </summary>
        public int ActiveBorrowings { get; set; }

        /// <summary>
        /// عدد الاستعارات المتأخرة
        /// Number of overdue borrowings
        /// </summary>
        public int OverdueBorrowings { get; set; }

        /// <summary>
        /// عدد الاستعارات المرجعة
        /// Number of returned borrowings
        /// </summary>
        public int ReturnedBorrowings { get; set; }

        /// <summary>
        /// إجمالي الرسوم المتأخرة
        /// Total late fees
        /// </summary>
        public decimal TotalLateFees { get; set; }

        /// <summary>
        /// متوسط فترة الاستعارة بالأيام
        /// Average borrowing period in days
        /// </summary>
        public double AverageBorrowingPeriod { get; set; }

        /// <summary>
        /// عدد الاستعارات هذا الشهر
        /// Number of borrowings this month
        /// </summary>
        public int BorrowingsThisMonth { get; set; }

        /// <summary>
        /// عدد الإرجاعات هذا الشهر
        /// Number of returns this month
        /// </summary>
        public int ReturnsThisMonth { get; set; }
    }
}
