using HospitalApi.Data;
using HospitalApi.Dtos;
using HospitalApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HospitalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            // below is for hash the password...
            var hashedPassword = PasswordHasher.Hash(request.Password);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Username == request.Username &&
                    u.PasswordHash == hashedPassword);

            if (user == null)
                return Unauthorized("Invalid username or password");

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Token = token,
                Username = user.Username,
                Role = user.Role.Name
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(jwtSettings["DurationInMinutes"]!)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            var existingUser = await _context.Users
                .AnyAsync(u => u.Username == dto.Username);

            if (existingUser)
                return BadRequest("Username already exists");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                RoleId = dto.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User created successfully");
        }
    }
}