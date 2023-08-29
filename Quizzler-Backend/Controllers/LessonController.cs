using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using System.Security.Claims;
using Quizzler_Backend.Filters;
using Quizzler_Backend.Services;

namespace Quizzler_Backend.Controllers
{
    [Route("api/lesson")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly QuizzlerDbContext _context;
        private readonly LessonService _lessonService;
        private readonly UserService _userService;
        private readonly GlobalService _globalService;
        private readonly ILogger<LessonController> _logger;

        public LessonController(QuizzlerDbContext context, LessonService lessonService, UserService userService, GlobalService globalService, ILogger<LessonController> logger)
        {
            _context = context;
            _lessonService = lessonService;
            _userService = userService;
            _logger = logger;
            _globalService = globalService;
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
        public async Task<ActionResult<Lesson>> AddNewLesson([FromForm] LessonAddDto lessonAddDto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.User.Include(u => u.Lesson).FirstOrDefaultAsync(u => u.UserId == userId);

            if (_lessonService.TitleExists(lessonAddDto.Title, user)) return BadRequest("User already has this lesson");
            if (!_lessonService.IsTitleCorrect(lessonAddDto.Title)) return BadRequest("Wrong title");
            if (!_lessonService.IsDescriptionCorrect(lessonAddDto.Description)) return BadRequest("Wrong description");
            
            var lesson = await _lessonService.CreateLesson(lessonAddDto, userId, user);
  
            if (lessonAddDto.Image is not null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await lessonAddDto.Image.CopyToAsync(memoryStream);
                    var newMedia = await _globalService.SaveImage(lessonAddDto.Image, _lessonService.GenerateImageName(lessonAddDto.Title), userId);
                    if (newMedia == null) return StatusCode(500, "Error saving image");
                    _context.Media.Add(newMedia);
                    lesson.Media = newMedia;
                }
            }

            _context.Lesson.Add(lesson);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(nameof(GetLessonById), "Lesson", new { id = lesson.LessonId }, $"Created lesson {lesson.LessonId}");
        }


        // POST: api/lesson/update
        // Method to update a lesson
        [Authorize]
        [HttpPatch("update")]
        public async Task<ActionResult<Lesson>> UpdateLesson([FromForm] LessonUpdateDto lessonUpdateDto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.User.Include(u => u.Lesson).FirstOrDefaultAsync(u => u.UserId == userId);
            var lesson = await _context.Lesson.FirstOrDefaultAsync(u => u.LessonId == lessonUpdateDto.LessonId);
            var imageMediaType = await _context.MediaType.FirstOrDefaultAsync(u => u.TypeName == "Image");

            if (!(userId == lesson.OwnerId)) return Unauthorized("User is not the owner");
            if (lessonUpdateDto.Title is not null)
            {
                if (_lessonService.TitleExists(lessonUpdateDto.Title, user)) return BadRequest("User already has this lesson");
                if (!_lessonService.IsTitleCorrect(lessonUpdateDto.Title)) return BadRequest("Wrong title");
                lesson.Title = lessonUpdateDto.Title;
            }
            if (lessonUpdateDto.Description is not null)
            {
                if (!_lessonService.IsDescriptionCorrect(lessonUpdateDto.Description)) return BadRequest("Wrong description");
                lesson.Description = lessonUpdateDto.Description;
            }
            lesson.IsPublic = lessonUpdateDto.IsPublic ?? lesson.IsPublic;

            if (lessonUpdateDto.Image is not null)
            {
      
                if (lessonUpdateDto.Image.Length > imageMediaType.MaxSize) return BadRequest("The image is too large"); 
                using (var memoryStream = new MemoryStream())
                {
                    await lessonUpdateDto.Image.CopyToAsync(memoryStream);
                    var newMedia = await _globalService.SaveImage(lessonUpdateDto.Image, _lessonService.GenerateImageName(lesson.Title), userId);
                    if (newMedia == null) return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    lesson.Media = newMedia;
                    _context.Media.Add(newMedia);
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Lesson updated");
        }
        // DELETE: api/lesson/delete
        // Method to delete a lesson
        [Authorize]
        [HttpDelete("delete")]
        public async Task<ActionResult<Lesson>> Delete(string lessonId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var lesson = await _context.Lesson.FirstOrDefaultAsync(u => u.LessonId.ToString() == lessonId) ;
            try
            {
                if (!_globalService.isItUssersLesson(userId, lesson)) return Unauthorized("Not user's lesson");
                // Removes lesson from the database and save changes
                _context.Lesson.Remove(lesson);
                await _context.SaveChangesAsync();
                return Ok("Lesson deleted successfully.");

            }
            catch (Exception ex)
            {
                return NotFound("No lesson found");
            }
        }
    }
}
