using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;

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
