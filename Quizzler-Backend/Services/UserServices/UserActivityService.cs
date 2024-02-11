using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos.Flashcard;
using Quizzler_Backend.Dtos.Lesson;
using Quizzler_Backend.Dtos.User;
using System.Security.Claims;

namespace Quizzler_Backend.Services.UserServices
{
    public class UserActivityService(QuizzlerDbContext context, GlobalService globalService)
    {
        private readonly QuizzlerDbContext _context = context;
        private readonly GlobalService _globalService = globalService;



        public async Task<ActionResult<IEnumerable<DateTime>>> GetUserFlashcardsCreationDatesAsync(ClaimsPrincipal userPrincipal)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var creationDates = await _context.Flashcard
                .Where(f => f.Lesson.OwnerId == userId)
                .Select(f => f.DateCreated)
                .ToListAsync();

            return new OkObjectResult(creationDates);
        }

        public async Task<ActionResult<IEnumerable<FlashcardLogSendDto>>> GetUserLogsAsync(ClaimsPrincipal userPrincipal)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var logs = await _context.FlashcardLog
                .Where(log => log.UserId == userId)
                .Select(log => new FlashcardLogSendDto
                {
                    Date = log.Date,
                    FlashcardId = log.FlashcardId,
                    WasCorrect = log.WasCorrect
                })
                .ToListAsync();

            return new OkObjectResult(logs);
        }

        public async Task<ActionResult<LessonInfoSendCardDto>> GetLastLessonInfoAsync(ClaimsPrincipal userPrincipal)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var lastLesson = await _context.FlashcardLog
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .Include(l => l.Flashcard)
                    .ThenInclude(f => f.Lesson)
                        .ThenInclude(l => l.LessonMedia)
                .Include(l => l.Flashcard)
                    .ThenInclude(f => f.Lesson)
                        .ThenInclude(l => l.Flashcards)
                .Include(l => l.Flashcard)
                    .ThenInclude(f => f.Lesson)
                        .ThenInclude(l => l.Likes)
                 .Include(l => l.Flashcard)
                    .ThenInclude(f => f.Lesson).ThenInclude(l => l.Owner)
                        .ThenInclude(u => u.Lesson)
                .OrderByDescending(fl => fl.Date)
                    .Select(l => l.Flashcard.Lesson)
                .FirstOrDefaultAsync();

            if (lastLesson == null)
                return new NotFoundObjectResult("No lessons found");

            var ownerDto = new UserSendDto
            {
                UserId = lastLesson.Owner.UserId,
                Username = lastLesson.Owner.Username,
                Avatar = lastLesson.Owner.Avatar,
                FirstName = lastLesson.Owner.FirstName,
                LastName = lastLesson.Owner.LastName,
                LastSeen = lastLesson.Owner.LastSeen,
                LessonCount = lastLesson.Owner.Lesson.Count
            };

            var result = new LessonInfoSendCardDto
            {
                LessonId = lastLesson.LessonId,
                Title = lastLesson.Title,
                Description = lastLesson.Description ?? "",
                ImageName = lastLesson.LessonMedia?.Name ?? "",
                FlashcardCount = lastLesson.Flashcards.Count,
                Owner = ownerDto,
                LikesCount = lastLesson.Likes.Count,
                IsLiked = lastLesson.Likes.Any(like => like.UserId == userId)
            };

            return new OkObjectResult(result);
        }

        public async Task<ActionResult<IEnumerable<LessonInfoSendDto>>> GetLikedLessonsAsync(ClaimsPrincipal userPrincipal)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var lessons = await _context.Lesson
                    .AsNoTracking()
                    .Include(l => l.LessonMedia)
                    .Include(l => l.LessonTags).ThenInclude(t => t.Tag)
                    .Include(l => l.Flashcards)
                    .Where(l => l.Likes.Any(like => like.UserId == userId) && (l.IsPublic || l.OwnerId == userId))
                    .Include(l => l.Likes)
                    .Include(l => l.Owner)
                    .Select(l => new LessonInfoSendDto
                    {
                        LessonId = l.LessonId,
                        Title = l.Title,
                        Description = l.Description ?? "",
                        ImageName = l.LessonMedia != null ? l.LessonMedia.Name : "",
                        DateCreated = l.DateCreated,
                        IsPublic = l.IsPublic,
                        Tags = l.LessonTags.Select(t => t.Tag.Name).ToList(),
                        FlashcardCount = l.Flashcards.Count,
                        LikesCount = l.Likes.Count,
                        IsLiked = l.Likes.Any(like => like.UserId == userId),
                        Owner = new UserSendDto
                        {
                            UserId = l.Owner.UserId,
                            Username = l.Owner.Username,
                            Avatar = l.Owner.Avatar,
                            FirstName = l.Owner.FirstName,
                            LastName = l.Owner.LastName,
                            LastSeen = l.Owner.LastSeen,
                            LessonCount = l.Owner.Lesson.Count
                        }
                    })
                    .ToListAsync();

            return new OkObjectResult(lessons);
        }

        public async Task<ActionResult<IEnumerable<DateTime>>> GetLastWeekActivityAsync(ClaimsPrincipal userPrincipal)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var startDate = DateTime.UtcNow.AddDays(-6);
            var activities = await _context.FlashcardLog
                .Where(fl => fl.UserId == userId && fl.Date > startDate)
                .Select(fl => fl.Date)
                .GroupBy(fl => fl.Date)
                .Select(flgroup => flgroup.Key)
                .ToListAsync();

            return new OkObjectResult(activities);
        }
    }
}