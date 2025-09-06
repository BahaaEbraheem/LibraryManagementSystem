using LibraryManagementSystem.DAL.Models;
using LibraryManagementSystem.DAL.Models.DTOs.AuthenticationDTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.BLL.Services
{
    /// <summary>
    /// خدمة JWT للمصادقة والتفويض
    /// JWT service for authentication and authorization
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtService> _logger;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly TokenValidationParameters _tokenValidationParameters;

        /// <summary>
        /// منشئ خدمة JWT
        /// JWT service constructor
        /// </summary>
        public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
        {
            _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenHandler = new JwtSecurityTokenHandler();

            // إعداد معاملات التحقق من الرمز
            // Setup token validation parameters
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        /// <summary>
        /// إنشاء رمز JWT للمستخدم المسجل دخوله
        /// Generate JWT token for logged in user
        /// </summary>
        public string GenerateToken(LoggedInUserDto loggedInUser)
        {
            try
            {
                _logger.LogDebug("إنشاء رمز JWT للمستخدم {UserId} - Generating JWT token for user", loggedInUser.UserId);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, loggedInUser.UserId.ToString()),
                    new(ClaimTypes.Email, loggedInUser.Email),
                    new(ClaimTypes.Name, loggedInUser.FullName),
                    new(ClaimTypes.Role, loggedInUser.Role.ToString()),
                    new("IsActive", loggedInUser.IsActive.ToString()),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience,
                    SigningCredentials = credentials
                };

                var token = _tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = _tokenHandler.WriteToken(token);

                _logger.LogDebug("تم إنشاء رمز JWT بنجاح للمستخدم {UserId} - JWT token generated successfully for user", loggedInUser.UserId);
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إنشاء رمز JWT للمستخدم {UserId} - Error generating JWT token for user", loggedInUser.UserId);
                throw;
            }
        }

        /// <summary>
        /// التحقق من صحة رمز JWT
        /// Validate JWT token
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return null;

                var principal = _tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

                // التحقق من نوع الرمز
                // Check token type
                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("رمز JWT غير صحيح - Invalid JWT token");
                    return null;
                }

                _logger.LogDebug("تم التحقق من صحة رمز JWT بنجاح - JWT token validated successfully");
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogDebug("انتهت صلاحية رمز JWT - JWT token expired");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل في التحقق من صحة رمز JWT - Failed to validate JWT token");
                return null;
            }
        }

        /// <summary>
        /// الحصول على معرف المستخدم من الرمز
        /// Get user ID from token
        /// </summary>
        public int? GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null)
                    return null;

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "خطأ في الحصول على معرف المستخدم من الرمز - Error getting user ID from token");
                return null;
            }
        }

        /// <summary>
        /// الحصول على دور المستخدم من الرمز
        /// Get user role from token
        /// </summary>
        public DAL.Models.Enums.UserRole? GetUserRoleFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null)
                    return null;

                var roleClaim = principal.FindFirst(ClaimTypes.Role);
                if (roleClaim != null && Enum.TryParse<DAL.Models.Enums.UserRole>(roleClaim.Value, out var role))
                {
                    return role;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "خطأ في الحصول على دور المستخدم من الرمز - Error getting user role from token");
                return null;
            }
        }

    }
}
