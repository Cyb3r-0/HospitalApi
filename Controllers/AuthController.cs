using HospitalApi.Data;
using HospitalApi.Dtos;
using HospitalApi.Helpers;
using HospitalApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HospitalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [EnableRateLimiting("login")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Username and password are required." });

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for: {Username} IP: {IP}", request.Username, GetIpAddress());
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid username or password." });
            }

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user.Id, GetIpAddress());

            _context.RefreshTokens.Add(refreshToken);

            var expiredTokens = await _context.RefreshTokens
                .Where(t => t.UserId == user.Id && t.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();
            _context.RefreshTokens.RemoveRange(expiredTokens);

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} logged in", user.Username);

            return Ok(new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful.",
                Data = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    AccessTokenExpiry = DateTime.UtcNow.AddMinutes(GetAccessTokenDurationMinutes()),
                    Username = user.Username,
                    Role = user.Role.Name
                }
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Refresh token is required." });

            var storedToken = await _context.RefreshTokens
                .Include(t => t.User).ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

            if (storedToken == null)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid refresh token." });

            if (storedToken.IsRevoked)
            {
                _logger.LogWarning("Token reuse detected for user {UserId} — revoking all sessions", storedToken.UserId);
                await RevokeAllUserTokens(storedToken.UserId, GetIpAddress());
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Refresh token reuse detected. All sessions revoked. Please log in again."
                });
            }

            if (storedToken.IsExpired)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Refresh token expired. Please log in again." });

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = GetIpAddress();

            var newAccessToken = GenerateAccessToken(storedToken.User);
            var newRefreshToken = GenerateRefreshToken(storedToken.UserId, GetIpAddress());
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Token refreshed successfully.",
                Data = new LoginResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    AccessTokenExpiry = DateTime.UtcNow.AddMinutes(GetAccessTokenDurationMinutes()),
                    Username = storedToken.User.Username,
                    Role = storedToken.User.Role.Name
                }
            });
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(RevokeTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Refresh token is required." });

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

            if (storedToken == null || storedToken.IsRevoked)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Token is invalid or already revoked." });

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = GetIpAddress();
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object> { Success = true, Message = "Logged out successfully." });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Username and password are required." });

            if (dto.Password.Length < 8)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Password must be at least 8 characters." });

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Username already exists." });
            }

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                RoleId = dto.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Username} RoleId: {RoleId}", dto.Username, dto.RoleId);

            return Ok(new ApiResponse<object> { Success = true, Message = $"User '{dto.Username}' created successfully." });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "New password and confirmation do not match." });

            if (dto.NewPassword.Length < 8)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Password must be at least 8 characters." });

            if (dto.NewPassword == dto.CurrentPassword)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "New password must be different from current password." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Unauthorized." });

            var user = await _context.Users.FindAsync(int.Parse(userIdClaim));
            if (user == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "User not found." });

            if (!PasswordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Current password is incorrect." });

            user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
            await _context.SaveChangesAsync();

            await RevokeAllUserTokens(user.Id, GetIpAddress());

            _logger.LogInformation("Password changed for user {Username}", user.Username);

            return Ok(new ApiResponse<object> { Success = true, Message = "Password changed. Please log in again." });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "New password must be at least 8 characters." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = $"User '{dto.Username}' not found." });

            user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
            await _context.SaveChangesAsync();

            await RevokeAllUserTokens(user.Id, GetIpAddress());

            _logger.LogInformation("Password reset by SuperAdmin for user {Username}", dto.Username);

            return Ok(new ApiResponse<object> { Success = true, Message = $"Password for '{dto.Username}' reset successfully." });
        }

        private string GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetAccessTokenDurationMinutes()),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(int userId, string? ip)
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(bytes),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ip
            };
        }

        private async Task RevokeAllUserTokens(int userId, string? ip)
        {
            var tokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();
            foreach (var t in tokens)
            {
                t.IsRevoked = true;
                t.RevokedAt = DateTime.UtcNow;
                t.RevokedByIp = ip;
            }
            await _context.SaveChangesAsync();
        }

        private double GetAccessTokenDurationMinutes()
        {
            var val = _configuration["Jwt:DurationInMinutes"];
            return double.TryParse(val, out var mins) ? mins : 60;
        }

        private string? GetIpAddress()
        {
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var fwd))
                return fwd.FirstOrDefault();
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }

        //[HttpPost("rehash-passwords")]
        //public async Task<IActionResult> RehashPasswords([FromBody] string adminKey)
        //{
        //    if (adminKey != "REHASH-2026") return Unauthorized();

        //    var users = await _context.Users.ToListAsync();
        //    foreach (var user in users)
        //    {
        //        // If not already BCrypt (BCrypt hashes start with $2)
        //        //if (!user.PasswordHash.StartsWith("$2"))
        //        {
        //            // Can't reverse old hash — reset to a temporary password
        //            user.PasswordHash = PasswordHasher.Hash("Admin#123");
        //        }
        //    }
        //    await _context.SaveChangesAsync();
        //    return Ok($"Rehashed {users.Count} users. Login with TempPass@123 then change password.");
        //}
    }
}