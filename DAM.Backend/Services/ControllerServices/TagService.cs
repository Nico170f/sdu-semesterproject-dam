using DAM.Backend.Controllers.API;
using DAM.Backend.Data;
using DAM.Backend.Data.Models;
using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DAM.Backend.Services;

public class TagService : ITagService
{
    private readonly IConfiguration _configuration;
    private readonly Database _database;

    public TagService(IConfiguration configuration, Database database)
    {
        _configuration = configuration;
        _database = database;
    }
    
    public async Task<IActionResult> CreateTag(CreateTagRequest requestParams)
    {
        var existingTag = await _database.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == requestParams.Name.ToLower());

        if (existingTag != null)
        {
            return new BadRequestObjectResult("A tag with that name already exists.");
        } 
        
        var tag = new Tag()
        {
            Name = requestParams.Name,
            UUID = Guid.NewGuid()
        };
        
        await _database.Tags.AddAsync(tag);
        await _database.SaveChangesAsync();
        
        return new OkObjectResult(tag);
    }
    
    public async Task<IActionResult> DeleteTag(DeleteTagRequest requestParams)
    {
        try
        {
            var existingTag = await _database.Tags
                .FirstOrDefaultAsync(t => t.UUID.ToString() == requestParams.TagUUID);

            if (existingTag == null)
            {
                return new NotFoundObjectResult("A tag with that ID doesn't exist");
            }
            
            await _database.Delete(existingTag);
            return new OkObjectResult("Tag removed successfully");
        }
        catch
        {
            return new BadRequestObjectResult("Error occured");
        }
    }
    
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

    public async Task<IActionResult> GetAssetsByTags(GetAssetsByTagsRequest requestParams)
    {
        if (requestParams == null || requestParams.tagList == null || !requestParams.tagList.Any())
        {
            return new BadRequestObjectResult("Tag list cannot be null or empty.");
        }

        GetAssetsByTagsResponse responseParams = new GetAssetsByTagsResponse
        {
            imageList = new List<Image>()
        };

        foreach (var tag in requestParams.tagList)
        {
            var tagUUID = tag.UUID;

            // Find images associated with the current tag
            var images = await _database.Images
                .Where(image => _database.ImageTags
                    .Any(it => it.TagUUID == tagUUID && it.ImageUUID == image.UUID))
                .ToListAsync();

            // Add the images to the response list
            responseParams.imageList.AddRange(images);
        }

        // Remove duplicate images (if any)
        responseParams.imageList = responseParams.imageList
            .GroupBy(img => img.UUID)
            .Select(group => group.First())
            .ToList();

        return new OkObjectResult(responseParams);
    }
}