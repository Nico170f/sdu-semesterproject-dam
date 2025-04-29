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
    
    public async Task<IActionResult> GetAllTags()
    {
        List<Tag> tagList = await _database.Tags.ToListAsync();
        return new OkObjectResult(tagList);
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
    
    public async Task<IActionResult> DeleteTag(string tagId)
    {
        try
        {
            var existingTag = await _database.Tags
                .FirstOrDefaultAsync(t => t.UUID.ToString() == tagId);

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
    
    /*
    
    */

    
    
    /*
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
    */

    /*

    
    */
}