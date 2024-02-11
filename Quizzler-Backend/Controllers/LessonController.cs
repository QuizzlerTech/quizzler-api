using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos.Flashcard;
using Quizzler_Backend.Dtos.Lesson;
using Quizzler_Backend.Dtos.User;
using Quizzler_Backend.Filters;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;
using Quizzler_Backend.Services.LessonServices;
using System.Security.Claims;

namespace Quizzler_Backend.Controllers
{
    [Route("api/lesson")]
    [ApiController]
    public class LessonController(QuizzlerDbContext context, LessonService lessonService, GlobalService globalService) : ControllerBase
    {
        private readonly QuizzlerDbContext _context = context;
        private readonly LessonService _lessonService = lessonService;
        private readonly GlobalService _globalService = globalService;

        [HttpGet("{id}")]
        [AllowPublicLessonFilter]
        public async Task<ActionResult<LessonSendDto>> GetLessonById(int id)
        {
            var lesson = await _context.Lesson
                                       .Include(l => l.LessonTags)
                                            .ThenInclude(lt => lt.Tag)
                                       .Include(l => l.LessonMedia)
                                       .Include(l => l.Flashcards)
                                            .ThenInclude(f => f.AnswerMedia)
                                       .Include(l => l.Flashcards)
                                            .ThenInclude(f => f.QuestionMedia)
                                       .Include(l => l.Owner)
                                            .ThenInclude(u => u.Lesson)
                                       .Include(l => l.Likes)
                                       .AsSplitQuery()
                                       .FirstOrDefaultAsync(u => u.LessonId == id);

            if (lesson == null) return NotFound();


            var flashcards = lesson.Flashcards.Select(f => new FlashcardSendDto
            {
                FlashcardId = f.FlashcardId,
                DateCreated = f.DateCreated,
                QuestionText = f.QuestionText,
                AnswerText = f.AnswerText,
                QuestionImageName = f.QuestionMedia?.Name,
                AnswerImageName = f.AnswerMedia?.Name
            }).ToList();
            var loggedUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var loggeduser = await _context.User
                .Include(u => u.FlashcardLog)
                .FirstOrDefaultAsync(u => u.UserId == loggedUserId);

            if (loggeduser != null)
            {
                var flashcardLogs = loggeduser.FlashcardLog.Where(fl => fl.LessonId == lesson.LessonId).ToList();
                if (flashcardLogs.Count > 0)
                {
                    flashcards = _lessonService.GetOrderOfFlashcards(flashcards, flashcardLogs);
                }
            }
            var userSendDto = new UserSendDto
            {
                UserId = lesson.Owner.UserId,
                Username = lesson.Owner.Username,
                FirstName = lesson.Owner.FirstName,
                LastName = lesson.Owner.LastName,
                LastSeen = lesson.Owner.LastSeen,
                Avatar = lesson.Owner.Avatar,
                LessonCount = lesson.Owner.Lesson.Count
            };
            // Populate LessonSendDto
            var lessonSendDto = new LessonSendDto
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                ImageName = lesson.LessonMedia?.Name,
                DateCreated = lesson.DateCreated,
                IsPublic = lesson.IsPublic,
                Owner = userSendDto,
                Tags = lesson.LessonTags.Select(l => l.Tag.Name).ToList(),
                Flashcards = flashcards,
                LikesCount = lesson.Likes.Count,
                IsLiked = lesson.Likes.Any(l => l.UserId == loggedUserId)
            };
            return Ok(lessonSendDto);
        }

