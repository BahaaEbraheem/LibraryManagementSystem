using LibraryManagementSystem.DAL.Data;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.Extensions.Logging;
using System.Data;

namespace LibraryManagementSystem.DAL.UnitOfWork
{
    /// <summary>
    /// تنفيذ وحدة العمل لإدارة المعاملات والمستودعات
    /// Unit of Work implementation for managing transactions and repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private bool _disposed = false;

        // المستودعات - Repositories
        private IBookRepository? _books;
        private IUserRepository? _users;
        private IBorrowingRepository? _borrowings;

        /// <summary>
        /// منشئ وحدة العمل
        /// Unit of Work constructor
        /// </summary>
        public UnitOfWork(IDatabaseConnectionFactory connectionFactory, ILogger<UnitOfWork> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// اتصال قاعدة البيانات
        /// Database connection
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = _connectionFactory.CreateConnection();
                    _logger.LogDebug("تم إنشاء اتصال جديد بقاعدة البيانات - Created new database connection");
                }
                return _connection;
            }
        }

        /// <summary>
        /// المعاملة الحالية
        /// Current transaction
        /// </summary>
        public IDbTransaction? Transaction => _transaction;

        /// <summary>
        /// مستودع الكتب
        /// Books repository
        /// </summary>
        public IBookRepository Books
        {
            get
            {
                if (_books == null)
                {
                    // إنشاء مستودع الكتب مع الاتصال المشترك
                    // Create books repository with shared connection
                    _books = new BookRepository(_connectionFactory, null!, null!);
                    _logger.LogDebug("تم إنشاء مستودع الكتب - Created books repository");
                }
                return _books;
            }
        }

        /// <summary>
        /// مستودع المستخدمين
        /// Users repository
        /// </summary>
        public IUserRepository Users
        {
            get
            {
                if (_users == null)
                {
                    // إنشاء مستودع المستخدمين مع الاتصال المشترك
                    // Create users repository with shared connection
                    _users = new UserRepository(_connectionFactory, null!, null!);
                    _logger.LogDebug("تم إنشاء مستودع المستخدمين - Created users repository");
                }
                return _users;
            }
        }

        /// <summary>
        /// مستودع الاستعارات
        /// Borrowings repository
        /// </summary>
        public IBorrowingRepository Borrowings
        {
            get
            {
                if (_borrowings == null)
                {
                    // إنشاء مستودع الاستعارات مع الاتصال المشترك
                    // Create borrowings repository with shared connection
                    _borrowings = new BorrowingRepository(_connectionFactory, null!, null!);
                    _logger.LogDebug("تم إنشاء مستودع الاستعارات - Created borrowings repository");
                }
                return _borrowings;
            }
        }

        /// <summary>
        /// بدء معاملة جديدة
        /// Begin a new transaction
        /// </summary>
        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("معاملة نشطة موجودة بالفعل - Transaction already active");
            }

            _transaction = Connection.BeginTransaction(isolationLevel);
            _logger.LogDebug("تم بدء معاملة جديدة بمستوى العزل {IsolationLevel} - Started new transaction with isolation level", 
                isolationLevel);

            return _transaction;
        }

        /// <summary>
        /// بدء معاملة جديدة بشكل غير متزامن
        /// Begin a new transaction asynchronously
        /// </summary>
        public async Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("معاملة نشطة موجودة بالفعل - Transaction already active");
            }

            // للاتصالات التي تدعم المعاملات غير المتزامنة
            // For connections that support async transactions
            if (Connection is System.Data.Common.DbConnection dbConnection)
            {
                _transaction = await dbConnection.BeginTransactionAsync(isolationLevel);
            }
            else
            {
                _transaction = Connection.BeginTransaction(isolationLevel);
            }

            _logger.LogDebug("تم بدء معاملة جديدة بشكل غير متزامن بمستوى العزل {IsolationLevel} - Started new async transaction with isolation level", 
                isolationLevel);

            return _transaction;
        }

        /// <summary>
        /// تأكيد المعاملة
        /// Commit the transaction
        /// </summary>
        public void Commit()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("لا توجد معاملة نشطة للتأكيد - No active transaction to commit");
            }

            try
            {
                _transaction.Commit();
                _logger.LogDebug("تم تأكيد المعاملة بنجاح - Transaction committed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تأكيد المعاملة - Error committing transaction");
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// تأكيد المعاملة بشكل غير متزامن
        /// Commit the transaction asynchronously
        /// </summary>
        public async Task CommitAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("لا توجد معاملة نشطة للتأكيد - No active transaction to commit");
            }

            try
            {
                // للمعاملات التي تدعم التأكيد غير المتزامن
                // For transactions that support async commit
                if (_transaction is System.Data.Common.DbTransaction dbTransaction)
                {
                    await dbTransaction.CommitAsync();
                }
                else
                {
                    _transaction.Commit();
                }

                _logger.LogDebug("تم تأكيد المعاملة بنجاح بشكل غير متزامن - Transaction committed successfully asynchronously");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تأكيد المعاملة بشكل غير متزامن - Error committing transaction asynchronously");
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// إلغاء المعاملة
        /// Rollback the transaction
        /// </summary>
        public void Rollback()
        {
            if (_transaction == null)
            {
                _logger.LogWarning("محاولة إلغاء معاملة غير موجودة - Attempted to rollback non-existent transaction");
                return;
            }

            try
            {
                _transaction.Rollback();
                _logger.LogDebug("تم إلغاء المعاملة بنجاح - Transaction rolled back successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إلغاء المعاملة - Error rolling back transaction");
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// إلغاء المعاملة بشكل غير متزامن
        /// Rollback the transaction asynchronously
        /// </summary>
        public async Task RollbackAsync()
        {
            if (_transaction == null)
            {
                _logger.LogWarning("محاولة إلغاء معاملة غير موجودة بشكل غير متزامن - Attempted to rollback non-existent transaction asynchronously");
                return;
            }

            try
            {
                // للمعاملات التي تدعم الإلغاء غير المتزامن
                // For transactions that support async rollback
                if (_transaction is System.Data.Common.DbTransaction dbTransaction)
                {
                    await dbTransaction.RollbackAsync();
                }
                else
                {
                    _transaction.Rollback();
                }

                _logger.LogDebug("تم إلغاء المعاملة بنجاح بشكل غير متزامن - Transaction rolled back successfully asynchronously");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إلغاء المعاملة بشكل غير متزامن - Error rolling back transaction asynchronously");
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// حفظ التغييرات
        /// Save changes
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            // في Dapper، التغييرات تُحفظ فوراً
            // In Dapper, changes are saved immediately
            // هذه الطريقة موجودة للتوافق مع نمط Unit of Work
            // This method exists for Unit of Work pattern compatibility
            
            if (_transaction != null)
            {
                await CommitAsync();
                return 1; // إشارة إلى نجاح العملية
            }

            return 0;
        }

        /// <summary>
        /// تنفيذ عملية داخل معاملة
        /// Execute operation within a transaction
        /// </summary>
        public async Task<T> ExecuteInTransactionAsync<T>(Func<IUnitOfWork, Task<T>> operation, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            var wasTransactionActive = _transaction != null;

            if (!wasTransactionActive)
            {
                await BeginTransactionAsync(isolationLevel);
            }

            try
            {
                var result = await operation(this);

                if (!wasTransactionActive)
                {
                    await CommitAsync();
                }

                return result;
            }
            catch
            {
                if (!wasTransactionActive && _transaction != null)
                {
                    await RollbackAsync();
                }
                throw;
            }
        }

        /// <summary>
        /// تنفيذ عملية داخل معاملة بدون إرجاع قيمة
        /// Execute operation within a transaction without return value
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<IUnitOfWork, Task> operation, 
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            await ExecuteInTransactionAsync(async uow =>
            {
                await operation(uow);
                return true;
            }, isolationLevel);
        }

        /// <summary>
        /// تحرير الموارد
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// تحرير الموارد المُدارة وغير المُدارة
        /// Dispose managed and unmanaged resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    if (_transaction != null)
                    {
                        _logger.LogWarning("إلغاء معاملة نشطة أثناء التخلص من وحدة العمل - Rolling back active transaction during UnitOfWork disposal");
                        _transaction.Rollback();
                        _transaction.Dispose();
                        _transaction = null;
                    }

                    _connection?.Dispose();
                    _connection = null;

                    _logger.LogDebug("تم التخلص من وحدة العمل بنجاح - UnitOfWork disposed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ أثناء التخلص من وحدة العمل - Error during UnitOfWork disposal");
                }

                _disposed = true;
            }
        }
    }
}
