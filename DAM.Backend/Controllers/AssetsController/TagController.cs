using DAM.Backend.Controllers.API;
using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data.Models;

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

    [HttpPost("{tagId}/create")]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest requestParams)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.CreateTag(requestParams);
    }

    [HttpDelete("{tagId}/delete")]
    public async Task<IActionResult> DeleteTag([FromBody] DeleteTagRequest requestParams)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _tagService.DeleteTag(requestParams);
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
        return await _tagService.GetAssetsBytag()
    }
    
}