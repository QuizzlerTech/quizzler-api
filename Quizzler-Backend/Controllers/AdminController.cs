using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Data;
using System.Security.Claims;

namespace Quizzler_Backend.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly QuizzlerDbContext _context;
        public AdminController(QuizzlerDbContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet("populate/flashcardLogs")]
        public async Task<ActionResult> PopulateFlashcardLogs()
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();
            if (user.Username != "admin") return Unauthorized();        // not ideal but serves the role and doesnt require DB changes

            var flashcards = await _context.Flashcard.ToDictionaryAsync(fc => fc.FlashcardId, fc => fc.LessonId);
            await _context.FlashcardLog.ForEachAsync(log =>
            {
                log.LessonId = flashcards[log.FlashcardId];
            });
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
