using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Filters;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;
using System.Security.Claims;

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
        public async Task<ActionResult<LessonSendDto>> GetLessonById(int id)
        {
            var lesson = await _context.Lesson
                                       .Include(l => l.LessonMedia)
                                       .Include(l => l.Flashcards)
                                            .ThenInclude(f => f.AnswerMedia)
                                       .Include(l => l.Flashcards)
                                            .ThenInclude(f => f.QuestionMedia)
                                       .FirstOrDefaultAsync(u => u.LessonId == id);

            if (lesson == null) return NotFound();
            // Populate LessonSendDto
            var lessonSendDto = new LessonSendDto
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                ImageName = lesson.LessonMedia?.Name,
                DateCreated = lesson.DateCreated,
                IsPublic = lesson.IsPublic,
                Tags = lesson.LessonTags.Select(l => l.Tag.Name).ToList(),
                Flashcards = lesson.Flashcards.Select(f => new FlashcardSendDto
                {
                    FlashcardId = f.FlashcardId,
                    DateCreated = f.DateCreated,
                    QuestionText = f.QuestionText,
                    AnswerText = f.AnswerText,
                    QuestionImageName = f.QuestionMedia?.Name,
                    AnswerImageName = f.AnswerMedia?.Name
                }).ToList()
            };

            return Ok(lessonSendDto);
        }

        // POST: api/lesson/add
        // Method to create new lesson

        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult<Lesson>> AddNewLesson([FromForm] LessonAddDto lessonAddDto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.User.Include(u => u.Lesson).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return Unauthorized("No user found");

            if (_lessonService.TitleExists(lessonAddDto.Title, user)) return BadRequest("User already has this lesson");
            if (!_lessonService.IsTitleCorrect(lessonAddDto.Title)) return BadRequest("Wrong title");
            if (!_lessonService.IsDescriptionCorrect(lessonAddDto.Description!)) return BadRequest("Wrong description");

            var lesson = _lessonService.CreateLesson(lessonAddDto, userId, user);

            if (lessonAddDto.Image != null)
            {
                if (!await _globalService.IsImageRightSize(lessonAddDto.Image)) return BadRequest("The image size is too large");
                using var memoryStream = new MemoryStream();
                await lessonAddDto.Image.CopyToAsync(memoryStream);
                var newMedia = await _globalService.SaveImage(lessonAddDto.Image, _lessonService.GenerateImageName(lessonAddDto.Title), userId);
                if (newMedia == null) return StatusCode(500, "Error saving image");
                _context.Media.Add(newMedia);
                lesson.LessonMedia = newMedia;
            }

            if (lessonAddDto.TagNames != null)
            {
                foreach (var tagName in lessonAddDto.TagNames)
                {
                    await _lessonService.AddLessonTag(tagName, lesson);
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

            if (user == null) return Unauthorized("Unauthorized");

            if (lesson == null) return BadRequest("Invalid lesson ID");

            if (!(userId == lesson.OwnerId)) return Unauthorized("User is not the owner");
            if (lessonUpdateDto.Title != null)
            {
                if (_lessonService.TitleExists(lessonUpdateDto.Title, user)) return BadRequest("User already has this lesson");
                if (!_lessonService.IsTitleCorrect(lessonUpdateDto.Title)) return BadRequest("Wrong title");
                lesson.Title = lessonUpdateDto.Title;
            }
            if (lessonUpdateDto.Description != null)
            {
                if (!_lessonService.IsDescriptionCorrect(lessonUpdateDto.Description)) return BadRequest("Wrong description");
                lesson.Description = lessonUpdateDto.Description;
            }
            lesson.IsPublic = lessonUpdateDto.IsPublic ?? lesson.IsPublic;

            if (lessonUpdateDto.Image != null)
            {
                if (!await _globalService.IsImageRightSize(lessonUpdateDto.Image)) return BadRequest("The image size is too large");
                using var memoryStream = new MemoryStream();
                await lessonUpdateDto.Image.CopyToAsync(memoryStream);
                var newMedia = await _globalService.SaveImage(lessonUpdateDto.Image, _lessonService.GenerateImageName(lesson.Title), userId);
                if (newMedia == null) return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                if (lesson.LessonMedia != null)
                {
                    await _globalService.DeleteImage(lesson.LessonMedia.Name);
                }
                lesson.LessonMedia = newMedia;
                _context.Media.Add(newMedia);
            }
            if (lessonUpdateDto.TagNames != null)
            {
                lesson.LessonTags.Clear();
                foreach (var tagName in lessonUpdateDto.TagNames)
                {
                    if (tagName == null) continue;
                    await _lessonService.AddLessonTag(tagName, lesson);
                }
            }
            var tagsForDeletion = _context.Tag
                .Where(t => t.LessonTags.Count == 0);
            foreach (var tag in tagsForDeletion)
            {
                _context.Remove(tag);
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
            var lesson = await _context.Lesson.FirstOrDefaultAsync(u => u.LessonId.ToString() == lessonId);
            if (lesson == null) return NotFound("No lesson found");
            if (!_globalService.IsUsersLesson(userId!, lesson)) return Unauthorized("Not user's lesson");
            var lessonTags = lesson.LessonTags.Where(l => l.Tag.LessonTags.Count == 1);
            foreach (var item in lessonTags)
            {
                _context.Remove(item.Tag);
            }
            if (lesson.LessonMedia != null)
            {
                await _globalService.DeleteImage(lesson.LessonMedia.Name);
            }
            _context.Lesson.Remove(lesson);
            await _context.SaveChangesAsync();
            return Ok("Lesson deleted successfully.");
        }
    }
}
