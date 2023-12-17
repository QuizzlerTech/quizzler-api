using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Dtos.Flashcard;
using Quizzler_Backend.Dtos.Lesson;
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return BadRequest("Not logged");
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // GET: api/user/{username}/profile
        // Method to get profile info 
        [HttpGet("{username}/profile")]
        public async Task<ActionResult<User>> GetUserProfileByUsername(string username)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();
            return Ok(user);
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
            var user = await _context.User
                .Include(u => u.Lesson)
                    .ThenInclude(l => l.LessonMedia)
                .Include(u => u.Lesson)
                    .ThenInclude(l => l.Flashcards)
                .Include(u => u.Lesson)
                    .ThenInclude(l => l.Likes)
                .FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound("No user found");

            bool isItLoggedUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id.ToString();
            var result = user.Lesson
                             .Where(l => l.IsPublic || isItLoggedUser)
                             .Select(l => new LessonInfoSendDto
                             {
                                 LessonId = l.LessonId,
                                 Title = l.Title,
                                 Description = l.Description,
                                 ImageName = l.LessonMedia?.Name,
                                 DateCreated = l.DateCreated,
                                 IsPublic = l.IsPublic,
                                 Tags = _context.Entry(l).Collection(l => l.LessonTags).Query().Select(t => t.Tag).Select(t => t.Name).ToList(),
                                 FlashcardCount = _context.Entry(l).Collection(l => l.Flashcards).Query().Count(),
                                 LikesCount = l.Likes.Count,
                                 IsLiked = l.Likes.Any(like => like.UserId == id),
                                 Owner = new UserSendDto
                                 {
                                     UserId = user.UserId,
                                     Username = user.Username,
                                     Avatar = user.Avatar,
                                     FirstName = user.FirstName,
                                     LastName = user.LastName,
                                     LastSeen = user.LastSeen,
                                     LessonCount = user.Lesson.Count
                                 },
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
            var user = await _context.User
                .Include(u => u.Lesson)
                .ThenInclude(f => f.Flashcards)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
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

        [Authorize]
        [HttpGet("lastLesson")]
        public async Task<ActionResult<LessonInfoSendCardDto>> GetLastLesson()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null) return NotFound("User not found");

            var lastLesson = await _context.FlashcardLog
                .AsNoTracking()
                .Where(l => l.UserId == user.UserId)
                .Include(l => l.Flashcard)
                    .ThenInclude(f => f.Lesson)
                        .ThenInclude(l => l.LessonMedia)
                .Include(l => l.Flashcard)
                    .ThenInclude(f => f.Lesson)
                .Include(l => l.Flashcard)
                    .ThenInclude(f => f.Lesson)
                        .ThenInclude(l => l.Likes)
                 .Include(l => l.Flashcard)
                    .ThenInclude(f => f.Lesson).ThenInclude(l => l.Owner)
                .OrderByDescending(fl => fl.Date)
                    .Select(l => l.Flashcard.Lesson)
                .FirstOrDefaultAsync();

            if (lastLesson == null) return NotFound("No lessons found");

            var ownerDto = new UserSendDto
            {
                UserId = lastLesson.Owner.UserId,
                Username = lastLesson.Owner.Username,
                Avatar = lastLesson.Owner.Avatar,
                FirstName = lastLesson.Owner.FirstName,
                LastName = lastLesson.Owner.LastName,
                LastSeen = lastLesson.Owner.LastSeen,
                LessonCount = lastLesson.Owner.Lesson.Count
            };

            var result = new LessonInfoSendCardDto
            {
                LessonId = lastLesson.LessonId,
                Title = lastLesson.Title,
                Description = lastLesson.Description ?? "",
                ImageName = lastLesson.LessonMedia?.Name ?? "",
                FlashcardCount = lastLesson.Flashcards.Count,
                Owner = ownerDto,
                LikesCount = lastLesson.Likes.Count,
                IsLiked = lastLesson.Likes.Any(like => like.UserId == user.UserId)
            };
            return Ok(result);
        }

        [Authorize]
        [HttpGet("likedLessons")]
        public async Task<ActionResult<IEnumerable<LessonInfoSendDto>>> GetLikedLessons()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound("No user found");

            var lessons = await _context.Lesson
                                        .AsNoTracking()
                                        .Include(l => l.LessonMedia)
                                        .Include(l => l.LessonTags).ThenInclude(t => t.Tag)
                                        .Include(l => l.Flashcards)
                                        .Where(l => l.Likes.Any(like => like.UserId == userId) && (l.IsPublic || l.OwnerId == userId))
                                        .Include(l => l.Likes)
                                        .Include(l => l.Owner)
                                        .ToListAsync();


            var result = lessons.Select(l => new LessonInfoSendDto
            {
                LessonId = l.LessonId,
                Title = l.Title,
                Description = l.Description,
                ImageName = l.LessonMedia?.Name,
                DateCreated = l.DateCreated,
                IsPublic = l.IsPublic,
                Tags = l.LessonTags.Select(t => t.Tag.Name).ToList(),
                FlashcardCount = l.Flashcards.Count,
                LikesCount = l.Likes.Count,
                IsLiked = l.Likes.Any(like => like.UserId == userId),
                Owner = new UserSendDto
                {
                    UserId = l.Owner.UserId,
                    Username = l.Owner.Username,
                    Avatar = l.Owner.Avatar,
                    FirstName = l.Owner.FirstName,
                    LastName = l.Owner.LastName,
                    LastSeen = l.Owner.LastSeen,
                    LessonCount = l.Owner.Lesson.Count
                }
            })
            .ToList();

            return Ok(result);
        }
        // GET: api/user/lastWeekActivity
        // Method to get list of days when user was learning (had flashcardsLogs)
        [Authorize]
        [HttpGet("lastWeekActivity")]
        public async Task<ActionResult<List<DateTime>>> GetLastWeekActivity()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User
                .Include(u => u.FlashcardLog)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null) return NotFound("User not found");
            var flashcardLogs = user.FlashcardLog
                .Select(l => l.Date.Date)
                .Distinct()
                .Where(d => d >= DateTime.UtcNow.AddDays(-6))
                .ToList();
            return Ok(flashcardLogs);
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

            return new CreatedAtActionResult(nameof(GetUserProfileByUsername), "User", new { username = user.Username }, "Created user");
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