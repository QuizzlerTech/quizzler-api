using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Dtos.Flashcard;
using Quizzler_Backend.Dtos.Lesson;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;

namespace Quizzler_Backend.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            return await _userService.GetMyProfileAsync(User);
        }

        [HttpGet("{username}/profile")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfileByUsername(string username)
        {
            return await _userService.GetUserProfileByUsernameAsync(username);
        }

        [Authorize]
        [HttpGet("check")]
        public async Task<IActionResult> CheckAuth()
        {
            return await _userService.CheckAuthAsync(User);
        }

        [Authorize]
        [HttpGet("lessons")]
        public async Task<ActionResult<IEnumerable<LessonInfoSendDto>>> GetMyLessons()
        {
            return await _userService.GetMyLessonsAsync(User);
        }

        [HttpGet("{id}/lessons")]
        public async Task<ActionResult<IEnumerable<LessonInfoSendDto>>> GetUserLessonsById(int id)
        {
            return await _userService.GetUserLessonsByIdAsync(id);
        }

        [Authorize]
        [HttpGet("flashcardsCreated")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetUserFlashcardsCreationDates()
        {
            return await _userService.GetUserFlashcardsCreationDatesAsync(User);
        }

        [Authorize]
        [HttpGet("logs")]
        public async Task<ActionResult<IEnumerable<FlashcardLogSendDto>>> GetUserLogs()
        {
            return await _userService.GetUserLogsAsync(User);
        }

        [Authorize]
        [HttpGet("lastLesson")]
        public async Task<ActionResult<LessonInfoSendCardDto>> GetLastLessonInfo()
        {
            return await _userService.GetLastLessonInfoAsync(User);
        }

        [Authorize]
        [HttpGet("likedLessons")]
        public async Task<ActionResult<IEnumerable<LessonInfoSendDto>>> GetLikedLessons()
        {
            return await _userService.GetLikedLessonsAsync(User);
        }

        [Authorize]
        [HttpGet("lastWeekActivity")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetLastWeekActivity()
        {
            return await _userService.GetLastWeekActivityAsync(User);
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

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto userRegisterDto)
        {
            return await _userService.RegisterAsync(userRegisterDto);
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(UserLoginDto userLoginDto)
        {
            return await _userService.LoginAsync(userLoginDto);
        }

        [Authorize]
        [HttpPatch("update")]
        public async Task<ActionResult<User>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            return await _userService.UpdateUserAsync(User, userUpdateDto);
        }

        [Authorize]
        [HttpPatch("updateAvatar")]
        public async Task<ActionResult<User>> UpdateUserAvatar(UserUpdateAvatarDto userUpdateAvatarDto)
        {
            return await _userService.UpdateUserAvatarAsync(User, userUpdateAvatarDto);
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<ActionResult<User>> Delete(string userPassword)
        {
            return await _userService.DeleteUserAsync(User, userPassword);
        }
    }
}
