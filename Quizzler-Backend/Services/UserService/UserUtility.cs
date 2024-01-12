using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace Quizzler_Backend.Services
{
    public class UserUtility(QuizzlerDbContext context, IConfiguration configuration, GlobalService globalService)
    {
        private readonly QuizzlerDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly GlobalService _globalService = globalService;

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["JwtKey"] ?? throw new InvalidOperationException("JwtKey configuration value is null or empty.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"],
                audience: _configuration["JwtIssuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<bool> AreCredentialsCorrect(UserLoginDto userLoginDto)
        {
            var user = await _context.User.Include(u => u.LoginInfo)
                .FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);

            if (user == null)
                return false;

            string generatedPasswordHash = _globalService.HashPassword(userLoginDto.Password, user.LoginInfo.Salt);
            return generatedPasswordHash == user.LoginInfo.PasswordHash;
        }
        public async Task<bool> EmailExists(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }
        public async Task<bool> UsernameExists(string username)
        {
            return await _context.User.AnyAsync(u => u.Username == username);
        }
        public bool IsEmailCorrect(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public bool IsPasswordGoodEnough(string password)
        {
            return password.Length >= 8;
        }
        public User CreateUser(UserRegisterDto userRegisterDto)
        {
            var salt = _globalService.CreateSalt();
            var passwordHash = _globalService.HashPassword(userRegisterDto.Password, salt);

            return new User
            {
                Email = userRegisterDto.Email,
                Username = userRegisterDto.Username,
                FirstName = userRegisterDto.FirstName,
                LastName = userRegisterDto.LastName,
                DateRegistered = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                LoginInfo = new LoginInfo
                {
                    Salt = salt,
                    PasswordHash = passwordHash
                },
            };
        }
    }
}