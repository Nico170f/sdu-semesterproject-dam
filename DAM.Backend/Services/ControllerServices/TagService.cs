using DAM.Backend.Controllers.API;
using DAM.Backend.Data;
using DAM.Backend.Data.Models;
using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DAM.Backend.Services;

public class TagService : ITagService
{
    private readonly Database _database;
    
    public async Task<IActionResult> GetImageTag(string imageId)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(imageId);
        if(imageUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }
        
        var imageTags = await _database.ImageTags
            .Where(it => it.ImageUUID == imageUUID)
            .ToListAsync();
        
        if (imageTags == null || imageTags.Count == 0)
        {
            return new NotFoundObjectResult("No tags found for that UUID");
        }
        
        return new OkObjectResult(imageTags);
    }

    public async Task<IActionResult> GetTags()
    {
        List<Tag> tagList = await _database.Tags.ToListAsync();
        
        return new OkObjectResult(tagList);
    }

    public async Task<IActionResult> AddTagsToImage(string imageId, AddTagsToImageRequest requestParams)
    {
        throw new NotImplementedException();
    }
}