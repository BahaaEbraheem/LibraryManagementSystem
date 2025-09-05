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

      
    }
}
