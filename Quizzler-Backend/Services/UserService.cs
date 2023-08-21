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

namespace Quizzler_Backend.Controllers.Services
{
    public class UserService
    {
        // Field variables
        private readonly QuizzlerDbContext _context;
        private readonly IConfiguration _configuration;

        // Constructor
        public UserService(QuizzlerDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
        public async Task<bool> DoesExist(string usernameOrEmail)
        {
            try
            {
                usernameOrEmail = new MailAddress(usernameOrEmail).Address;
                return await _context.User.AnyAsync(u => u.Email == usernameOrEmail);
            }
            catch (FormatException)
            {
                return await _context.User.AnyAsync(u => u.Username == usernameOrEmail);
            }
        }

        // Validate if entered login credentials are correct
        public async Task<bool> AreCredentialsCorrect(UserLoginDto loginDto)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            string generatedPassword = HashPassword(loginDto.Password, user.LoginInfo.Salt);
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
                    Salt = CreateSalt(),
                }

            };

            user.LoginInfo.PasswordHash = HashPassword(userRegisterDto.Password, user.LoginInfo.Salt);
            return user;
        }

        // Create a new salt for password hashing
        public string CreateSalt()
        {
            return PasswordGenerator.Generate(length: 16, allowed: Sets.Alphanumerics); // used MlkPwgen
        }

        // Hash a password using Argon2
        public string HashPassword(string password, string salt)
        {
            var config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Version = Argon2Version.Nineteen,
                MemoryCost = 32768,
                Threads = Environment.ProcessorCount,
                Password = Encoding.UTF8.GetBytes(password),
                Salt = Encoding.UTF8.GetBytes(salt),
                HashLength = 60
            };

            var argon2 = new Argon2(config);

            using (SecureArray<byte> hash = argon2.Hash())
            {
                return Convert.ToBase64String(hash.Buffer); // used Isopoh.Cryptography.Argon2
            }
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

    }
}
