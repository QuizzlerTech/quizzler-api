using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quizzler_Backend.Data;

namespace Quizzler_Backend.Controllers
{
    [Route("api/tag")]
    [ApiController]
    public class TagController(QuizzlerDbContext context) : ControllerBase
    {
        private readonly QuizzlerDbContext _context = context;

        [HttpGet("{phrase}")]
        public async Task<ActionResult<List<string>>> GetTagsBasedOnString(string phrase)
        {
            var list = await _context.Tag
                .Where(x => x.Name.Contains(phrase))
                .Select(x => x.Name)
                .Take(10)
                .ToListAsync();
            return list;
        }
    }
}