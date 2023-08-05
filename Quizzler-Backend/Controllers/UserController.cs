using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

// Defining the route and controller type
[Route("api/[controller]")]
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

    // GET: api/User/
    // Method to get user by their email
    [Authorize]
    [HttpGet("GetUser")]
    public async Task<ActionResult<User>> GetUser(string email)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    // GET: api/User/validateJWT
    // Method to validate JSON Web Token
    [Authorize]
    [HttpGet("ValidateJWT")]
    public async Task<ActionResult<User>> ValidateJWT()
    {
        return Ok("You are authorized!");
    }

    // POST: api/User/registerUser
    // Method to register a new user
    [HttpPost("RegisterUser")]
    public async Task<ActionResult<User>> RegisterUser(UserRegisterDto userRegisterDto)
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

        // Checks if the email is correctly formatted
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

        return new CreatedAtActionResult(nameof(GetUser), "User", new { email = user.Email }, "Created user");

    }

    // POST: api/User/loginUser
    // Method for user login
    [HttpPost("LoginUser")]
    public async Task<ActionResult<User>> LoginUser(LoginDto loginDto)
    {
        // Checks if the user exists
        if (!await _userService.DoesExist(loginDto.UsernameOrEmail))
        {
            return StatusCode(409, $"{loginDto.UsernameOrEmail} is not registered");
        }

        // Checks if the login credentials are correct
        if (await _userService.AreLoginCredentialsCorrect(loginDto))
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == loginDto.UsernameOrEmail || u.Email == loginDto.UsernameOrEmail);
            var token = _userService.GenerateJwtToken(user);
            return Ok(token);
        }
        else
        {
            return StatusCode(400, $"Wrong credentials");
        }
    }

    // DELETE: api/User/deleteUser
    // Method to delete a user
    [Authorize]
    [HttpDelete("DeleteUser")]
    public async Task<ActionResult<User>> DeleteUser(string userPassword)
    {

        var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        try
        {
            if (await _userService.AreCredentialsCorrectByEmail(email, userPassword))
            {
                // Removes user from the database and save changes
                var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
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
