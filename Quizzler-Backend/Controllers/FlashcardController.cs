using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Filters;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;
using System.ComponentModel.DataAnnotations.Schema;
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
        public async Task<ActionResult<Flashcard>> AddNewFlashcard([FromForm] FlashcardAddDto flashcardAddDto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var lessonToAddTo = await _context.Lesson.FirstOrDefaultAsync(u => u.LessonId == flashcardAddDto.LessonId);
            if (lessonToAddTo == null) return NotFound("Not found the lesson");
            if (!(lessonToAddTo.OwnerId == userId)) return Unauthorized("User is not the owner of the lesson");
            var newFlaschard = _flashcardService.CreateNewFlashcard(flashcardAddDto);

            if (flashcardAddDto.QuestionText == null && flashcardAddDto.QuestionImage == null) return BadRequest("No question text nor image");
            if (flashcardAddDto.AnswerText == null && flashcardAddDto.AnswerImage == null) return BadRequest("No answer text nor image");

            if (flashcardAddDto.QuestionImage != null)
            {
                if (!await _globalService.IsImageRightSize(flashcardAddDto.QuestionImage)) return BadRequest("The image size is too large");
                using (var memoryStream = new MemoryStream())
                {
                    await flashcardAddDto.QuestionImage.CopyToAsync(memoryStream);
                    var newMedia = await _globalService.SaveImage(flashcardAddDto.QuestionImage, _flashcardService.GenerateImageName(), userId);
                    if (newMedia == null) return StatusCode(500, "Error saving image");
                    _context.Media.Add(newMedia);
                    newFlaschard.QuestionMedia = newMedia;
                }
            }
            if (flashcardAddDto.AnswerImage != null)
            {
                if (!await _globalService.IsImageRightSize(flashcardAddDto.AnswerImage)) return BadRequest("The image size is too large");
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

        // PATCH: api/flashcard/update
        // Method to update a flashcard  
        [Authorize]
        [HttpPatch("update")]

        public async Task<ActionResult<Flashcard>> UpdateFlashcard([FromForm] FlashcardUpdateDto flashcardUpdateDto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var flashcard = await _context.Flashcard.FirstOrDefaultAsync(f => f.FlashcardId == flashcardUpdateDto.FlashcardId);
            if (flashcard == null) return NotFound("Not found the flashcard");
            if (flashcard.Lesson.OwnerId != userId) return Unauthorized("User is not the owner of the lesson");
            flashcard.QuestionText = flashcardUpdateDto.QuestionText ?? flashcard.QuestionText;
            flashcard.AnswerText = flashcardUpdateDto.AnswerText ?? flashcard.AnswerText;
            if(Request.Form.ContainsKey("QuestionImage") && flashcardUpdateDto.QuestionImage is null && flashcard.QuestionMedia != null)
            {
                var media = flashcard.QuestionMedia;
                flashcard.QuestionMediaId = null;
                flashcard.QuestionMedia = null;
                _context.Media.Remove(media);
            }
            if (Request.Form.ContainsKey("AnswerImage") && flashcardUpdateDto.AnswerImage is null && flashcard.AnswerMedia != null)
            {
                var media = flashcard.AnswerMedia;
                flashcard.AnswerMediaId = null;
                flashcard.AnswerMedia = null;
                _context.Media.Remove(media);
            }
            if (flashcardUpdateDto.QuestionImage != null)
            {
                if (!await _globalService.IsImageRightSize(flashcardUpdateDto.QuestionImage)) return BadRequest("The image size is too large");
                using (var memoryStream = new MemoryStream())
                {
                    await flashcardUpdateDto.QuestionImage.CopyToAsync(memoryStream);
                    var newMedia = await _globalService.SaveImage(flashcardUpdateDto.QuestionImage, _flashcardService.GenerateImageName(), userId);
                    if (newMedia == null) return StatusCode(500, "Error saving image");
                    _context.Media.Add(newMedia);
                    flashcard.QuestionMedia = newMedia;
                }
            }
            if (flashcardUpdateDto.AnswerImage != null)
            {
                if (!await _globalService.IsImageRightSize(flashcardUpdateDto.AnswerImage)) return BadRequest("The image size is too large");
                using (var memoryStream = new MemoryStream())
                {
                    await flashcardUpdateDto.AnswerImage.CopyToAsync(memoryStream);
                    var newMedia = await _globalService.SaveImage(flashcardUpdateDto.AnswerImage, _flashcardService.GenerateImageName(), userId);
                    if (newMedia == null) return StatusCode(500, "Error saving image");
                    _context.Media.Add(newMedia);
                    flashcard.AnswerMedia = newMedia;
                }
            }
            if (flashcard.QuestionText == null && flashcard.QuestionMedia == null) return BadRequest("No question text nor image after the update");
            if (flashcard.AnswerText == null && flashcard.AnswerMedia == null) return BadRequest("No answer text nor image after the update");

            _context.Flashcard.Update(flashcard);
            await _context.SaveChangesAsync();
            return StatusCode(201, $"Updated flashcard {flashcard.FlashcardId}");
        }

        // DELETE: api/flashcard/delete
        // Method to delete a flashcard
        [Authorize]
        [HttpDelete("delete")]
        public async Task<ActionResult<Flashcard>> Delete(string flashcardId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var flashcard = await _context.Flashcard.FirstOrDefaultAsync(f => f.FlashcardId.ToString() == flashcardId);
            if (flashcard == null) return NotFound("No flashcard found");

            var lesson = flashcard.Lesson;
            if (!_globalService.isItUssersLesson(userId, lesson)) return Unauthorized("Not user's lesson");
            // Removes flashcard from the database and save changes
            _context.Flashcard.Remove(flashcard);
            await _context.SaveChangesAsync();
            return Ok("Flashcard deleted successfully.");

        }

        // POST: api/flashcard/log
        // Method to log the flashcard learned
        [HttpPost("log")]
        public async Task<ActionResult<FlashcardLog>> FlashcardLog(FlashcardLogDto flashcardLogDto)
        {
            var flashcard = await _context.Flashcard.FirstOrDefaultAsync(f => f.FlashcardId == flashcardLogDto.FlashcardId);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync( u=> u.UserId.ToString() == userId);
            
            if (flashcard == null) return BadRequest("Flashcard not found");
            var newLog = new FlashcardLog { Date = DateTime.UtcNow, Flashcard = flashcard, WasCorrect = flashcardLogDto.WasCorrect, User = user};
            
            _context.FlashcardLog.Add(newLog);
            await _context.SaveChangesAsync();

            return Ok("Log made");
        }
    }
}
