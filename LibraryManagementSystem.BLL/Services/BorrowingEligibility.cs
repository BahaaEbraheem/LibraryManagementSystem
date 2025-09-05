using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// أهلية الاستعارة
    /// Borrowing eligibility
    /// </summary>
    public class BorrowingEligibility
    {
        /// <summary>
        /// هل يمكن الاستعارة
        /// Can borrow
        /// </summary>
        public bool CanBorrow { get; set; }

        /// <summary>
        /// سبب عدم الإمكانية
        /// Reason for inability
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// عدد الكتب المستعارة حالياً
        /// Current borrowed books count
        /// </summary>
        public int CurrentBorrowedBooks { get; set; }

        /// <summary>
        /// الحد الأقصى للاستعارة
        /// Maximum borrowing limit
        /// </summary>
        public int MaxBorrowingLimit { get; set; }

        /// <summary>
        /// عدد الكتب المتأخرة
        /// Overdue books count
        /// </summary>
        public int OverdueBooks { get; set; }

        /// <summary>
        /// هل الكتاب متاح
        /// Is book available
        /// </summary>
        public bool IsBookAvailable { get; set; }

        /// <summary>
        /// هل المستخدم نشط
        /// Is user active
        /// </summary>
        public bool IsUserActive { get; set; }

        /// <summary>
        /// إنشاء نتيجة إيجابية
        /// Create positive result
        /// </summary>
        public static BorrowingEligibility Eligible()
        {
            return new BorrowingEligibility
            {
                CanBorrow = true
            };
        }

        /// <summary>
        /// إنشاء نتيجة سلبية
        /// Create negative result
        /// </summary>
        public static BorrowingEligibility NotEligible(string reason)
        {
            return new BorrowingEligibility
            {
                CanBorrow = false,
                Reason = reason
            };
        }
    }
}
