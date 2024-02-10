using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.SecurityTokenService;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos.Flashcard;
using Quizzler_Backend.Models;
using System.Security.Claims;

namespace Quizzler_Backend.Services
{
    public class FlashcardService
    {
        private readonly GlobalService _globalService;
        private readonly QuizzlerDbContext _context;

        public FlashcardService(GlobalService globalService, QuizzlerDbContext context)
        {
            _globalService = globalService;
            _context = context;
        }
        public Flashcard CreateNewFlashcard(FlashcardAddDto flashcardAddDto)
        {
            var flashcard = new Flashcard
            {
                LessonId = flashcardAddDto.LessonId,
                QuestionText = flashcardAddDto.QuestionText,
                AnswerText = flashcardAddDto.AnswerText,
                DateCreated = DateTime.UtcNow,
            };
            return flashcard;
        }
        public async Task<ActionResult> AddNewFlashcard(ClaimsPrincipal userPrincipal, FlashcardAddDto flashcardAddDto)
        {
            int? userIdTemp = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userIdTemp == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            int userId = (int)userIdTemp;


            var lessonToAddTo = await _context.Lesson.FindAsync(flashcardAddDto.LessonId);
            if (lessonToAddTo == null) return new NotFoundObjectResult("Not found the lesson");
            if (lessonToAddTo.OwnerId != userId) throw new UnauthorizedAccessException("User is not the owner of the lesson");

            var newFlashcard = CreateNewFlashcard(flashcardAddDto);

            if (IsContentMissing(newFlashcard)) return new BadRequestObjectResult("No text nor image provided for the flashcard");

            if (flashcardAddDto.QuestionImage != null)
            {
                if (!await _globalService.IsImageRightSize(flashcardAddDto.QuestionImage))
                    return new BadRequestObjectResult("The question image size is too large");
                newFlashcard.QuestionMedia = await SaveImage(flashcardAddDto.QuestionImage, userId);
            }

            if (flashcardAddDto.AnswerImage != null)
            {
                if (!await _globalService.IsImageRightSize(flashcardAddDto.AnswerImage))
                    return new BadRequestObjectResult("The answer image size is too large");
                newFlashcard.AnswerMedia = await SaveImage(flashcardAddDto.AnswerImage, userId);
            }
            _context.Flashcard.Add(newFlashcard);
            await _context.SaveChangesAsync();

            return new OkResult();
        }
        public async Task<Media?> SaveImage(IFormFile image, int userId)
        {
            var imageName = GenerateImageName();
            var newMedia = await _globalService.SaveImage(image, imageName, userId);
            if (newMedia == null) return null;
            _context.Media.Add(newMedia);
            await _context.SaveChangesAsync();
            return newMedia;
        }
        public async Task<ActionResult> UpdateFlashcard(ClaimsPrincipal userPrincipal, FlashcardUpdateDto flashcardUpdateDto)
        {
            int userId = int.Parse(userPrincipal.FindFirstValue("sub"));

            var flashcard = await _context.Flashcard
                .Include(f => f.Lesson)
                .Include(f => f.QuestionMedia)
                .Include(f => f.AnswerMedia)
                .FirstOrDefaultAsync(f => f.FlashcardId == flashcardUpdateDto.FlashcardId);

            if (flashcard == null) return new NotFoundObjectResult("Not found the flashcard");
            if (flashcard.Lesson.OwnerId != userId) return new UnauthorizedObjectResult("User is not the owner of the lesson");

            // Update text properties
            flashcard.QuestionText = flashcardUpdateDto.QuestionText ?? flashcard.QuestionText;
            flashcard.AnswerText = flashcardUpdateDto.AnswerText ?? flashcard.AnswerText;

            // Handle image updates 
            if (flashcardUpdateDto.QuestionImage != null)
            {
                await UpdateImage(flashcard, flashcardUpdateDto.QuestionImage, "QuestionMedia");
            }

            if (flashcardUpdateDto.AnswerImage != null)
            {
                await UpdateImage(flashcard, flashcardUpdateDto.AnswerImage, "AnswerMedia");
            }

            // Validate final content
            if (IsContentMissing(flashcard)) return new NotFoundObjectResult("No text nor image after the update");

            _context.Update(flashcard);
            await _context.SaveChangesAsync();

            return new OkResult();
        }

        private async Task UpdateImage(Flashcard flashcard, IFormFile image, string mediaPropertyName)
        {
            if (!await _globalService.IsImageRightSize(image))
            {
                throw new BadRequestException("The image size is too large");
            }

            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);

            var newMedia = await _globalService.SaveImage(image, GenerateImageName(), flashcard.Lesson.OwnerId) ?? throw new Exception("Error saving image");
            var existingMedia = mediaPropertyName == "QuestionMedia" ? flashcard.QuestionMedia : flashcard.AnswerMedia;
            if (existingMedia != null)
            {
                _context.Remove(existingMedia);
                await _globalService.DeleteImage(existingMedia.Name);
            }
            if (mediaPropertyName == "QuestionMedia") flashcard.QuestionMedia = newMedia;
            else flashcard.AnswerMedia = newMedia;

            _context.Media.Add(newMedia);
        }



        public string GenerateImageName()
        {
            return _globalService.CreateSalt() + ".jpeg";
        }
        public bool IsContentMissing(Flashcard flashcard)
        {
            return (flashcard.QuestionText == null && flashcard.QuestionMedia == null) ||
                   (flashcard.QuestionText == "" && flashcard.QuestionMedia == null) ||
                   (flashcard.AnswerText == null && flashcard.AnswerMedia == null) ||
                   (flashcard.AnswerText == "" && flashcard.AnswerMedia == null);
        }
    }
}
