using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// خدمة المستخدمين
    /// User service
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على جميع المستخدمين
        /// Get all users
        /// </summary>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogDebug("الحصول على جميع المستخدمين - Getting all users");
                var users = await _userRepository.GetAllAsync();
                _logger.LogDebug("تم الحصول على {Count} مستخدم - Retrieved {Count} users", users.Count());
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدمين - Error getting users");
                throw;
            }
        }

        /// <summary>
        /// الحصول على مستخدم بالمعرف
        /// Get user by ID
        /// </summary>
        public async Task<ServiceResult<User>> GetUserByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("تم تمرير معرف مستخدم غير صحيح: {UserId} - Invalid user ID provided", id);
                    return ServiceResult<User>.Failure("معرف المستخدم غير صحيح - Invalid user ID");
                }

                _logger.LogDebug("الحصول على المستخدم {UserId} - Getting user by ID", id);
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    _logger.LogWarning("لم يتم العثور على المستخدم {UserId} - User not found", id);
                    return ServiceResult<User>.Failure("المستخدم غير موجود - User not found");
                }

                _logger.LogDebug("تم العثور على المستخدم {UserId} - User found", id);
                return ServiceResult<User>.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدم {UserId} - Error getting user", id);
                return ServiceResult<User>.Failure("خطأ في الحصول على المستخدم - Error getting user");
            }
        }

        /// <summary>
        /// الحصول على مستخدم بالبريد الإلكتروني
        /// Get user by email
        /// </summary>
        public async Task<ServiceResult<User>> GetUserByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("تم تمرير بريد إلكتروني فارغ - Empty email provided");
                    return ServiceResult<User>.Failure("البريد الإلكتروني مطلوب - Email is required");
                }

                _logger.LogDebug("الحصول على المستخدم بالبريد الإلكتروني {Email} - Getting user by email", email);
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("لم يتم العثور على المستخدم بالبريد الإلكتروني {Email} - User not found by email", email);
                    return ServiceResult<User>.Failure("المستخدم غير موجود - User not found");
                }

                _logger.LogDebug("تم العثور على المستخدم بالبريد الإلكتروني {Email} - User found by email", email);
                return ServiceResult<User>.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدم بالبريد الإلكتروني {Email} - Error getting user by email", email);
                return ServiceResult<User>.Failure("خطأ في الحصول على المستخدم - Error getting user");
            }
        }

        /// <summary>
        /// الحصول على المستخدمين النشطين
        /// Get active users
        /// </summary>
        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            try
            {
                _logger.LogDebug("الحصول على المستخدمين النشطين - Getting active users");
                var users = await _userRepository.GetActiveUsersAsync();
                _logger.LogDebug("تم الحصول على {Count} مستخدم نشط - Retrieved {Count} active users", users.Count());
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدمين النشطين - Error getting active users");
                throw;
            }
        }

        /// <summary>
        /// البحث عن المستخدمين
        /// Search users
        /// </summary>
        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogDebug("مصطلح البحث فارغ، إرجاع جميع المستخدمين - Empty search term, returning all users");
                    return await GetAllUsersAsync();
                }

                _logger.LogDebug("البحث عن المستخدمين بالمصطلح: {SearchTerm} - Searching users with term", searchTerm);
                var users = await _userRepository.SearchUsersAsync(searchTerm);
                _logger.LogDebug("تم العثور على {Count} مستخدم - Found {Count} users", users.Count());
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في البحث عن المستخدمين - Error searching users");
                throw;
            }
        }

        /// <summary>
        /// إضافة مستخدم جديد
        /// Add a new user
        /// </summary>
        public async Task<ServiceResult<int>> AddUserAsync(User user)
        {
            try
            {
                if (user == null)
                {
                    _logger.LogWarning("تم تمرير مستخدم فارغ - Null user provided");
                    return ServiceResult<int>.Failure("بيانات المستخدم مطلوبة - User data is required");
                }

                // التحقق من صحة البيانات
                var validationResult = ValidateUser(user);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<int>.Failure(validationResult.ErrorMessage!);
                }

                // التحقق من عدم وجود مستخدم بنفس البريد الإلكتروني
                if (await _userRepository.ExistsByEmailAsync(user.Email))
                {
                    _logger.LogWarning("محاولة إضافة مستخدم بريد إلكتروني موجود: {Email} - Attempt to add user with existing email", user.Email);
                    return ServiceResult<int>.Failure("البريد الإلكتروني موجود بالفعل - Email already exists");
                }

                _logger.LogDebug("إضافة مستخدم جديد: {Email} - Adding new user", user.Email);

                // تشفير كلمة المرور إذا لم تكن مشفرة بالفعل
                // Hash password if not already hashed
                if (!string.IsNullOrEmpty(user.PasswordHash) && !IsPasswordHashed(user.PasswordHash))
                {
                    user.PasswordHash = HashPassword(user.PasswordHash);
                }

                user.MembershipDate = DateTime.Now;
                user.CreatedDate = DateTime.Now;
                user.ModifiedDate = DateTime.Now;
                user.IsActive = true;

                var userId = await _userRepository.AddAsync(user);

                _logger.LogInformation("تم إضافة مستخدم جديد بالمعرف {UserId} - New user added with ID", userId);
                return ServiceResult<int>.Success(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة المستخدم - Error adding user");
                return ServiceResult<int>.Failure("خطأ في إضافة المستخدم - Error adding user");
            }
        }

        /// <summary>
        /// تحديث مستخدم موجود
        /// Update an existing user
        /// </summary>
        public async Task<ServiceResult<bool>> UpdateUserAsync(User user)
        {
            try
            {
                if (user == null)
                {
                    _logger.LogWarning("تم تمرير مستخدم فارغ للتحديث - Null user provided for update");
                    return ServiceResult<bool>.Failure("بيانات المستخدم مطلوبة - User data is required");
                }

                // التحقق من وجود المستخدم
                var existingUser = await _userRepository.GetByIdAsync(user.UserId);
                if (existingUser == null)
                {
                    _logger.LogWarning("محاولة تحديث مستخدم غير موجود: {UserId} - Attempt to update non-existing user", user.UserId);
                    return ServiceResult<bool>.Failure("المستخدم غير موجود - User not found");
                }

                // التحقق من صحة البيانات
                var validationResult = ValidateUser(user);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<bool>.Failure(validationResult.ErrorMessage!);
                }

                // التحقق من عدم وجود مستخدم آخر بنفس البريد الإلكتروني
                var userWithSameEmail = await _userRepository.GetByEmailAsync(user.Email);
                if (userWithSameEmail != null && userWithSameEmail.UserId != user.UserId)
                {
                    _logger.LogWarning("محاولة تحديث مستخدم ببريد إلكتروني موجود: {Email} - Attempt to update user with existing email", user.Email);
                    return ServiceResult<bool>.Failure("البريد الإلكتروني موجود بالفعل - Email already exists");
                }

                _logger.LogDebug("تحديث المستخدم {UserId} - Updating user", user.UserId);

                user.ModifiedDate = DateTime.Now;
                var success = await _userRepository.UpdateAsync(user);

                if (success)
                {
                    _logger.LogInformation("تم تحديث المستخدم {UserId} بنجاح - User updated successfully", user.UserId);
                }
                else
                {
                    _logger.LogWarning("فشل في تحديث المستخدم {UserId} - Failed to update user", user.UserId);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث المستخدم {UserId} - Error updating user", user?.UserId);
                return ServiceResult<bool>.Failure("خطأ في تحديث المستخدم - Error updating user");
            }
        }

        /// <summary>
        /// حذف مستخدم
        /// Delete a user
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteUserAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("تم تمرير معرف مستخدم غير صحيح للحذف: {UserId} - Invalid user ID provided for deletion", id);
                    return ServiceResult<bool>.Failure("معرف المستخدم غير صحيح - Invalid user ID");
                }

                // التحقق من وجود المستخدم
                var existingUser = await _userRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    _logger.LogWarning("محاولة حذف مستخدم غير موجود: {UserId} - Attempt to delete non-existing user", id);
                    return ServiceResult<bool>.Failure("المستخدم غير موجود - User not found");
                }

                _logger.LogDebug("حذف المستخدم {UserId} - Deleting user", id);
                var success = await _userRepository.DeleteAsync(id);

                if (success)
                {
                    _logger.LogInformation("تم حذف المستخدم {UserId} بنجاح - User deleted successfully", id);
                }
                else
                {
                    _logger.LogWarning("فشل في حذف المستخدم {UserId} - Failed to delete user", id);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المستخدم {UserId} - Error deleting user", id);
                return ServiceResult<bool>.Failure("خطأ في حذف المستخدم - Error deleting user");
            }
        }

        /// <summary>
        /// تفعيل أو إلغاء تفعيل مستخدم
        /// Activate or deactivate a user
        /// </summary>
        public async Task<ServiceResult<bool>> SetActiveStatusAsync(int id, bool isActive)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("تم تمرير معرف مستخدم غير صحيح: {UserId} - Invalid user ID provided", id);
                    return ServiceResult<bool>.Failure("معرف المستخدم غير صحيح - Invalid user ID");
                }

                _logger.LogDebug("تغيير حالة المستخدم {UserId} إلى {IsActive} - Changing user status to", id, isActive);
                var success = await _userRepository.SetActiveStatusAsync(id, isActive);

                if (success)
                {
                    _logger.LogInformation("تم تغيير حالة المستخدم {UserId} إلى {IsActive} - User status changed to", id, isActive);
                }
                else
                {
                    _logger.LogWarning("فشل في تغيير حالة المستخدم {UserId} - Failed to change user status", id);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تغيير حالة المستخدم {UserId} - Error changing user status", id);
                return ServiceResult<bool>.Failure("خطأ في تغيير حالة المستخدم - Error changing user status");
            }
        }

        /// <summary>
        /// التحقق من وجود مستخدم بالبريد الإلكتروني
        /// Check if user exists by email
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return false;
                }

                return await _userRepository.ExistsByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من وجود المستخدم بالبريد الإلكتروني {Email} - Error checking user existence by email", email);
                return false;
            }
        }

        /// <summary>
        /// التحقق من صحة بيانات المستخدم
        /// Validate user data
        /// </summary>
        private ServiceResult<bool> ValidateUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.FirstName))
            {
                return ServiceResult<bool>.Failure("الاسم الأول مطلوب - First name is required");
            }

            if (string.IsNullOrWhiteSpace(user.LastName))
            {
                return ServiceResult<bool>.Failure("الاسم الأخير مطلوب - Last name is required");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return ServiceResult<bool>.Failure("البريد الإلكتروني مطلوب - Email is required");
            }

            if (!IsValidEmail(user.Email))
            {
                return ServiceResult<bool>.Failure("البريد الإلكتروني غير صحيح - Invalid email format");
            }

            return ServiceResult<bool>.Success(true);
        }

        /// <summary>
        /// التحقق من صحة تنسيق البريد الإلكتروني
        /// Validate email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// تشفير كلمة المرور
        /// Hash password
        /// </summary>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "LibraryManagementSalt"));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// التحقق من كون كلمة المرور مشفرة بالفعل
        /// Check if password is already hashed
        /// </summary>
        private bool IsPasswordHashed(string password)
        {
            // كلمة المرور المشفرة تكون عادة أطول من 40 حرف وتحتوي على أحرف Base64
            // Hashed passwords are usually longer than 40 characters and contain Base64 characters
            if (string.IsNullOrEmpty(password) || password.Length < 40)
                return false;

            try
            {
                // محاولة فك تشفير Base64 للتحقق من صحة التنسيق
                // Try to decode Base64 to verify format
                Convert.FromBase64String(password);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
