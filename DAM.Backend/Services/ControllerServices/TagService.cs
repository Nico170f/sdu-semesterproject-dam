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
    
    public async Task<IActionResult> GetTags(string? searchString, int? amount, int? page)
    {
	    int itemsPerPage = amount ?? 20;
	    int currentPage = page ?? 1;
    
	    IQueryable<Tag> query = _database.Tags;
    
	    if (!string.IsNullOrEmpty(searchString))
	    {
		    query = query.Where(t => t.Name.Contains(searchString) || 
		                             t.UUID.ToString().Contains(searchString));
	    }
    
	    var tags = await query
		    .OrderBy(t => t.Name)
		    .Skip((currentPage - 1) * itemsPerPage)
		    .Take(itemsPerPage)
		    .ToListAsync();
        
	    return new OkObjectResult(tags);
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
    
    public async Task<IActionResult> GetAssetsTags(GetAssetsTagsRequest query)
    {
        if (!query.TagList.Any())
        {
            return new BadRequestObjectResult("Tag list cannot be null or empty");
        }

        var assetsWithTags = await _database.AssetTags
            .Where(it => query.TagList.Contains(it.TagUUID))
            .GroupBy(it => it.AssetUUID)
            .Where(group => group.Select(it => it.TagUUID).Distinct().Count() == query.TagList.Count)
            .Select(group => group.Key)
            .ToListAsync();

        var assets = await _database.Asset
            .Where(asset => assetsWithTags.Contains(asset.UUID))
            .Select(asset => new Asset
            {
                UUID = asset.UUID,
                Content = asset.Content,
                Width = asset.Width,
                Height = asset.Height,
                CreatedAt = asset.CreatedAt,
                UpdatedAt = asset.UpdatedAt
            })
            .ToListAsync();

        return new OkObjectResult(assets);
    }
}