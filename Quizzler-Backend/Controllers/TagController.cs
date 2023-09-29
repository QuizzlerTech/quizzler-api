using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Services;

namespace Quizzler_Backend.Controllers
{
    [Route("api/tag")]
    [ApiController]
    public class TagController : ControllerBase
    { 
        private readonly QuizzlerDbContext _context;
        private readonly GlobalService _globalService;
        private readonly ILogger<LessonController> _logger;

        public TagController(QuizzlerDbContext context, GlobalService globalService, ILogger<LessonController> logger)
        {
            _context = context;
            _logger = logger;
            _globalService = globalService;
        }

        // GET: api/tag/{phrase}
        // Get top 10 tags based on phrase 
        [HttpGet("{phrase}")]
        public async Task<ActionResult<List<string>>> GetTagsBasedOnString(string phrase)
        {
            List<string> list = await _context.Tag
                .Where(x => x.Name.Contains(phrase))
                .Select(x => x.Name)
                .Take(10)
                .ToListAsync(); 
            return list;
        }
    }
}