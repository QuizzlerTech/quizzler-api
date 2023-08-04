using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;

[Route("api/[controller]")]
[ApiController]

public class UserController : ControllerBase
{
    private readonly QuizzlerDbContext _context;
    private readonly UserService _userService;

    public UserController(QuizzlerDbContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }


    // GET: api/User
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.User.ToListAsync();
    }

    // GET: api/User/5
    // [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.User.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }
    // GET: api/User/token
    [Authorize]
    [HttpGet("validateJWT")]
    public async Task<ActionResult<User>> validateJWT()
    {
        return Ok("You are authorized!");
    }



    // POST: api/User
    [HttpPost("registerUser")]
    public async Task<ActionResult<User>> RegisterUser(UserRegisterDto userRegisterDto)
    {
        if (await _userService.EmailExists(userRegisterDto.Email))
        {
            return StatusCode(409, $"Email {userRegisterDto.Email} already registered");
        }
        if (await _userService.UsernameExists(userRegisterDto.Username))
        {
            return StatusCode(409, $"Username {userRegisterDto.Username} already registered");
        }
        if (!_userService.IsEmailCorrect(userRegisterDto.Email))
        {
            return StatusCode(422, $"Email {userRegisterDto.Email} is not a proper email address");
        }
        if (!_userService.IsPasswordGoodEnough(userRegisterDto.Password))
        {
            return StatusCode(422, $"Password {userRegisterDto.Password} does not meet the requirements");
        }

        var user = await _userService.RegisterUser(userRegisterDto);

        await _context.SaveChangesAsync();

        // Return a 201 Created status code and the location of the new resource
        return new CreatedAtActionResult(nameof(UserController.GetUser), "User", new { id = user.UserId }, null);
    }

    // POST: api/User

    [HttpPost("loginUser")]
    public async Task<ActionResult<User>> LoginUser(LoginDto loginDto)
    {
        if (!await _userService.DoesExist(loginDto.UsernameOrEmail))
        {
            return StatusCode(409, $"{loginDto.UsernameOrEmail} is not registered");
        }
        if (await _userService.AreLoginCredentialCorrect(loginDto))
        {
            var token = _userService.GenerateJwtToken();
            return Ok(token);
        }
        else
        {
            return StatusCode(403, $"Wrong credentials");
        }

    }


    //  PUT: api/User/5
    //  [HttpPut("{id}")]
    //
    // LATER
    // 

    [Authorize]
    // DELETE: api/User/
    [HttpDelete("{email}")]
    public async Task<ActionResult<User>> DeleteUser(string email)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return NotFound();
        }

        _context.User.Remove(user);
        await _context.SaveChangesAsync();

        return user;
    }
}
