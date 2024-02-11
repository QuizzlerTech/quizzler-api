using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizzler_Backend.Dtos.Flashcard;
using Quizzler_Backend.Dtos.Lesson;
using Quizzler_Backend.Services.UserServices;

namespace Quizzler_Backend.Controllers.UserController
{
    [Route("api/user/activity")]
    [ApiController]
    public class UserActivityController(UserActivityService userActivityService) : ControllerBase
    {
        private readonly UserActivityService _userActivityService = userActivityService;



        [Authorize]
        [HttpGet("flashcardsCreated")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetUserFlashcardsCreationDates()
        {
            return await _userActivityService.GetUserFlashcardsCreationDatesAsync(User);
        }

        [Authorize]
        [HttpGet("logs")]
        public async Task<ActionResult<IEnumerable<FlashcardLogSendDto>>> GetUserLogs()
        {
            return await _userActivityService.GetUserLogsAsync(User);
        }

        [Authorize]
        [HttpGet("lastLesson")]
        public async Task<ActionResult<LessonInfoSendCardDto>> GetLastLessonInfo()
        {
            return await _userActivityService.GetLastLessonInfoAsync(User);
        }

        [Authorize]
        [HttpGet("likedLessons")]
        public async Task<ActionResult<IEnumerable<LessonInfoSendDto>>> GetLikedLessons()
        {
            return await _userActivityService.GetLikedLessonsAsync(User);
        }

        [Authorize]
        [HttpGet("lastWeekActivity")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetLastWeekActivity()
        {
            return await _userActivityService.GetLastWeekActivityAsync(User);
        }
    }
}
