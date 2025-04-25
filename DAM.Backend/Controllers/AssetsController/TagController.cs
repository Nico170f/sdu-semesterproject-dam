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

    [HttpPost("{imageId}/add")]
    public async Task<IActionResult> AddTagsToImage(string imageId, string tag)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.AddTagsToImage(imageId, tag);
    }

    [HttpDelete("{imageId}")]
    public async Task<IActionResult> RemoveTagsFromImage(string imageId, string tag)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.RemoveTagsFromImage(imageId, tag);
    }
}