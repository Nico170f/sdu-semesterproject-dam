using DAM.Backend.Controllers.API;
using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data.Models;

namespace DAM.Backend.Controllers;

public class TagsController : ApiController
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /*
     * GET /tags
     * Gets tags with optional search parameters
     */
    [HttpGet()]
    public async Task<IActionResult> GetTags([FromQuery] string? searchString = null, [FromQuery] int? amount = null, [FromQuery] int? page = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.GetTags(searchString, amount, page);
    }
    
    [HttpGet("count")]
    public async Task<IActionResult> GetTagsCount([FromQuery] string? searchString = null, [FromQuery] string? assetId = null)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await _tagService.GetCountOfTags(searchString, assetId);
    }
    
    
    /*
     * POST /tags
     * Creates a new tag
     */
    [HttpPost()]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.CreateTag(body);
    }
    
    
    /*
     * DELETE /tags/{tagId}
     * Deletes a tag by ID
     */
    [HttpDelete("{tagId}")]
    public async Task<IActionResult> DeleteTag(string tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.DeleteTag(tagId);
    }

    /*
     * GET tags/search
     * Gets all assets associated with tagList
     * 
     */
    [HttpGet("search")]
    public async Task<IActionResult> GetAssetsTags([FromQuery] string tagList)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        GetAssetsTagsRequest query = new GetAssetsTagsRequest
        {
            TagList = tagList.Split(",").Select(Guid.Parse).ToList()
        };
        
        return await _tagService.GetAssetsTags(query);
    }
}