﻿using FuzzySharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos.Lesson;
using Quizzler_Backend.Dtos.Search;
using Quizzler_Backend.Dtos.User;
using System.Security.Claims;

namespace Quizzler_Backend.Services.SearchServices
{
    public class SearchService(QuizzlerDbContext context, GlobalService globalService)
    {
        private readonly GlobalService _globalService = globalService;
        private readonly QuizzlerDbContext _context = context;

        public class FuzzyMatchResult
        {
            public string Target { get; set; } = string.Empty;
            public int Score { get; set; }
        }
        public async Task<ActionResult<CombinedSearchSendDto>> Search(string query, ClaimsPrincipal userPrincipal)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var queryLower = query.ToLower();
            var threshold = 80;

            var preliminaryUserResults = await _context.User.AsNoTracking()
                .Where(u => u.Username.ToLower().Contains(queryLower)
                            || (u.FirstName != null && queryLower.Contains(u.FirstName.ToLower()))
                            || (u.LastName != null && queryLower.Contains(u.LastName.ToLower()))
                            || (u.FirstName != null && u.FirstName.ToLower().Contains(queryLower))
                            || (u.LastName != null && u.LastName.ToLower().Contains(queryLower)))
                .Include(u => u.Lesson)
                .ToListAsync();

            var preliminaryLessonResults = await
                _context.Lesson.AsNoTracking()
                .Where(l => l.Title.ToLower().Contains(queryLower) || l.LessonTags.Any(lt => lt.Tag.Name.Contains(queryLower) || queryLower.Contains(lt.Tag.Name) && (l.IsPublic || l.OwnerId == userId)))
                .Include(l => l.LessonMedia)
                .Include(l => l.Owner)
                    .ThenInclude(u => u.Lesson)
                .Include(l => l.Flashcards)
                .Include(l => l.LessonTags)
                    .ThenInclude(lt => lt.Tag)
                .Include(l => l.Likes)
                .ToListAsync();

            var userResults = preliminaryUserResults
               .Where(u => FuzzyMatch(queryLower, new List<string> { u.Username, u.FirstName!, u.LastName!, $"{u.FirstName} {u.LastName}" }).Score > threshold)
                .Select(u => new UserSendDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    LastSeen = u.LastSeen,
                    Avatar = u.Avatar,
                    LessonCount = u.Lesson.Count
                })
                .Take(5)
                .ToList();

            var lessonResults = preliminaryLessonResults
                .Where(l => FuzzyMatch(queryLower, l.LessonTags.Select(lt => lt.Tag.Name).Append(l.Title).ToList()).Score > threshold)
                .Select(l => new LessonInfoSendDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    ImageName = l.LessonMedia?.Name,
                    Description = l.Description,
                    FlashcardCount = l.Flashcards.Count,
                    Owner = new UserSendDto
                    {
                        UserId = l.OwnerId,
                        Username = l.Owner.Username,
                        FirstName = l.Owner.FirstName,
                        LastName = l.Owner.LastName,
                        LastSeen = l.Owner.LastSeen,
                        Avatar = l.Owner.Avatar,
                        LessonCount = l.Owner.Lesson.Count
                    },
                    Tags = l.LessonTags.Select(t => t.Tag.Name).ToList(),
                    LikesCount = l.Likes.Count,
                    IsLiked = l.Likes.Any(like => like.UserId == userId),
                    DateCreated = l.DateCreated,
                })
                .Take(5)
                .ToList();

            var combinedResults = new CombinedSearchSendDto
            {
                Users = userResults,
                Lessons = lessonResults
            };

            return new OkObjectResult(combinedResults);
        }
        private static FuzzyMatchResult FuzzyMatch(string query, IEnumerable<string> targets)
        {
            var match = Process.ExtractOne(query, targets);
            if (match != null)
            {
                return new FuzzyMatchResult
                {
                    Target = match.Value,
                    Score = match.Score
                };
            }
            else
            {
                return new FuzzyMatchResult
                {
                    Target = string.Empty,
                    Score = 0
                };
            }
        }
        public async Task<ActionResult<ICollection<string>>> AutoComplete(string query, ClaimsPrincipal userPrincipal)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var queryLower = query.ToLower();
            var threshold = 60;

            var preliminaryUserResults = await _context.User.AsNoTracking()
                .Where(u => u.Username.ToLower().Contains(queryLower)
                     || (u.FirstName != null && queryLower.Contains(u.FirstName.ToLower()))
                     || (u.LastName != null && queryLower.Contains(u.LastName.ToLower()))
                     || (u.FirstName != null && u.FirstName.ToLower().Contains(queryLower))
                     || (u.LastName != null && u.LastName.ToLower().Contains(queryLower)))
                .Include(u => u.Lesson)
                .ToListAsync();

            var preliminaryLessonResults = await _context.Lesson.AsNoTracking()
                .Where(l => l.IsPublic || l.OwnerId == userId)
                .Include(l => l.LessonTags)
                .ThenInclude(lt => lt.Tag)
                .ToListAsync();

            var userResults = preliminaryUserResults
                .Select(u => new
                {
                    User = u,
                    MatchResult = FuzzyMatch(queryLower, new List<string> { u.Username, u.FirstName!, u.LastName!, $"{u.FirstName} {u.LastName}" })
                })
                .Where(x => x.MatchResult.Score > threshold)
                .Select(x => x.MatchResult.Target)
                .Distinct()
                .Take(5)
                .ToList();

            var lessonResults = preliminaryLessonResults
                .Select(l => new
                {
                    Lesson = l,
                    MatchResult = FuzzyMatch(queryLower, l.LessonTags.Select(lt => lt.Tag.Name).Append(l.Title).ToList())
                })
                .Where(x => x.MatchResult.Score > threshold)
                .Select(x => x.MatchResult.Target)
                .Distinct()
                .Take(5)
                .ToList();

            var combinedResults = lessonResults.Concat(userResults).Take(5);
            return new OkObjectResult(combinedResults);
        }
    }
}
