using LibraryManagementSystem.DAL.Repositories;
using System.Data;

namespace LibraryManagementSystem.DAL.UnitOfWork
{
    /// <summary>
    /// واجهة وحدة العمل لإدارة المعاملات والمستودعات
    /// Unit of Work interface for managing transactions and repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// مستودع الكتب
        /// Books repository
        /// </summary>
        IBookRepository Books { get; }

        /// <summary>
        /// مستودع المستخدمين
        /// Users repository
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// مستودع الاستعارات
        /// Borrowings repository
        /// </summary>
        IBorrowingRepository Borrowings { get; }

        /// <summary>
        /// اتصال قاعدة البيانات
        /// Database connection
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// المعاملة الحالية
        /// Current transaction
        /// </summary>
        IDbTransaction? Transaction { get; }

        /// <summary>
        /// بدء معاملة جديدة
        /// Begin a new transaction
        /// </summary>
        /// <param name="isolationLevel">مستوى العزل - Isolation level</param>
        /// <returns>المعاملة الجديدة - New transaction</returns>
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// بدء معاملة جديدة بشكل غير متزامن
        /// Begin a new transaction asynchronously
        /// </summary>
        /// <param name="isolationLevel">مستوى العزل - Isolation level</param>
        /// <returns>المعاملة الجديدة - New transaction</returns>
        Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// تأكيد المعاملة
        /// Commit the transaction
        /// </summary>
        void Commit();

        /// <summary>
        /// تأكيد المعاملة بشكل غير متزامن
        /// Commit the transaction asynchronously
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// إلغاء المعاملة
        /// Rollback the transaction
        /// </summary>
        void Rollback();

        /// <summary>
        /// إلغاء المعاملة بشكل غير متزامن
        /// Rollback the transaction asynchronously
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// حفظ التغييرات
        /// Save changes
        /// </summary>
        /// <returns>عدد الصفوف المتأثرة - Number of affected rows</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// تنفيذ عملية داخل معاملة
        /// Execute operation within a transaction
        /// </summary>
        /// <typeparam name="T">نوع النتيجة - Result type</typeparam>
        /// <param name="operation">العملية المراد تنفيذها - Operation to execute</param>
        /// <param name="isolationLevel">مستوى العزل - Isolation level</param>
        /// <returns>نتيجة العملية - Operation result</returns>
        Task<T> ExecuteInTransactionAsync<T>(Func<IUnitOfWork, Task<T>> operation, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// تنفيذ عملية داخل معاملة بدون إرجاع قيمة
        /// Execute operation within a transaction without return value
        /// </summary>
        /// <param name="operation">العملية المراد تنفيذها - Operation to execute</param>
        /// <param name="isolationLevel">مستوى العزل - Isolation level</param>
        Task ExecuteInTransactionAsync(Func<IUnitOfWork, Task> operation, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}
