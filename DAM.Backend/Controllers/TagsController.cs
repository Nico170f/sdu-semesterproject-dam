using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.Mvc;
using DAM.Shared.Requests;

namespace DAM.Backend.Controllers;

public class TagsController(ITagService tagService) : ApiController
{

	/*
     * GET /tags
     * Gets tags with optional search parameters
     */
    [HttpGet()]
    public async Task<IActionResult> GetTags([FromQuery] string? searchString = null, [FromQuery] int? amount = null, [FromQuery] int? page = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await tagService.GetTags(searchString, amount, page);
    }
    
    [HttpGet("count")]
    public async Task<IActionResult> GetTagsCount([FromQuery] string? searchString = null, [FromQuery] Guid? assetId = null)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await tagService.GetCountOfTags(searchString, assetId);
    }
    
    
    /*
     * POST /tags
     * Creates a new tag
     */
    [HttpPost()]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await tagService.CreateTag(body);
    }
    
    
    /*
     * DELETE /tags/{tagId}
     * Deletes a tag by ID
     */
    [HttpDelete("{tagId:guid}")]
    public async Task<IActionResult> DeleteTag(Guid tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await tagService.DeleteTag(tagId);
    }

}