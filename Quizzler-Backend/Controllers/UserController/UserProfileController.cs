using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizzler_Backend.Dtos.Lesson;
using Quizzler_Backend.Dtos.User;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services.UserServices;

namespace Quizzler_Backend.Controllers.UserController
{
    [Route("api/user/profile")]
    [ApiController]
    public class UserProfileController(UserProfileService userProfileService) : ControllerBase
    {
        private readonly UserProfileService _userProfileService = userProfileService;

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            return await _userProfileService.GetMyProfileAsync(User);
        }

        [HttpGet("{username}/profile")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfileByUsername(string username)
        {
            return await _userProfileService.GetUserProfileByUsernameAsync(username);
        }
        [Authorize]
        [HttpGet("lessons")]
        public async Task<ActionResult<IEnumerable<LessonInfoSendDto>>> GetMyLessons()
        {
            return await _userProfileService.GetMyLessonsAsync(User);
        }

        [HttpGet("{id}/lessons")]
        public async Task<ActionResult<IEnumerable<LessonInfoSendDto>>> GetUserLessonsById(int id)
        {
            return await _userProfileService.GetUserLessonsByIdAsync(id);
        }

        [Authorize]
        [HttpPatch("update")]
        public async Task<ActionResult<User>> UpdateUser(UserUpdateDto userUpdateDto)
        {
            return await _userProfileService.UpdateUserAsync(User, userUpdateDto);
        }

        [Authorize]
        [HttpPatch("updateAvatar")]
        public async Task<ActionResult<User>> UpdateUserAvatar(UserUpdateAvatarDto userUpdateAvatarDto)
        {
            return await _userProfileService.UpdateUserAvatarAsync(User, userUpdateAvatarDto);
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<ActionResult<User>> Delete(string userPassword)
        {
            return await _userProfileService.DeleteUserAsync(User, userPassword);
        }
    }
}
