using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.Mvc;
using DAM.Shared.Requests;

namespace DAM.Backend.Controllers;

public class TagsController(ITagService tagService) : ApiController
{
    
    /*
     * POST /tags
     * Creates a new tag
     */
    [HttpPost]
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
    
    /*
     * GET /tags
     * Gets tags
     */
    [HttpGet]
    public async Task<IActionResult> GetTags(
	    [FromQuery] Guid? assetIdParent = null,
	    [FromQuery] Guid? assetIdToAvoid = null,
	    [FromQuery] string? searchString = null, 
	    [FromQuery] int? amount = null, 
	    [FromQuery] int? page = null)
    {
	    if (!ModelState.IsValid) 
		    return BadRequest(ModelState);

	    if (assetIdParent is not null)
		    return await tagService.GetTagsOnAsset(assetIdParent.Value);
	    
	    if (assetIdToAvoid is not null) 
		    return await tagService.GetTagsGallery(assetIdToAvoid.Value, searchString, amount, page);
	    
	    return await tagService.GetTags(searchString, amount, page);
    }
}