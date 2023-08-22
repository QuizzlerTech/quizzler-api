using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using MlkPwgen;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using Microsoft.AspNetCore.DataProtection;
using Isopoh.Cryptography.SecureArray;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Quizzler_Backend.Services;

namespace Quizzler_Backend.Controllers.Services
{
    public class UserService
    {
        // Field variables
        private readonly QuizzlerDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly GlobalService _globalService;

        // Constructor
        public UserService(QuizzlerDbContext context, IConfiguration configuration, GlobalService globalService)
        {
            _context = context;
            _configuration = configuration;
            _globalService = globalService;
        }

        // Check if email exists in the database
        public async Task<bool> EmailExists(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }

        // Check if username exists in the database
        public async Task<bool> UsernameExists(string username)
        {
            return await _context.User.AnyAsync(u => u.Username == username);
        }

        // Check if user exists (by username or email)
        public async Task<bool> DoesExist(string email)
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

        // Validate if entered login credentials are correct
        public async Task<bool> AreCredentialsCorrect(UserLoginDto userloginDto)
        {
            var user = await _context.User.Include(u => u.LoginInfo).FirstOrDefaultAsync(u => u.Email == userloginDto.Email);
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

        // Validate password complexity
        public bool IsPasswordGoodEnough(string password)
        {
            return password.Length >= 8;
        }


        // Create a new user
        public async Task<User> CreateUser(UserRegisterDto userRegisterDto)
        {
            var user = new User
            {
                Email = userRegisterDto.Email,
                Username = userRegisterDto.Username,
                FirstName = userRegisterDto.FirstName,
                LastName = userRegisterDto.LastName,
                DateRegistered = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                Media = new List<Media>(),
                Lesson = new List<Lesson>(),
                LoginInfo = new LoginInfo
                {
                    Salt = _globalService.CreateSalt(),
                }

            };

            user.LoginInfo.PasswordHash = _globalService.HashPassword(userRegisterDto.Password, user.LoginInfo.Salt);
            return user;
        }

        // Generate a JWT token for a user
        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
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
