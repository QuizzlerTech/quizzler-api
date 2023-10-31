using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Data;

namespace Quizzler_Backend.Controllers
{
    [Route("api/tag")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly QuizzlerDbContext _context;

        public TagController(QuizzlerDbContext context)
        {
            _context = context;
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