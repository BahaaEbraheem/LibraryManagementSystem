using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.DAL.Repositories
{


    /// <summary>
    /// الكتاب الأكثر استعارة
    /// Most borrowed book
    /// </summary>
    public class MostBorrowedBook
    {
        /// <summary>
        /// معرف الكتاب
        /// Book ID
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// عنوان الكتاب
        /// Book title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// مؤلف الكتاب
        /// Book author
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// الرقم المعياري للكتاب
        /// Book ISBN
        /// </summary>
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// عدد مرات الاستعارة
        /// Number of times borrowed
        /// </summary>
        public int BorrowCount { get; set; }

        /// <summary>
        /// عدد الاستعارات النشطة حالياً
        /// Current active borrowings
        /// </summary>
        public int CurrentActiveBorrowings { get; set; }
    }
}
