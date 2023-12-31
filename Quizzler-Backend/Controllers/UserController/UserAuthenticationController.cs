using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using Quizzler_Backend.Services;

namespace Quizzler_Backend.Controllers.UserController
{
    [Route("api/user/auth")]
    [ApiController]
    public class UserAuthenticationController(UserAuthenticationService userAuthenticationService) : ControllerBase
    {
        private readonly UserAuthenticationService _userAuthenticationService = userAuthenticationService;

        [Authorize]
        [HttpGet("check")]
        public async Task<IActionResult> CheckAuth()
        {
            return await _userAuthenticationService.CheckAuthAsync(User);
        }
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto userRegisterDto)
        {

            return await _userAuthenticationService.RegisterAsync(userRegisterDto);
        }
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(UserLoginDto userLoginDto)
        {
            return await _userAuthenticationService.LoginAsync(userLoginDto);
        }
    }
}
