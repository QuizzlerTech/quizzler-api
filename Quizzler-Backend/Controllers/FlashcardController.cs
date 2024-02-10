using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos.Flashcard;
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
        public async Task<ActionResult<Flashcard>> AddNewFlashcard([FromForm] FlashcardAddDto flashcardAddDto)
        {
            return await _flashcardService.AddNewFlashcard(User, flashcardAddDto);
        }

        // PATCH: api/flashcard/update
        // Method to update a flashcard  
        [Authorize]
        [HttpPatch("update")]

        public async Task<ActionResult<Flashcard>> UpdateFlashcard([FromForm] FlashcardUpdateDto flashcardUpdateDto)
        {
            return await _flashcardService.UpdateFlashcard(User, flashcardUpdateDto);
        }

        // DELETE: api/flashcard/delete
        // Method to delete a flashcard
        [Authorize]
        [HttpDelete("delete")]
        public async Task<ActionResult<Flashcard>> Delete(string flashcardId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("No user found");
            var flashcard = await _context.Flashcard.Include(f => f.Lesson).FirstOrDefaultAsync(f => f.FlashcardId.ToString() == flashcardId);
            if (flashcard == null) return NotFound("No flashcard found");

            var lesson = flashcard.Lesson;
            if (userId != flashcard.Lesson.OwnerId.ToString()) return Unauthorized("Not user's lesson");
            // Removes flashcard from the database and save changes
            if (flashcard.QuestionMedia != null) await _globalService.DeleteImage(flashcard.QuestionMedia.Name);
            if (flashcard.AnswerMedia != null) await _globalService.DeleteImage(flashcard.AnswerMedia.Name);
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
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (flashcard == null) return BadRequest("Flashcard not found");
            var newLog = new FlashcardLog { Date = DateTime.UtcNow, Flashcard = flashcard, WasCorrect = flashcardLogDto.WasCorrect, User = user, LessonId = flashcard.LessonId };

            _context.FlashcardLog.Add(newLog);
            await _context.SaveChangesAsync();

            return Ok("Log made");
        }
    }
}
