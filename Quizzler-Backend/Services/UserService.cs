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
        public async Task<bool> AreLoginCredentialsCorrect(LoginDto loginDto)
        {
            if (IsEmailCorrect(loginDto.UsernameOrEmail)) return await AreCredentialsCorrectByEmail(loginDto.UsernameOrEmail, loginDto.Password);
            return await AreCredentialsCorrectByUsername(loginDto.UsernameOrEmail, loginDto.Password);

        }
        public async Task<bool> AreCredentialsCorrectByEmail(string email, string password)
        {
            var loginInfo = await GetLoginInfoByEmail(email);
            string generatedPassword = HashPassword(password, loginInfo.Salt);
            if (generatedPassword == loginInfo.PasswordHash) return true;
            return false;

        }
        public async Task<bool> AreCredentialsCorrectByUsername(string username, string password)
        {
            var loginInfo = await GetLoginInfoByUsername(username);
            string generatedPassword = HashPassword(password, loginInfo.Salt);
            if (generatedPassword == loginInfo.PasswordHash) return true;
            return false;

        }

        // Get login information by email
        private async Task<LoginInfo> GetLoginInfoByEmail(string email)
        {
            var user = await _context.User
                                     .Include(u => u.LoginInfo)
                                     .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Handle case where user is not found
                throw new Exception("User not found");
            }

            return user.LoginInfo;
        }

        // Get login information by username
        private async Task<LoginInfo> GetLoginInfoByUsername(string username)
        {
            var user = await _context.User
                                     .Include(u => u.LoginInfo)
                                     .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            return user.LoginInfo;
        }

        // Validate email format
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


        // Register a new user
        public async Task<User> CreateUser(UserRegisterDto userDto)
        {
            var user = new User
            {
                Email = userDto.Email,
                Username = userDto.Username,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                DateRegistered = DateTime.UtcNow,
                Media = new List<Media>(),
                Lesson = new List<Lesson>(),
                LoginInfo = new LoginInfo
                {
                    Salt = CreateSalt(),
                }
            };

            user.LoginInfo.PasswordHash = HashPassword(userDto.Password, user.LoginInfo.Salt);
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
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(60 * 24 * 7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
