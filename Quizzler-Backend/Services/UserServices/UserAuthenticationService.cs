using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quizzler_Backend.Controllers.UserController;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos.User;
using Quizzler_Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Quizzler_Backend.Services.UserServices
{
    public class UserAuthenticationService(QuizzlerDbContext context, IConfiguration configuration, GlobalService globalService, UserService userUtility)
    {
        private readonly QuizzlerDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly GlobalService _globalService = globalService;
        private readonly UserService _userUtility = userUtility;

        // UserAuthenticationService
        public async Task<ActionResult<User>> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            if (await EmailExists(userRegisterDto.Email))
            {
                return new ConflictObjectResult($"Email {userRegisterDto.Email} already registered");
            }

            if (await UsernameExists(userRegisterDto.Username))
            {
                return new ConflictObjectResult($"Username {userRegisterDto.Username} already registered");
            }

            if (!_userUtility.IsEmailCorrect(userRegisterDto.Email))
            {
                return new BadRequestObjectResult($"Email {userRegisterDto.Email} is not a proper email address");
            }

            if (!_userUtility.IsPasswordGoodEnough(userRegisterDto.Password))
            {
                return new BadRequestObjectResult("Password does not meet the required criteria");
            }

            var newUser = _userUtility.CreateUser(userRegisterDto);
            await _context.User.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(nameof(UserProfileController.GetUserProfileByUsername), "User", new { username = newUser.Username }, newUser);
        }
        public async Task<ActionResult<User>> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);
            if (user == null || !await AreCredentialsCorrect(userLoginDto))
            {
                return new UnauthorizedObjectResult("Invalid credentials");
            }
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return new OkObjectResult(token);
        }
        public async Task<IActionResult> CheckAuthAsync(ClaimsPrincipal user)
        {
            if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return new NotFoundObjectResult("User does not exist");
            }

            var userEntity = await _context.User.FindAsync(userId);
            if (userEntity == null)
            {
                return new NotFoundObjectResult("User does not exist");
            }

            userEntity.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new OkObjectResult("You are authorized!");
        }
        private string GenerateJwtToken(User user)
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
        private async Task<bool> AreCredentialsCorrect(UserLoginDto userLoginDto)
        {
            var user = await _context.User.Include(u => u.LoginInfo)
                .FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);

            if (user == null)
                return false;

            string generatedPasswordHash = _globalService.HashPassword(userLoginDto.Password, user.LoginInfo.Salt);
            return generatedPasswordHash == user.LoginInfo.PasswordHash;
        }
        private async Task<bool> EmailExists(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }
        private async Task<bool> UsernameExists(string username)
        {
            return await _context.User.AnyAsync(u => u.Username == username);
        }
    }
}