using LibraryManagementSystem.BLL.Validation;
using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.Enums;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// خدمة التحقق من الصلاحيات
    /// Authorization service
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            IUserRepository userRepository,
            IBorrowingRepository borrowingRepository,
            ILogger<AuthorizationService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _borrowingRepository = borrowingRepository ?? throw new ArgumentNullException(nameof(borrowingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// التحقق من صلاحية إدارة الكتب
        /// Check if user can manage books
        /// </summary>
        public async Task<ServiceResult<bool>> CanManageBooksAsync(int userId)
        {
            try
            {
                var userRoleResult = await GetUserRoleAsync(userId);
                if (!userRoleResult.IsSuccess)
                {
                    return ServiceResult<bool>.Failure(userRoleResult.ErrorMessage ?? "فشل في الحصول على دور المستخدم");
                }

                var canManage = userRoleResult.Data.CanManageBooks();
                
                if (!canManage)
                {
                    _logger.LogWarning("المستخدم {UserId} ليس لديه صلاحية إدارة الكتب - User does not have permission to manage books", userId);
                }

                return ServiceResult<bool>.Success(canManage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صلاحية إدارة الكتب للمستخدم {UserId} - Error checking book management permission", userId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء التحقق من الصلاحيات");
            }
        }

        /// <summary>
        /// التحقق من صلاحية إدارة المستخدمين
        /// Check if user can manage users
        /// </summary>
        public async Task<ServiceResult<bool>> CanManageUsersAsync(int userId)
        {
            try
            {
                var userRoleResult = await GetUserRoleAsync(userId);
                if (!userRoleResult.IsSuccess)
                {
                    return ServiceResult<bool>.Failure(userRoleResult.ErrorMessage ?? "فشل في الحصول على دور المستخدم");
                }

                var canManage = userRoleResult.Data.CanManageUsers();
                
                if (!canManage)
                {
                    _logger.LogWarning("المستخدم {UserId} ليس لديه صلاحية إدارة المستخدمين - User does not have permission to manage users", userId);
                }

                return ServiceResult<bool>.Success(canManage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صلاحية إدارة المستخدمين للمستخدم {UserId} - Error checking user management permission", userId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء التحقق من الصلاحيات");
            }
        }

        /// <summary>
        /// التحقق من صلاحية إدارة الاستعارات
        /// Check if user can manage borrowings
        /// </summary>
        public async Task<ServiceResult<bool>> CanManageBorrowingsAsync(int userId)
        {
            try
            {
                var userRoleResult = await GetUserRoleAsync(userId);
                if (!userRoleResult.IsSuccess)
                {
                    return ServiceResult<bool>.Failure(userRoleResult.ErrorMessage ?? "فشل في الحصول على دور المستخدم");
                }

                var canManage = userRoleResult.Data.CanManageBorrowings();
                
                if (!canManage)
                {
                    _logger.LogWarning("المستخدم {UserId} ليس لديه صلاحية إدارة الاستعارات - User does not have permission to manage borrowings", userId);
                }

                return ServiceResult<bool>.Success(canManage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صلاحية إدارة الاستعارات للمستخدم {UserId} - Error checking borrowing management permission", userId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء التحقق من الصلاحيات");
            }
        }

        /// <summary>
        /// التحقق من صلاحية عرض الإحصائيات
        /// Check if user can view statistics
        /// </summary>
        public async Task<ServiceResult<bool>> CanViewStatisticsAsync(int userId)
        {
            try
            {
                var userRoleResult = await GetUserRoleAsync(userId);
                if (!userRoleResult.IsSuccess)
                {
                    return ServiceResult<bool>.Failure(userRoleResult.ErrorMessage ?? "فشل في الحصول على دور المستخدم");
                }

                var canView = userRoleResult.Data.CanViewStatistics();
                
                if (!canView)
                {
                    _logger.LogWarning("المستخدم {UserId} ليس لديه صلاحية عرض الإحصائيات - User does not have permission to view statistics", userId);
                }

                return ServiceResult<bool>.Success(canView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صلاحية عرض الإحصائيات للمستخدم {UserId} - Error checking statistics view permission", userId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء التحقق من الصلاحيات");
            }
        }

        /// <summary>
        /// التحقق من صلاحية استعارة الكتب
        /// Check if user can borrow books
        /// </summary>
        public async Task<ServiceResult<bool>> CanBorrowBooksAsync(int userId)
        {
            try
            {
                // التحقق من نشاط المستخدم أولاً
                // Check user activity first
                var isActiveResult = await IsUserActiveAsync(userId);
                if (!isActiveResult.IsSuccess || !isActiveResult.Data)
                {
                    return ServiceResult<bool>.Failure("المستخدم غير نشط أو غير موجود");
                }

                // جميع المستخدمين النشطين يمكنهم الاستعارة
                // All active users can borrow
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صلاحية الاستعارة للمستخدم {UserId} - Error checking borrowing permission", userId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء التحقق من الصلاحيات");
            }
        }

        /// <summary>
        /// التحقق من صلاحية الوصول للاستعارة المحددة
        /// Check if user can access specific borrowing
        /// </summary>
        public async Task<ServiceResult<bool>> CanAccessBorrowingAsync(int userId, int borrowingId)
        {
            try
            {
                // المديرون يمكنهم الوصول لجميع الاستعارات
                // Administrators can access all borrowings
                var canManageResult = await CanManageBorrowingsAsync(userId);
                if (canManageResult.IsSuccess && canManageResult.Data)
                {
                    return ServiceResult<bool>.Success(true);
                }

                // المستخدمون العاديون يمكنهم الوصول لاستعاراتهم فقط
                // Regular users can only access their own borrowings
                var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
                if (borrowing == null)
                {
                    return ServiceResult<bool>.Failure("الاستعارة غير موجودة");
                }

                var canAccess = borrowing.UserId == userId;
                
                if (!canAccess)
                {
                    _logger.LogWarning("المستخدم {UserId} حاول الوصول للاستعارة {BorrowingId} التي لا تخصه - User tried to access borrowing that doesn't belong to them", userId, borrowingId);
                }

                return ServiceResult<bool>.Success(canAccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من صلاحية الوصول للاستعارة {BorrowingId} للمستخدم {UserId} - Error checking borrowing access permission", borrowingId, userId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء التحقق من الصلاحيات");
            }
        }

        /// <summary>
        /// الحصول على دور المستخدم
        /// Get user role
        /// </summary>
        public async Task<ServiceResult<UserRole>> GetUserRoleAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("المستخدم {UserId} غير موجود - User not found", userId);
                    return ServiceResult<UserRole>.Failure("المستخدم غير موجود");
                }

                return ServiceResult<UserRole>.Success(user.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على دور المستخدم {UserId} - Error getting user role", userId);
                return ServiceResult<UserRole>.Failure("حدث خطأ أثناء الحصول على دور المستخدم");
            }
        }

        /// <summary>
        /// التحقق من وجود المستخدم ونشاطه
        /// Check if user exists and is active
        /// </summary>
        public async Task<ServiceResult<bool>> IsUserActiveAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("المستخدم {UserId} غير موجود - User not found", userId);
                    return ServiceResult<bool>.Success(false);
                }

                return ServiceResult<bool>.Success(user.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من نشاط المستخدم {UserId} - Error checking user activity", userId);
                return ServiceResult<bool>.Failure("حدث خطأ أثناء التحقق من نشاط المستخدم");
            }
        }
    }
}
