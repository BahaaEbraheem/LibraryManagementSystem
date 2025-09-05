using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.DAL.Repositories
{


    /// <summary>
    /// المستخدم الأكثر نشاطاً في الاستعارة
    /// Most active borrowing user
    /// </summary>
    public class MostActiveUser
    {
        /// <summary>
        /// معرف المستخدم
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// الاسم الكامل
        /// Full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// البريد الإلكتروني
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// إجمالي عدد الاستعارات
        /// Total borrowings count
        /// </summary>
        public int TotalBorrowings { get; set; }

        /// <summary>
        /// عدد الاستعارات النشطة حالياً
        /// Current active borrowings
        /// </summary>
        public int CurrentActiveBorrowings { get; set; }

        /// <summary>
        /// عدد الكتب المتأخرة
        /// Overdue books count
        /// </summary>
        public int OverdueBooks { get; set; }
    }
}
