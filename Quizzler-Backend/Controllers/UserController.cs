using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly QuizzlerDbContext _context;
    private readonly UserService _userService;

    // Controller constructor
    public UserController(QuizzlerDbContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    // GET: api/profile
    // Method to get logged user profile info 
    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<User>> GetMyProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await GetUserProfile(Convert.ToInt32(userId));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/user/{id}/profile
    // Method to get profile info 
    [Authorize]
    [HttpGet("{id}/profile")]
    public async Task<ActionResult<User>> GetUserProfile(int id)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return NotFound();
        var result = new User
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            LastSeen = user.LastSeen,
            DateRegistered = user.DateRegistered,
        };
        return result;
    }

    // PUT: api/user/update
    // Method to get profile info 
    [Authorize]
    [HttpPatch("update")]
    public async Task<ActionResult<User>> UpdateUser(UserUpdateDto userUpdateDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

        if (!await _userService.AreCredentialsCorrect(new LoginDto { Email = user.Email, Password = userUpdateDto.CurrentPassword })) return StatusCode(400, $"Wrong credentials");


        // Checks if the email or username already exists
        if (userUpdateDto.Email is not null)
        {
            if (await _userService.EmailExists(userUpdateDto.Email) && !(userUpdateDto.Email == user.Email)) return StatusCode(409, $"Email {userUpdateDto.Email} already registered");
        }
        if (userUpdateDto.Username is not null)
        {
            if (await _userService.UsernameExists(userUpdateDto.Username) && !(userUpdateDto.Username == user.Username)) return StatusCode(409, $"Username {userUpdateDto.Username} already registered");
        }
        // Checks if the email is correct
        if (userUpdateDto.Email is not null)
        {
            if (!_userService.IsEmailCorrect(userUpdateDto.Email)) return StatusCode(422, $"Email {userUpdateDto.Email} is not a proper email address");
        }
        // Checks if the password meets the criteria
        if (userUpdateDto.Password is not null)
        {
            if (!_userService.IsPasswordGoodEnough(userUpdateDto.Password)) return StatusCode(422, $"Password does not meet the requirements");
        }

        user.Username = userUpdateDto.Username ?? user.Username;
        user.Email = userUpdateDto.Email ?? user.Email;
        user.FirstName = userUpdateDto.FirstName ?? user.FirstName;
        user.LastName = userUpdateDto.LastName ?? user.LastName;
        user.LoginInfo.PasswordHash = userUpdateDto.Password != null ? _userService.HashPassword(userUpdateDto.Password, user.LoginInfo.Salt) : user.LoginInfo.PasswordHash;
        user.Avatar = userUpdateDto.Avatar ?? user.Avatar;

        _context.SaveChanges();

        return Ok("Updated");
    }

    // GET: api/user/check
    // Method to validate JSON Web Token
    [Authorize]
    [HttpGet("check")]
    public async Task<ActionResult<User>> CheckAuth()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
        user.LastSeen = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok("You are authorized!");
    }

    // POST: api/user/register
    // Method to register a new user
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(UserRegisterDto userRegisterDto)
    {
        // Checks if the email or username already exists
        if (await _userService.EmailExists(userRegisterDto.Email))
        {
            return StatusCode(409, $"Email {userRegisterDto.Email} already registered");
        }
        if (await _userService.UsernameExists(userRegisterDto.Username))
        {
            return StatusCode(409, $"Username {userRegisterDto.Username} already registered");
        }

        // Checks if the email is correct
        if (!_userService.IsEmailCorrect(userRegisterDto.Email))
        {
            return StatusCode(422, $"Email {userRegisterDto.Email} is not a proper email address");
        }

        // Checks if the password meets the criteria
        if (!_userService.IsPasswordGoodEnough(userRegisterDto.Password))
        {
            return StatusCode(422, $"Password does not meet the requirements");
        }

        var user = await _userService.CreateUser(userRegisterDto);
        _context.User.Add(user);

        await _context.SaveChangesAsync();

        return new CreatedAtActionResult(nameof(GetUserProfile), "User", new { id = user.UserId }, "Created user");

    }

    // POST: api/user/login
    // Method for user login
    [HttpPost("login")]
    public async Task<ActionResult<User>> Login(LoginDto loginDto)
    {
        // Checks if the user exists
        if (!await _userService.DoesExist(loginDto.Email))
        {
            return StatusCode(409, $"{loginDto.Email} is not registered");
        }

        // Checks if the login credentials are correct
        if (await _userService.AreCredentialsCorrect(loginDto))
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            var token = _userService.GenerateJwtToken(user);
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(token);
        }
        else
        {
            return StatusCode(400, $"Wrong credentials");
        }
    }
    // DELETE: api/user/delete
    // Method to delete a user
    [Authorize]
    [HttpDelete("delete")]
    public async Task<ActionResult<User>> Delete(string userPassword)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.User.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
        try
        {
            if (await _userService.AreCredentialsCorrect(new LoginDto { Email = user.Email, Password = userPassword }))
            {
                // Removes user from the database and save changes
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
                return Ok("User deleted successfully.");
            }
        }
        catch (Exception ex)
        {
            return NotFound();
        }

        return StatusCode(403, "Invalid credentials");
    }
}
