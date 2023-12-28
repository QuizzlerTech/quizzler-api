using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quizzler_Backend.Controllers;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Dtos.Flashcard;
using Quizzler_Backend.Dtos.Lesson;
using Quizzler_Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace Quizzler_Backend.Services
{
    public class UserService
    {
        private readonly QuizzlerDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly GlobalService _globalService;

        public UserService(QuizzlerDbContext context, IConfiguration configuration, GlobalService globalService)
        {
            _context = context;
            _configuration = configuration;
            _globalService = globalService;
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExists(string username)
        {
            return await _context.User.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> AreCredentialsCorrect(UserLoginDto userLoginDto)
        {
            var user = await _context.User.Include(u => u.LoginInfo)
                .FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);

            if (user == null)
                return false;

            string generatedPasswordHash = _globalService.HashPassword(userLoginDto.Password, user.LoginInfo.Salt);
            return generatedPasswordHash == user.LoginInfo.PasswordHash;
        }

        public bool IsEmailCorrect(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public bool IsPasswordGoodEnough(string password)
        {
            return password.Length >= 8;
        }

        public User CreateUser(UserRegisterDto userRegisterDto)
        {
            var salt = _globalService.CreateSalt();
            var passwordHash = _globalService.HashPassword(userRegisterDto.Password, salt);

            return new User
            {
                Email = userRegisterDto.Email,
                Username = userRegisterDto.Username,
                FirstName = userRegisterDto.FirstName,
                LastName = userRegisterDto.LastName,
                DateRegistered = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                LoginInfo = new LoginInfo
                {
                    Salt = salt,
                    PasswordHash = passwordHash
                },
            };
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["JwtKey"] ?? throw new InvalidOperationException("JwtKey configuration value is null or empty.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"],
                audience: _configuration["JwtIssuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<ActionResult<UserProfileDto>> GetMyProfileAsync(ClaimsPrincipal userPrincipal)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var userEntity = await _context.User.FindAsync(userId);
            if (userEntity == null)
            {
                return new NotFoundResult();
            }

            return new UserProfileDto
            {
                UserId = userEntity.UserId,
                Username = userEntity.Username,
                Email = userEntity.Email,
                FirstName = userEntity.FirstName,
                LastName = userEntity.LastName,
                DateRegistered = userEntity.DateRegistered,
                LastSeen = userEntity.LastSeen,
                Avatar = userEntity.Avatar
            };
        }

        public async Task<ActionResult<UserProfileDto>> GetUserProfileByUsernameAsync(string username)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return new NotFoundResult();
            }

            return new UserProfileDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateRegistered = user.DateRegistered,
                LastSeen = user.LastSeen,
                Avatar = user.Avatar
            };
        }

        public async Task<IActionResult> CheckAuthAsync(ClaimsPrincipal user)
        {
            if (!int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return new NotFoundObjectResult("User does not exist");
            }

            var userEntity = await _context.User.FindAsync(userId);
            if (userEntity == null)
            {
                return new NotFoundObjectResult("User does not exist");
            }

            userEntity.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new OkObjectResult("You are authorized!");
        }
        public async Task<ActionResult<User>> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            if (await EmailExists(userRegisterDto.Email))
            {
                return new ConflictObjectResult($"Email {userRegisterDto.Email} already registered");
            }

            if (await UsernameExists(userRegisterDto.Username))
            {
                return new ConflictObjectResult($"Username {userRegisterDto.Username} already registered");
            }

            if (!IsEmailCorrect(userRegisterDto.Email))
            {
                return new BadRequestObjectResult($"Email {userRegisterDto.Email} is not a proper email address");
            }

            if (!IsPasswordGoodEnough(userRegisterDto.Password))
            {
                return new BadRequestObjectResult("Password does not meet the required criteria");
            }

            var newUser = CreateUser(userRegisterDto);
            await _context.User.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return new CreatedAtActionResult(nameof(UserController.GetUserProfileByUsername), "User", new { username = newUser.Username }, newUser);
        }
        public async Task<ActionResult<User>> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);
            if (user == null || !await AreCredentialsCorrect(userLoginDto))
            {
                return new UnauthorizedObjectResult("Invalid credentials");
            }

            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return new OkObjectResult(token);
        }

        public async Task<ActionResult<User>> UpdateUserAsync(ClaimsPrincipal userPrincipal, UserUpdateDto userUpdateDto)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }

            var user = await _context.User.Include(u => u.LoginInfo).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return new NotFoundResult();
            }
            if (!await AreCredentialsCorrect(new UserLoginDto { Email = user.Email, Password = userUpdateDto.CurrentPassword }))
                return new UnauthorizedResult();

            // Check and update username if provided and if it's not the same as the current one
            if (!string.IsNullOrEmpty(userUpdateDto.Username) && user.Username != userUpdateDto.Username)
            {
                if (await UsernameExists(userUpdateDto.Username))
                {
                    return new ConflictObjectResult("Username already in use.");
                }
                user.Username = userUpdateDto.Username;
            }

            // Check and update email if provided and if it's not the same as the current one
            if (!string.IsNullOrEmpty(userUpdateDto.Email) && user.Email != userUpdateDto.Email)
            {
                if (!IsEmailCorrect(userUpdateDto.Email) || await EmailExists(userUpdateDto.Email))
                {
                    return new BadRequestObjectResult("Invalid or already used email address.");
                }
                user.Email = userUpdateDto.Email;
            }

            // Update other properties as needed
            user.FirstName = userUpdateDto.FirstName ?? user.FirstName;
            user.LastName = userUpdateDto.LastName ?? user.LastName;

            // Update password if provided
            if (!string.IsNullOrEmpty(userUpdateDto.Password))
            {
                if (!IsPasswordGoodEnough(userUpdateDto.Password))
                {
                    return new BadRequestObjectResult("Password does not meet the required criteria.");
                }
                user.LoginInfo.PasswordHash = _globalService.HashPassword(userUpdateDto.Password, user.LoginInfo.Salt);
            }

            await _context.SaveChangesAsync();
            return new OkObjectResult(user);
        }


        public async Task<ActionResult<User>> UpdateUserAvatarAsync(ClaimsPrincipal userPrincipal, UserUpdateAvatarDto userUpdateAvatarDto)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var user = await _context.User.FindAsync(userId);
            if (user == null)
            {
                return new NotFoundResult();
            }

            user.Avatar = userUpdateAvatarDto.Avatar;
            await _context.SaveChangesAsync();
            return new OkObjectResult("Avatar updated successfully");
        }

        public async Task<ActionResult<User>> DeleteUserAsync(ClaimsPrincipal userPrincipal, string userPassword)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var user = await _context.User.FindAsync(userId);
            if (user == null || !await AreCredentialsCorrect(new UserLoginDto { Email = user.Email, Password = userPassword }))
            {
                return new UnauthorizedResult();
            }
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return new OkObjectResult("User deleted successfully.");
        }
        public async Task<ActionResult<IEnumerable<LessonInfoSendDto>>> GetMyLessonsAsync(ClaimsPrincipal userPrincipal)
        {
            int? userId = _globalService.GetUserIdFromClaims(userPrincipal);
            if (userId == null)
            {
                return new NotFoundObjectResult("Invalid user identifier");
            }
            var lessons = await _context.Lesson
                .Where(l => l.OwnerId == userId)
                .Select(l => new LessonInfoSendDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Description = l.Description,
                    ImageName = l.LessonMedia!.Name,
                    DateCreated = l.DateCreated,
                    IsPublic = l.IsPublic,
                    Tags = l.LessonTags.Select(t => t.Tag.Name).ToList(),
                    FlashcardCount = l.Flashcards.Count,
                    Owner = new UserSendDto
                    {
                        UserId = l.Owner.UserId,
                        Username = l.Owner.Username,
                        Avatar = l.Owner.Avatar
                    }
                })
                .ToListAsync();

            return new OkObjectResult(lessons);
        }


        public async Task<ActionResult<IEnumerable<LessonInfoSendDto>>> GetUserLessonsByIdAsync(int id)
        {
            var lessons = await _context.Lesson
                .Where(l => l.OwnerId == id && l.IsPublic)
                .Select(l => new LessonInfoSendDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Description = l.Description,
                    ImageName = l.LessonMedia!.Name,
                    DateCreated = l.DateCreated,
                    IsPublic = l.IsPublic,
                    Tags = l.LessonTags.Select(t => t.Tag.Name).ToList(),
                    FlashcardCount = l.Flashcards.Count,
                    Owner = new UserSendDto
                    {
                        UserId = l.Owner.UserId,
                        Username = l.Owner.Username,
                        Avatar = l.Owner.Avatar
                    }
                })
                .ToListAsync();

            return new OkObjectResult(lessons);
        }

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
                .Include(l => l.Flashcard)
                    .ThenInclude(f => f.Lesson)
                        .ThenInclude(l => l.Likes)
                 .Include(l => l.Flashcard)
                    .ThenInclude(f => f.Lesson).ThenInclude(l => l.Owner)
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
                        ImageName = l.LessonMedia!.Name ?? "",
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