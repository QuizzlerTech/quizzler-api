using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Dtos;
using Quizzler_Backend.Models;
using System.Security.Claims;


namespace Quizzler_Backend.Controllers
{
    [Route("api/media")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly QuizzlerDbContext _context;
        /*  private readonly MediaService _mediaService;*/
        public MediaController(QuizzlerDbContext context/*, MediaService _mediaService*/)
        {
            _context = context;
            /*_mediaService = mediaService;*/
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file.");
            }

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine("/var/www/quizzler/images/", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save image metadata to MySQL database
            // ...

            return Ok(new { FileName = fileName });
        }



    }
}
