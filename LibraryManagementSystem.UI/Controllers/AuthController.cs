using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Models.DTOs;
using LibraryManagementSystem.UI.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.UI.Controllers
{
    /// <summary>
    /// تحكم في عمليات المصادقة والتفويض
    /// Authentication and authorization operations controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// منشئ التحكم
        /// Controller constructor
        /// </summary>
        public AuthController(
            IAuthenticationService authenticationService,
            IJwtService jwtService,
            IUserService userService,
            ILogger<AuthController> logger)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// تسجيل الدخول بـ JWT
        /// JWT login
        /// </summary>
        /// <param name="loginDto">بيانات تسجيل الدخول</param>
        /// <returns>رمز JWT</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "بيانات غير صحيحة - Invalid data", errors = ModelState });
                }

                _logger.LogInformation("محاولة تسجيل دخول JWT للمستخدم: {Email} - JWT login attempt for user: {Email}", loginDto.Email);

                // التحقق من بيانات المستخدم
                // Validate user credentials
                var authResult = await _authenticationService.LoginAsync(loginDto);
                if (!authResult.IsSuccess || authResult.Data == null)
                {
                    _logger.LogWarning("فشل تسجيل الدخول للمستخدم: {Email} - Login failed for user: {Email}", loginDto.Email);
                    return Unauthorized(new { success = false, message = "بيانات تسجيل الدخول غير صحيحة - Invalid login credentials" });
                }

                var user = authResult.Data;

                // إنشاء رمز JWT
                // Generate JWT token
                var accessToken = _jwtService.GenerateToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var response = new JwtAuthenticationResult
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60), // يجب أن يأتي من الإعدادات
                    TokenType = "Bearer",
                    User = new UserInfo
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        FullName = user.FullName,
                        Role = user.Role.ToString(),
                        IsActive = user.IsActive
                    }
                };

                _logger.LogInformation("تم تسجيل الدخول بنجاح للمستخدم: {Email} - Successful login for user: {Email}", loginDto.Email);
                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تسجيل الدخول - Error during login");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم - Server error occurred" });
            }
        }

        /// <summary>
        /// تسجيل مستخدم جديد
        /// Register new user
        /// </summary>
        /// <param name="registerDto">بيانات التسجيل</param>
        /// <returns>رمز JWT</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "بيانات غير صحيحة - Invalid data", errors = ModelState });
                }

                _logger.LogInformation("محاولة تسجيل مستخدم جديد: {Email} - New user registration attempt: {Email}", registerDto.Email);

                // تسجيل المستخدم
                // Register user
                var registerResult = await _authenticationService.RegisterAsync(registerDto);
                if (!registerResult.IsSuccess)
                {
                    _logger.LogWarning("فشل تسجيل المستخدم: {Email} - Registration failed for user: {Email}", registerDto.Email);
                    return BadRequest(new { success = false, message = registerResult.ErrorMessage });
                }

                // الحصول على بيانات المستخدم المسجل
                // Get registered user data
                var userResult = await _userService.GetUserByIdAsync(registerResult.Data);
                if (!userResult.IsSuccess || userResult.Data == null)
                {
                    return StatusCode(500, new { success = false, message = "حدث خطأ أثناء إنشاء الحساب - Error occurred during account creation" });
                }

                var user = userResult.Data;

                // إنشاء رمز JWT
                // Generate JWT token
                var accessToken = _jwtService.GenerateToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var response = new JwtAuthenticationResult
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    TokenType = "Bearer",
                    User = new UserInfo
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Role = user.Role.ToString(),
                        IsActive = user.IsActive
                    }
                };

                _logger.LogInformation("تم تسجيل المستخدم بنجاح: {Email} - User registered successfully: {Email}", registerDto.Email);
                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تسجيل المستخدم - Error during registration");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم - Server error occurred" });
            }
        }

        /// <summary>
        /// تحديث رمز JWT
        /// Refresh JWT token
        /// </summary>
        /// <param name="request">طلب تحديث الرمز</param>
        /// <returns>رمز JWT جديد</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new { success = false, message = "رموز غير صحيحة - Invalid tokens" });
                }

                _logger.LogDebug("محاولة تحديث رمز JWT - JWT token refresh attempt");

                var newAccessToken = await _jwtService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
                if (string.IsNullOrEmpty(newAccessToken))
                {
                    return Unauthorized(new { success = false, message = "فشل في تحديث الرمز - Failed to refresh token" });
                }

                var response = new
                {
                    accessToken = newAccessToken,
                    refreshToken = _jwtService.GenerateRefreshToken(),
                    expiresAt = DateTime.UtcNow.AddMinutes(60),
                    tokenType = "Bearer"
                };

                _logger.LogDebug("تم تحديث رمز JWT بنجاح - JWT token refreshed successfully");
                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث الرمز - Error refreshing token");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم - Server error occurred" });
            }
        }

        /// <summary>
        /// إبطال رمز JWT
        /// Revoke JWT token
        /// </summary>
        /// <returns>نتيجة الإبطال</returns>
        [HttpPost("revoke")]
        [JwtAuthenticatedOnly]
        public async Task<IActionResult> RevokeToken()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new { success = false, message = "رمز غير صحيح - Invalid token" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();

                _logger.LogDebug("محاولة إبطال رمز JWT - JWT token revocation attempt");

                var result = await _jwtService.RevokeTokenAsync(token);
                if (!result)
                {
                    return BadRequest(new { success = false, message = "فشل في إبطال الرمز - Failed to revoke token" });
                }

                _logger.LogDebug("تم إبطال رمز JWT بنجاح - JWT token revoked successfully");
                return Ok(new { success = true, message = "تم إبطال الرمز بنجاح - Token revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إبطال الرمز - Error revoking token");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم - Server error occurred" });
            }
        }

        /// <summary>
        /// الحصول على معلومات المستخدم الحالي
        /// Get current user information
        /// </summary>
        /// <returns>معلومات المستخدم</returns>
        [HttpGet("me")]
        [JwtAuthenticatedOnly]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { success = false, message = "رمز غير صحيح - Invalid token" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var userId = _jwtService.GetUserIdFromToken(token);

                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "رمز غير صحيح - Invalid token" });
                }

                var userResult = await _userService.GetUserByIdAsync(userId.Value);
                if (!userResult.IsSuccess || userResult.Data == null)
                {
                    return NotFound(new { success = false, message = "المستخدم غير موجود - User not found" });
                }

                var user = userResult.Data;
                var userInfo = new UserInfo
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Role = user.Role.ToString(),
                    IsActive = user.IsActive
                };

                return Ok(new { success = true, data = userInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على معلومات المستخدم - Error getting user information");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم - Server error occurred" });
            }
        }
    }

    /// <summary>
    /// طلب تحديث الرمز
    /// Refresh token request
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// رمز الوصول
        /// Access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// رمز التحديث
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
