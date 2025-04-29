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
     * Gets all tags
     */
    [HttpGet()]
    public async Task<IActionResult> GeAllTags()
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.GetTags();
    }
    
    
    /*
     * POST /tags
     * Creates a new tag
     */
    [HttpPost("{tagId}")]
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
}