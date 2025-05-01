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
    public async Task<IActionResult> GetTags()
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.GetTags();
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
    public async Task<IActionResult> DeleteTag(DeleteTagRequest requestParams)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.DeleteTag(requestParams);
    }
    
    [HttpGet("{imageId}/get")]
    public async Task<IActionResult> GetImageTag(string imageId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.GetImageTag(imageId);
    }
    
    [HttpGet("{imageId}/getexcluded")]
    public async Task<IActionResult> GetTagsNotOnImage(string imageId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.GetTagsNotOnImage(imageId);
    }

    [HttpPost("{imageId}/add")]
    public async Task<IActionResult> AddTagsToImage(string imageId, string tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.AddTagToImage(imageId, tagId);
    }

    [HttpDelete("{imageId}/deletefromimage")]
    public async Task<IActionResult> RemoveTagsFromImage(string imageId, string tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.RemoveTagFromImage(imageId, tagId);
    }

    public async Task<IActionResult> GetAssetsByTag(GetAssetsByTagsRequest requestParams)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.GetAssetsByTags(requestParams);
    }
}