using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using System.Security.Claims;

namespace Quizzler_Backend.Services
{
    public class UserProfileService(QuizzlerDbContext context, GlobalService globalService, UserUtility userUtility)
    {
        private readonly QuizzlerDbContext _context = context;
        private readonly GlobalService _globalService = globalService;
        private readonly UserUtility _userUtility = userUtility;

        // UserProfileService
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
            if (!await _userUtility.AreCredentialsCorrect(new UserLoginDto { Email = user.Email, Password = userUpdateDto.CurrentPassword }))
                return new UnauthorizedResult();

            // Check and update username if provided and if it's not the same as the current one
            if (!string.IsNullOrEmpty(userUpdateDto.Username) && user.Username != userUpdateDto.Username)
            {
                if (await _userUtility.UsernameExists(userUpdateDto.Username))
                {
                    return new ConflictObjectResult("Username already in use.");
                }
                user.Username = userUpdateDto.Username;
            }

            // Check and update email if provided and if it's not the same as the current one
            if (!string.IsNullOrEmpty(userUpdateDto.Email) && user.Email != userUpdateDto.Email)
            {
                if (!_userUtility.IsEmailCorrect(userUpdateDto.Email) || await _userUtility.EmailExists(userUpdateDto.Email))
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
                if (!_userUtility.IsPasswordGoodEnough(userUpdateDto.Password))
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
            if (user == null || !await _userUtility.AreCredentialsCorrect(new UserLoginDto { Email = user.Email, Password = userPassword }))
            {
                return new UnauthorizedResult();
            }
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return new OkObjectResult("User deleted successfully.");
        }
    }
}