using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;
using System.Security.Claims;

namespace Quizzler_Backend.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly QuizzlerDbContext _context;
        private readonly UserService _userService;
        private readonly GlobalService _globalService;

        // Controller constructor
        public UserController(QuizzlerDbContext context, UserService userService, GlobalService globalService)
        {
            _context = context;
            _userService = userService;
            _globalService = globalService;
        }

        // GET: api/profile
        // Method to get logged user profile info 
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<User>> GetMyProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return await GetUserProfileById(Convert.ToInt32(userId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/user/{id}/profile
        // Method to get profile info 
        [Authorize]
        [HttpGet("{id}/profile")]
        public async Task<ActionResult<User>> GetUserProfileById(int id)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound("No user found");
            var result = new User
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                LastSeen = user.LastSeen,
                DateRegistered = user.DateRegistered,
                Avatar = user.Avatar,
            };
            return Ok(result);
        }

        // PATCH: api/user/update
        // Method to get profile info 
        [Authorize]
        [HttpPatch("update")]
        public async Task<ActionResult<User>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (!await _userService.AreCredentialsCorrect(new UserLoginDto { Email = user.Email, Password = userUpdateDto.CurrentPassword })) return StatusCode(400, $"Wrong credentials");

            // Checks if the email or username already exists
            if (userUpdateDto.Email is not null)
            {
                if (await _userService.EmailExists(userUpdateDto.Email) && !(userUpdateDto.Email == user.Email)) return StatusCode(409, $"Email {userUpdateDto.Email} already registered");
            }
            if (userUpdateDto.Username is not null)
            {
                if (await _userService.UsernameExists(userUpdateDto.Username) && !(userUpdateDto.Username == user.Username)) return StatusCode(409, $"Username {userUpdateDto.Username} already registered");
            }
            // Checks if the email is correct
            if (userUpdateDto.Email is not null)
            {
                if (!_userService.IsEmailCorrect(userUpdateDto.Email)) return StatusCode(422, $"Email {userUpdateDto.Email} is not a proper email address");
            }
            // Checks if the password meets the criteria
            if (userUpdateDto.Password is not null)
            {
                if (!_userService.IsPasswordGoodEnough(userUpdateDto.Password)) return StatusCode(422, $"Password does not meet the requirements");
            }

            user.Username = userUpdateDto.Username ?? user.Username;
            user.Email = userUpdateDto.Email ?? user.Email;
            user.FirstName = userUpdateDto.FirstName ?? user.FirstName;
            user.LastName = userUpdateDto.LastName ?? user.LastName;
            user.LoginInfo.PasswordHash = userUpdateDto.Password != null ? _globalService.HashPassword(userUpdateDto.Password, user.LoginInfo.Salt) : user.LoginInfo.PasswordHash;

            await _context.SaveChangesAsync();

            return Ok("Updated");
        }
        // PATCH: api/user/update
        // Method to get profile info 
        [Authorize]
        [HttpPatch("updateAvatar")]
        public async Task<ActionResult<User>> UpdateUserAvatar(UserUpdateAvatarDto userUpdateAvatarDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            user.Avatar = userUpdateAvatarDto.Avatar;

            await _context.SaveChangesAsync();

            return Ok("Avatar updated");
        }
        // GET: api/user/check
        // Method to validate JSON Web Token
        [Authorize]
        [HttpGet("check")]
        public async Task<ActionResult<User>> CheckAuth()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok("You are authorized!");
        }

        // POST: api/user/register
        // Method to register a new user
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto userRegisterDto)
        {
            // Checks if the email or username already exists
            if (await _userService.EmailExists(userRegisterDto.Email)) return StatusCode(409, $"Email {userRegisterDto.Email} already registered");
            if (await _userService.UsernameExists(userRegisterDto.Username)) return StatusCode(409, $"Username {userRegisterDto.Username} already registered");
            // Checks if the email is correct
            if (!_userService.IsEmailCorrect(userRegisterDto.Email)) return StatusCode(422, $"Email {userRegisterDto.Email} is not a proper email address");
            // Checks if the password meets the criteria
            if (!_userService.IsPasswordGoodEnough(userRegisterDto.Password)) return StatusCode(422, $"Password does not meet the requirements");

            var user = await _userService.CreateUser(userRegisterDto);
            _context.User.Add(user);

            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(nameof(GetUserProfileById), "User", new { id = user.UserId }, "Created user");
        }

        // POST: api/user/login
        // Method for user login
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(UserLoginDto userLoginDto)
        {
            // Checks if the user exists
            if (!await _userService.EmailExists(userLoginDto.Email)) return StatusCode(409, $"{userLoginDto.Email} is not registered");
            // Checks if the login credentials are incorrect
            if (!await _userService.AreCredentialsCorrect(userLoginDto)) return StatusCode(400, $"Wrong credentials");
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);
            var token = _userService.GenerateJwtToken(user);
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(token);

        }
        // DELETE: api/user/delete
        // Method to delete a user
        [Authorize]
        [HttpDelete("delete")]
        public async Task<ActionResult<User>> Delete(string userPassword)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            try
            {
                if (!await _userService.AreCredentialsCorrect(new UserLoginDto { Email = user.Email, Password = userPassword })) return StatusCode(403, "Invalid credentials");
                // Removes user from the database and save changes
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
                return Ok("User deleted successfully.");

            }
            catch (Exception ex)
            {
                return NotFound("No user found");
            }
        }
    }

}