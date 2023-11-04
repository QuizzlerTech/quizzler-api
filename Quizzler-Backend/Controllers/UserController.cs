using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Data;
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

        // GET: api/user/check
        // Method to validate JSON Web Token
        [Authorize]
        [HttpGet("check")]
        public async Task<ActionResult<User>> CheckAuth()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null) return NotFound("User doesnt exist");
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok("You are authorized!");
        }

        // GET: api/lessons
        // Method to get logged user lessons info 
        [Authorize]
        [HttpGet("lessons")]
        public async Task<ActionResult<ICollection<LessonInfoSendDto>>> GetMyLessons()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return await GetUserLessonsById(Convert.ToInt32(userId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET: api/user/{id}/lessons
        // Method to get lessons info 
        [HttpGet("{id}/lessons")]
        public async Task<ActionResult<ICollection<LessonInfoSendDto>>> GetUserLessonsById(int id)
        {
            var user = await _context.User.Include(u => u.Lesson).FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound("No user found");

            bool isItLoggedUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id.ToString();
            var result = user.Lesson
                             .Where(l => l.IsPublic || isItLoggedUser)
                             .Select(l => new LessonInfoSendDto
                             {
                                 LessonId = l.LessonId,
                                 Title = l.Title,
                                 Description = l.Description,
                                 ImagePath = l.LessonMedia?.Name,
                                 DateCreated = l.DateCreated,
                                 IsPublic = l.IsPublic,
                                 Tags = l.LessonTags.Select(l => l.Tag.Name).ToList(),
                                 FlashcardCount = l.Flashcards.Count,
                             })
                             .ToList();

            return Ok(result.Count == 0 ? new List<LessonInfoSendDto>() : result);
        }

        // GET: api/user/flashcardsCreated
        // Method to get flashcard creation info
        [Authorize]
        [HttpGet("flashcardsCreated")]
        public async Task<ActionResult<List<DateTime>>> GetUserFlashcardsCreationDates()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            var flashcardDates = user!.Lesson.SelectMany(f => f.Flashcards).Select(d => d.DateCreated).ToList();
            return flashcardDates;
        }

        // GET: api/user/logs
        // Method to get flashcard logs for stats
        [Authorize]
        [HttpGet("logs")]
        public async Task<ActionResult<List<FlashcardLogSendDto>>> GetUserLogs()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            var flashcardLogs = await _context.User
                .Where(u => u.UserId.ToString() == userId)
                .SelectMany(u => u.FlashcardLog)
                .Select(log => new FlashcardLogSendDto
                {
                    Date = log.Date,
                    FlashcardId = log.FlashcardId,
                    WasCorrect = log.WasCorrect
                })
                .ToListAsync();

            return flashcardLogs;
        }



        // POST: api/user/register
        // Method to register a new user
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto userRegisterDto)
        {
            // Checks if the email or username already exists
            if (await _userService.EmailExists(userRegisterDto.Email)) return Conflict($"Email already registered");
            if (await _userService.UsernameExists(userRegisterDto.Username)) return Conflict("Username already registered");
            // Checks if the email is correct
            if (!_userService.IsEmailCorrect(userRegisterDto.Email)) return BadRequest($"Given email is not a proper email address");
            // Checks if the password meets the criteria
            if (!_userService.IsPasswordGoodEnough(userRegisterDto.Password)) return BadRequest("The password must be at least 8 characters long");

            var user = _userService.CreateUser(userRegisterDto);
            _context.User.Add(user);

            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(nameof(GetUserProfileById), "User", new { id = user.UserId }, "Created user");
        }

        // POST: api/user/login
        // Method for user login
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(UserLoginDto userLoginDto)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);
            if (user == null) return Unauthorized("Invalid credentials");
            if (!await _userService.AreCredentialsCorrect(userLoginDto)) return Unauthorized("Invalid credentials");
            var token = _userService.GenerateJwtToken(user);
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(token);
        }

        // PATCH: api/user/update
        // Method to get profile info 
        [Authorize]
        [HttpPatch("update")]
        public async Task<ActionResult<User>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null) return NotFound("User not found.");

            if (!await _userService.AreCredentialsCorrect(new UserLoginDto { Email = user.Email, Password = userUpdateDto.CurrentPassword }))
                return Unauthorized("Invalid credentials");

            // Email checks
            if (string.IsNullOrWhiteSpace(userUpdateDto.Email))
                return BadRequest("Email cannot be empty.");

            if (userUpdateDto.Email != null && userUpdateDto.Email != user.Email && await _userService.EmailExists(userUpdateDto.Email))
                return Conflict($"Email {userUpdateDto.Email} already registered");

            if (userUpdateDto.Email != null && !_userService.IsEmailCorrect(userUpdateDto.Email))
                return BadRequest($"Email {userUpdateDto.Email} is not a proper email address");

            // Username check
            if (userUpdateDto.Username != null && userUpdateDto.Username != user.Username && await _userService.UsernameExists(userUpdateDto.Username))
                return Conflict($"Username {userUpdateDto.Username} already registered");

            // Password check
            if (userUpdateDto.Password != null && !_userService.IsPasswordGoodEnough(userUpdateDto.Password))
                return BadRequest("The password must be at least 8 characters long.");

            // Updating user details
            user.Username = userUpdateDto.Username ?? user.Username;
            user.Email = userUpdateDto.Email ?? user.Email;
            user.FirstName = userUpdateDto.FirstName ?? user.FirstName;
            user.LastName = userUpdateDto.LastName ?? user.LastName;
            user.LoginInfo.PasswordHash = userUpdateDto.Password != null ? _globalService.HashPassword(userUpdateDto.Password, user.LoginInfo.Salt) : user.LoginInfo.PasswordHash;

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // PATCH: api/user/update
        // Method to get profile info 
        [Authorize]
        [HttpPatch("updateAvatar")]
        public async Task<ActionResult<User>> UpdateUserAvatar(UserUpdateAvatarDto userUpdateAvatarDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            user!.Avatar = userUpdateAvatarDto.Avatar;
            await _context.SaveChangesAsync();
            return Ok("Avatar updated");
        }

        // DELETE: api/user/delete
        // Method to delete a user
        [Authorize]
        [HttpDelete("delete")]
        public async Task<ActionResult<User>> Delete(string userPassword)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null) return NotFound("User not found");
            if (!await _userService.AreCredentialsCorrect(new UserLoginDto { Email = user!.Email, Password = userPassword })) return StatusCode(403, "Invalid credentials");
            // Removes user from the database and save changes
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("User deleted successfully.");
        }
    }

}