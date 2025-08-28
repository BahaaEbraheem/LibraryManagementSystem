using LibraryManagementSystem.DAL.Models;

namespace LibraryManagementSystem.BLL.Validation
{
    /// <summary>
    /// واجهة مدقق قواعد الأعمال
    /// Business rule validator interface
    /// </summary>
    public interface IBusinessRuleValidator
    {
        /// <summary>
        /// التحقق من صحة استعارة كتاب
        /// Validate book borrowing
        /// </summary>
        /// <param name="userId">معرف المستخدم</param>
        /// <param name="bookId">معرف الكتاب</param>
        /// <returns>نتيجة التحقق</returns>
        Task<ValidationResult> ValidateBorrowingAsync(int userId, int bookId);

        /// <summary>
        /// التحقق من صحة إرجاع كتاب
        /// Validate book return
        /// </summary>
        /// <param name="borrowingId">معرف الاستعارة</param>
        /// <param name="userId">معرف المستخدم</param>
        /// <returns>نتيجة التحقق</returns>
        Task<ValidationResult> ValidateReturnAsync(int borrowingId, int userId);

        /// <summary>
        /// التحقق من صحة تجديد استعارة
        /// Validate borrowing renewal
        /// </summary>
        /// <param name="borrowingId">معرف الاستعارة</param>
        /// <param name="userId">معرف المستخدم</param>
        /// <returns>نتيجة التحقق</returns>
        Task<ValidationResult> ValidateRenewalAsync(int borrowingId, int userId);

        /// <summary>
        /// التحقق من صحة إضافة كتاب
        /// Validate book addition
        /// </summary>
        /// <param name="book">بيانات الكتاب</param>
        /// <returns>نتيجة التحقق</returns>
        Task<ValidationResult> ValidateBookAdditionAsync(Book book);

        /// <summary>
        /// التحقق من صحة تحديث كتاب
        /// Validate book update
        /// </summary>
        /// <param name="book">بيانات الكتاب</param>
        /// <returns>نتيجة التحقق</returns>
        Task<ValidationResult> ValidateBookUpdateAsync(Book book);

        /// <summary>
        /// التحقق من صحة حذف كتاب
        /// Validate book deletion
        /// </summary>
        /// <param name="bookId">معرف الكتاب</param>
        /// <returns>نتيجة التحقق</returns>
        Task<ValidationResult> ValidateBookDeletionAsync(int bookId);

        /// <summary>
        /// التحقق من صحة إضافة مستخدم
        /// Validate user addition
        /// </summary>
        /// <param name="user">بيانات المستخدم</param>
        /// <returns>نتيجة التحقق</returns>
        Task<ValidationResult> ValidateUserAdditionAsync(User user);

        /// <summary>
        /// التحقق من صحة تحديث مستخدم
        /// Validate user update
        /// </summary>
        /// <param name="user">بيانات المستخدم</param>
        /// <returns>نتيجة التحقق</returns>
        Task<ValidationResult> ValidateUserUpdateAsync(User user);

        /// <summary>
        /// التحقق من صحة حذف مستخدم
        /// Validate user deletion
        /// </summary>
        /// <param name="userId">معرف المستخدم</param>
        /// <returns>نتيجة التحقق</returns>
        Task<ValidationResult> ValidateUserDeletionAsync(int userId);
    }



    /// <summary>
    /// أنواع قواعد الأعمال
    /// Business rule types
    /// </summary>
    public enum BusinessRuleType
    {
        /// <summary>
        /// قاعدة الاستعارة
        /// Borrowing rule
        /// </summary>
        Borrowing,

        /// <summary>
        /// قاعدة الإرجاع
        /// Return rule
        /// </summary>
        Return,

        /// <summary>
        /// قاعدة التجديد
        /// Renewal rule
        /// </summary>
        Renewal,

        /// <summary>
        /// قاعدة الكتاب
        /// Book rule
        /// </summary>
        Book,

        /// <summary>
        /// قاعدة المستخدم
        /// User rule
        /// </summary>
        User,

        /// <summary>
        /// قاعدة عامة
        /// General rule
        /// </summary>
        General
    }

    /// <summary>
    /// أكواد أخطاء قواعد الأعمال
    /// Business rule error codes
    /// </summary>
    public static class BusinessRuleErrorCodes
    {
        // أخطاء الاستعارة - Borrowing errors
        public const string BookNotAvailable = "BOOK_NOT_AVAILABLE";
        public const string UserBorrowingLimitExceeded = "USER_BORROWING_LIMIT_EXCEEDED";
        public const string UserNotActive = "USER_NOT_ACTIVE";
        public const string BookAlreadyBorrowedByUser = "BOOK_ALREADY_BORROWED_BY_USER";
        public const string UserHasOverdueBooks = "USER_HAS_OVERDUE_BOOKS";

        // أخطاء الإرجاع - Return errors
        public const string BorrowingNotFound = "BORROWING_NOT_FOUND";
        public const string BorrowingAlreadyReturned = "BORROWING_ALREADY_RETURNED";
        public const string UnauthorizedReturn = "UNAUTHORIZED_RETURN";

        // أخطاء التجديد - Renewal errors
        public const string RenewalLimitExceeded = "RENEWAL_LIMIT_EXCEEDED";
        public const string BorrowingOverdue = "BORROWING_OVERDUE";
        public const string BookReservedByOthers = "BOOK_RESERVED_BY_OTHERS";

        // أخطاء الكتاب - Book errors
        public const string BookIsbnExists = "BOOK_ISBN_EXISTS";
        public const string BookHasActiveBorrowings = "BOOK_HAS_ACTIVE_BORROWINGS";
        public const string InvalidBookData = "INVALID_BOOK_DATA";

        // أخطاء المستخدم - User errors
        public const string UserEmailExists = "USER_EMAIL_EXISTS";
        public const string UserHasActiveBorrowings = "USER_HAS_ACTIVE_BORROWINGS";
        public const string InvalidUserData = "INVALID_USER_DATA";
        public const string CannotDeleteAdminUser = "CANNOT_DELETE_ADMIN_USER";
    }
}
