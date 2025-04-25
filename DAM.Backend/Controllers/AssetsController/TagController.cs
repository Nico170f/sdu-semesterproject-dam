using DAM.Backend.Controllers.API;
using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Controllers;

public class TagController : ApiController
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetTags()
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.GetTags();
    }

    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImageTag(string imageId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.GetImageTag(imageId);
    }

    [HttpPost("{imageId}")]
    public async Task<IActionResult> AddTagsToImage(string imageId, string tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.AddTagToImage(imageId, tagId);
    }

    [HttpDelete("{imageId}")]
    public async Task<IActionResult> RemoveTagsFromImage(string imageId, string tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.RemoveTagFromImage(imageId, tagId);
    }
}