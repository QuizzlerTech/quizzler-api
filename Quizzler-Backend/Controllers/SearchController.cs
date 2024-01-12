using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Data;
using Quizzler_Backend.Dtos.Search;

namespace Quizzler_Backend.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly QuizzlerDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly SearchService _searchService;
        public SearchController(QuizzlerDbContext context, IMemoryCache memoryCache, SearchService searchService)
        {
            _context = context;
            _searchService = searchService;
            _memoryCache = memoryCache;
        }


        // GET: api/search/{query}
        // Method to get search results
        [HttpGet("{query}")]
        public async Task<ActionResult<CombinedSearchSendDto>> Search(string query)
        {
            return await _searchService.Search(query, User);
        }
        // GET: api/search/autocomplete/{query}
        // Method to get autocompletion
        [HttpGet("autocomplete/{query}")]
        public async Task<ActionResult<ICollection<String>>> AutoComplete(string query)
        {
            return await _searchService.AutoComplete(query, User);
        }
    }
}

