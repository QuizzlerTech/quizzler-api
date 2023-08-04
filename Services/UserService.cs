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

namespace Quizzler_Backend.Controllers.Services
{
    public class UserService
    {
        private readonly QuizzlerDbContext _context;
        private readonly IConfiguration _configuration;
        public UserService(QuizzlerDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }
        public async Task<bool> UsernameExists(string username)
        {
            return await _context.User.AnyAsync(u => u.Username == username);
        }
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
        public async Task<bool> AreLoginCredentialCorrect(LoginDto loginDto)
        {
            LoginInfo loginInfo;
            if (IsEmailCorrect(loginDto.UsernameOrEmail))
            {
                loginInfo = await GetLoginInfoByEmail(loginDto.UsernameOrEmail);
            }
            else
            {
                loginInfo = await GetLoginInfoByUsername(loginDto.UsernameOrEmail);
            }
            string generatedPassword = HashPassword(loginDto.Password, loginInfo.Salt);

            if (generatedPassword == loginInfo.PasswordHash)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
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
            if (password.Length >= 8)
            {
                return true;
            }
            return false;
        }
        public async Task<User> RegisterUser(UserRegisterDto userDto)
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

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
        public string CreateSalt()
        {
            return PasswordGenerator.Generate(length: 16, allowed: Sets.Alphanumerics); // used MlkPwgen
        }
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
            var argon2A = new Argon2(config);

            using (SecureArray<byte> hash = argon2A.Hash())
            {
                return Convert.ToBase64String(hash.Buffer); // used Isopoh.Cryptography.Argon2
            }
        }
        public string GenerateJwtToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                null,
                expires: DateTime.Now.AddMinutes(60 * 24 * 7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
