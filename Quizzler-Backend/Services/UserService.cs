using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace Quizzler_Backend.Controllers.Services
{
    public class UserService
    {
        private readonly QuizzlerDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly GlobalService _globalService;

        public UserService(QuizzlerDbContext context, IConfiguration configuration, GlobalService globalService)
        {
            _context = context;
            _configuration = configuration;
            _globalService = globalService;
        }
        public async Task<bool> EmailExists(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExists(string username)
        {
            return await _context.User.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> AreCredentialsCorrect(UserLoginDto userloginDto)
        {
            var user = await _context.User.Include(u => u.LoginInfo).FirstOrDefaultAsync(u => u.Email == userloginDto.Email) ?? throw new ArgumentNullException(nameof(userloginDto));
            string generatedPassword = _globalService.HashPassword(userloginDto.Password, user.LoginInfo.Salt);
            if (generatedPassword == user.LoginInfo.PasswordHash) return true;
            return false;
        }


        public bool IsEmailCorrect(string email)
        {
            try
            {
                email = new MailAddress(email).Address;
                return true;
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
            var user = new User
            {
                Email = userRegisterDto.Email,
                Username = userRegisterDto.Username,
                FirstName = userRegisterDto.FirstName,
                LastName = userRegisterDto.LastName,
                DateRegistered = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                UserMedia = new List<Media>(),
                Lesson = new List<Lesson>(),
                LoginInfo = new LoginInfo
                {
                    Salt = _globalService.CreateSalt(),
                }

            };

            user.LoginInfo.PasswordHash = _globalService.HashPassword(userRegisterDto.Password, user.LoginInfo.Salt);
            return user;
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["JwtKey"] ?? throw new InvalidOperationException("JwtKey configuration value is null or empty.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            };

            var token = new JwtSecurityToken(_configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: DateTime.Now.AddMinutes(60 * 24 * 7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public static implicit operator UserService(GlobalService v)
        {
            throw new NotImplementedException();
        }
    }
}
