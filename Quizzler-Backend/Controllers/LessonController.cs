using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using System.Security.Claims;
using Quizzler_Backend.Filters;
using Microsoft.Extensions.Logging;

namespace Quizzler_Backend.Controllers
{
    [Route("api/lesson")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly QuizzlerDbContext _context;
        private readonly LessonService _lessonService;
        private readonly UserService _userService;
        private readonly ILogger<LessonController> _logger;

        public LessonController(QuizzlerDbContext context, LessonService lessonService, UserService userService, ILogger<LessonController> logger)
        {
            _context = context;
            _lessonService = lessonService;
            _userService = userService;
            _logger = logger;
        }

        // GET: api/lesson/{id}
        // Method to get lesson by id
        [HttpGet("{id}")]
        [AllowPublicLessonFilter]
        public async Task<ActionResult<Lesson>> GetLessonById(int id)
        {
            var lesson = await _context.Lesson.FirstOrDefaultAsync(u => u.LessonId == id);
            return Ok(lesson);
        }

        // POST: api/lesson/add
        // Method to create new lesson
        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult<Lesson>> AddNewLesson(LessonAddDto lessonAddDto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.User.Include(u => u.Lesson).FirstOrDefaultAsync(u => u.UserId == userId);

            if (_lessonService.TitleExists(lessonAddDto.Title, user)) return BadRequest("User already has this lesson");
            if (!_lessonService.IsTitleCorrect(lessonAddDto.Title)) return BadRequest("Wrong title");
            if (!_lessonService.IsDescriptionCorrect(lessonAddDto.Description)) return BadRequest("Wrong description");

            var lesson = await _lessonService.CreateLesson(lessonAddDto, userId, user);
            _context.Lesson.Add(lesson);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(nameof(GetLessonById), "Lesson", new { id = lesson.LessonId }, "Created lesson");
        }


    }
}
