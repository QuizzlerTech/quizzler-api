using Microsoft.AspNetCore.Mvc;
using Quizzler_Backend.Dtos.Search;
using Quizzler_Backend.Services.SearchServices;

namespace Quizzler_Backend.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController(SearchService searchService) : ControllerBase
    {
        private readonly SearchService _searchService = searchService;

        [HttpGet("{query}")]
        public async Task<ActionResult<CombinedSearchSendDto>> Search(string query)
        {
            return await _searchService.Search(query, User);
        }
        [HttpGet("autocomplete/{query}")]
        public async Task<ActionResult<ICollection<string>>> AutoComplete(string query)
        {
            return await _searchService.AutoComplete(query, User);
        }
    }
}

