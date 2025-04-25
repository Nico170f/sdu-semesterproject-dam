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
        
        List<Tag> imageTagsList = new List<Tag>();
       
        imageTagsList = await _database.Tags
            .Where(tag => _database.ImageTags
                .Any(it => it.ImageUUID == imageUUID && it.TagUUID == tag.UUID))
            .ToListAsync();
        
        if (imageTagsList == null || imageTagsList.Count == 0)
        {
            return new NotFoundObjectResult("No tags found for that UUID");
        }
        
        return new OkObjectResult(imageTagsList);
    }

    public async Task<IActionResult> GetTags()
    {
        List<Tag> tagList = await _database.Tags.ToListAsync();
        
        return new OkObjectResult(tagList);
    }
    
    public async Task<IActionResult> GetTagsNotOnImage(string imageUUID)
    {
        Guid ImageUUID = HelperService.ParseStringGuid(imageUUID).Value;
        
        List<Tag> tagsNotOnImage = new List<Tag>();
     
        tagsNotOnImage = await _database.Tags
            .Where(tag => !_database.ImageTags
                .Any(it => it.ImageUUID == ImageUUID && it.TagUUID == tag.UUID))
            .ToListAsync();
    
        return new OkObjectResult(tagsNotOnImage);
    }

    public async Task<IActionResult> AddTagToImage(string imageId, string tagId)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(imageId);
        Guid? tagUUID = HelperService.ParseStringGuid(tagId);

        if (imageUUID == null || tagUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        try
        {
            var image = await _database.Images.FindAsync(imageUUID);
            if (image == null)
            {
                return new BadRequestObjectResult("Image not found");
            }
            
            var tag = await _database.Tags.FindAsync(tagUUID);
            if (tag == null)
            {
                return new NotFoundObjectResult("Tag not found");
            }

            var existingRelationship = await _database.ImageTags
                .FirstOrDefaultAsync(it => it.ImageUUID == imageUUID && it.TagUUID == tagUUID);
            if (existingRelationship != null)
            {
                return new OkObjectResult("Tag is already associated with image");
            }

            var imageTag = new ImageTags()
            {
                ImageUUID = (Guid)imageUUID,
                TagUUID = (Guid)tagUUID
            };
            
            await _database.ImageTags.AddAsync(imageTag);
            await _database.SaveChangesAsync();
            
            return new OkObjectResult("Tag associated with image completed");
        }
        catch
        {
            return new BadRequestObjectResult("Bacons mom");
        }
    }
    
    public async Task<IActionResult> RemoveTagFromImage(string imageId, string tagId)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(imageId);
        Guid? tagUUID = HelperService.ParseStringGuid(tagId);

        if (imageUUID == null || tagUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        var imageTag = await _database.ImageTags
            .FirstOrDefaultAsync(it => it.ImageUUID == imageUUID && it.TagUUID == tagUUID);
        
        if (imageTag == null)
        {
            return new NotFoundObjectResult("Tag on image not found");
        }

        _database.ImageTags.Remove(imageTag);
        await _database.SaveChangesAsync();
        
        return new OkObjectResult("Tag removed from image");
    }
}