        [HttpGet("byUser/{userId}/{title}")]
        [AllowPublicLessonFilter]
        public async Task<ActionResult<LessonSendDto>> GetLessonByTitle(int userId, string title)
        {
            Console.WriteLine("GetLessonByTitle");
            var user = await _context.User
                                        .Include(u => u.Lesson)
                                            .ThenInclude(l => l.LessonTags)
                                                .ThenInclude(lt => lt.Tag)
                                        .Include(u => u.Lesson)
                                            .ThenInclude(l => l.Flashcards)
                                            .ThenInclude(f => f.QuestionMedia)
                                        .Include(u => u.Lesson)
                                            .ThenInclude(l => l.Flashcards)
                                            .ThenInclude(f => f.AnswerMedia)
                                        .Include(u => u.Lesson)
                                            .ThenInclude(l => l.LessonMedia)
                                        .Include(u => u.Lesson)
                                            .ThenInclude(l => l.Likes)
                                        .AsSplitQuery()
                                        .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();
            var lesson = user.Lesson
                .FirstOrDefault(l => l.Title == title);
            if (lesson == null) return NotFound();

            var flashcards = lesson.Flashcards.Select(f => new FlashcardSendDto
            {
                FlashcardId = f.FlashcardId,
                DateCreated = f.DateCreated,
                QuestionText = f.QuestionText,
                AnswerText = f.AnswerText,
                QuestionImageName = f.QuestionMedia?.Name,
                AnswerImageName = f.AnswerMedia?.Name
            }).ToList();
            var loggedUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var loggeduser = await _context.User
                .Include(u => u.FlashcardLog)
                .FirstOrDefaultAsync(u => u.UserId == loggedUserId);

            if (loggeduser != null)
            {
                var flashcardLogs = loggeduser.FlashcardLog.Where(fl => fl.LessonId == lesson.LessonId).ToList();
                if (flashcardLogs.Count > 0)
                {
                    flashcards = _lessonService.GetOrderOfFlashcards(flashcards, flashcardLogs);
                }
            }
            var userSendDto = new UserSendDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                LastSeen = user.LastSeen,
                Avatar = user.Avatar,
                LessonCount = user.Lesson.Count
            };
            // Populate LessonSendDto
            var lessonSendDto = new LessonSendDto
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                ImageName = lesson.LessonMedia?.Name,
                DateCreated = lesson.DateCreated,
                IsPublic = lesson.IsPublic,
                Owner = userSendDto,
                Tags = lesson.LessonTags.Select(l => l.Tag.Name).ToList(),
                Flashcards = flashcards,
                LikesCount = lesson.Likes.Count,
                IsLiked = lesson.Likes.Any(l => l.UserId == loggedUserId)
            };
            return Ok(lessonSendDto);
        }

        [HttpGet("topLessons")]
        public async Task<ActionResult<List<LessonInfoSendCardDto>>> GetTopLessons(int top = 5)
        {
            var loggedUserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var lessons = await _context.Lesson
                .Where(l => l.IsPublic)
                .OrderByDescending(l => l.Flashcards.Sum(f => f.FlashcardLog!.Count))
                .Take(top)
                .Select(l => new LessonInfoSendCardDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Description = l.Description,
                    FlashcardCount = l.Flashcards.Count,
                    ImageName = l.LessonMedia!.Name,
                    Owner = new UserSendDto
                    {
                        UserId = l.Owner.UserId,
                        Username = l.Owner.Username,
                        FirstName = l.Owner.FirstName,
                        LastName = l.Owner.LastName,
                        LastSeen = l.Owner.LastSeen,
                        Avatar = l.Owner.Avatar,
                        LessonCount = l.Owner.Lesson.Count
                    },
                    LikesCount = l.Likes.Count,
                    IsLiked = l.Likes.Any(like => like.UserId == loggedUserId)
                })
                .ToListAsync();

            return Ok(lessons);
        }

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
                _context.Media.Add(newMedia);
                lesson.LessonMedia = newMedia;
            }

            if (lessonAddDto.TagNames != null)
            {
                foreach (var tagName in lessonAddDto.TagNames)
                {
                    try
                    {
                        await _lessonService.AddLessonTag(tagName, lesson);
                    }
                    catch (ArgumentException ex)
                    {
                        return BadRequest(ex.Message);
                    }

                }
            }

            _context.Lesson.Add(lesson);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(nameof(GetLessonById), "Lesson", new { id = lesson.LessonId }, $"Created lesson {lesson.LessonId}");
        }

        [Authorize]
        [HttpPost("like")]
        public async Task<ActionResult<Lesson>> LikeLesson(int lessonId, bool like)
        {
            var lesson = await _context.Lesson.FirstOrDefaultAsync(l => l.LessonId == lessonId);
            if (lesson == null) return BadRequest("Lesson not found");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
            if (user == null) return Unauthorized("Unauthorized");

            var currentLike = await _context.Like.FirstOrDefaultAsync(l => l.LessonId == lessonId && l.UserId == user.UserId);
            if (currentLike != null && !like)
            {
                _context.Remove(like);
            }
            else if (currentLike == null && like)
            {
                _context.Like.Add(new Like { LessonId = lessonId, UserId = user.UserId });
            }

            await _context.SaveChangesAsync();
            return Ok("Liked set");
        }

        // POST: api/lesson/importCsv
        // Method to create new lesson
        /*        [Authorize]
                [HttpPost("importCsv")]
                public async Task<ActionResult<Lesson>> ImportFlashcardsFromCsv(IFormFile Csv, int LessonId)
                {
                    var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    var user = await _context.User
                        .Include(u => u.Lesson)
                        .FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user == null) return Unauthorized("No user found");

                    var lesson = user.Lesson.FirstOrDefault(l => l.LessonId == LessonId);
                    if (lesson == null) return NotFound();

                    using (var reader = new StreamReader(Csv.OpenReadStream()))
                    {
                        var fileContent = reader.ReadToEndAsync().Result.Split('\n');
                        foreach (var line in fileContent)
                        {
                            if (!line.Contains(';')) continue;
                            var flashcard = new Flashcard { AnswerText = line.Split(';')[0], QuestionText = line.Split(';')[1] };
                            lesson.Flashcards.Add(flashcard);
                        }
                    }

                    await _context.SaveChangesAsync();

                    return Ok(lesson);
                }*/


        [Authorize]
        [HttpPatch("update")]
        public async Task<ActionResult<Lesson>> UpdateLesson([FromForm] LessonUpdateDto lessonUpdateDto)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.User.Include(u => u.Lesson).FirstOrDefaultAsync(u => u.UserId == userId);
            var lesson = await _context.Lesson.Include(l => l.LessonMedia).FirstOrDefaultAsync(u => u.LessonId == lessonUpdateDto.LessonId);

            if (user == null) return Unauthorized("Unauthorized");
            if (lesson == null) return BadRequest("Invalid lesson ID");

            if (userId != lesson.OwnerId) return Unauthorized("User is not the owner");
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

            if (Request.Form.ContainsKey("Image") && lessonUpdateDto.Image is null && lesson.LessonMedia != null)
            {
                var media = lesson.LessonMedia;
                lesson.LessonMedia = null;
                _context.Media.Remove(media);
            }

            if (lessonUpdateDto.Image != null)
            {
                if (!await _globalService.IsImageRightSize(lessonUpdateDto.Image)) return BadRequest("The image size is too large");
                using var memoryStream = new MemoryStream();
                await lessonUpdateDto.Image.CopyToAsync(memoryStream);
                var newMedia = await _globalService.SaveImage(lessonUpdateDto.Image, _lessonService.GenerateImageName(lesson.Title), userId);
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

        [Authorize]
        [HttpDelete("delete")]
        public async Task<ActionResult<Lesson>> Delete(string lessonId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var lesson = await _context.Lesson
                .Include(l => l.LessonTags)
                    .ThenInclude(lt => lt.Tag)
                        .ThenInclude(t => t.LessonTags)
                .Include(l => l.LessonMedia)
                .FirstOrDefaultAsync(u => u.LessonId.ToString() == lessonId);
            if (lesson == null) return NotFound("No lesson found");
            if (userId != lesson.OwnerId.ToString()) return Unauthorized("Not user's lesson");
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
