using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Filters;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;
using System.Security.Claims;

namespace Quizzler_Backend.Controllers
{
    [Route("api/flashcard")]
    [ApiController]
    public class FlashcardController : ControllerBase
    {
        private readonly QuizzlerDbContext _context;
        private readonly GlobalService _globalService;
        private readonly FlashcardService _flashcardService;


        public FlashcardController(QuizzlerDbContext context, GlobalService globalService, FlashcardService flashcardService)
        {
            _context = context;
            _globalService = globalService;
            _flashcardService = flashcardService;

        }
        // POST: api/flashcard/add
        // Method to create new flashcard
        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult<Lesson>> AddNewFlashcard([FromForm] FlashcardAddDto flashcardAddDto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var lessonToAddTo = await _context.Lesson.FirstOrDefaultAsync(u => u.LessonId == flashcardAddDto.LessonId);
            if (lessonToAddTo == null) return NotFound("Not found the lesson");
            if (!(lessonToAddTo.OwnerId == userId)) return Unauthorized("User is not the owner of the lesson");
            var newFlaschard = await _flashcardService.createNewFlashcard(flashcardAddDto);

            if (flashcardAddDto.QuestionImage is not null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await flashcardAddDto.QuestionImage.CopyToAsync(memoryStream);
                    var newMedia = await _globalService.SaveImage(flashcardAddDto.QuestionImage, _flashcardService.GenerateImageName(), userId);
                    if (newMedia == null) return StatusCode(500, "Error saving image");
                    _context.Media.Add(newMedia);
                    newFlaschard.QuestionMedia = newMedia;
                }
            }
            if (flashcardAddDto.AnswerImage is not null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await flashcardAddDto.AnswerImage.CopyToAsync(memoryStream);
                    var newMedia = await _globalService.SaveImage(flashcardAddDto.AnswerImage, _flashcardService.GenerateImageName(), userId);
                    if (newMedia == null) return StatusCode(500, "Error saving image");
                    _context.Media.Add(newMedia);
                    newFlaschard.AnswerMedia = newMedia;
                }
            }

            _context.Flashcard.Add(newFlaschard);

            await _context.SaveChangesAsync();
            return StatusCode(201, $"Created flashcard {newFlaschard.FlashcardId}");
            }



    }
}
