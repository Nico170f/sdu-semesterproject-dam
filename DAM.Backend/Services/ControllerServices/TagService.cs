using DAM.Backend.Data;
using DAM.Backend.Services.ControllerServices;
using DAM.Shared.Models;
using DAM.Shared.Requests;
using DAM.Shared.Responses;
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
		try
		{
			Tag? existingTag = await _database.Tags
				.FirstOrDefaultAsync(t => t.Name.Equals(requestParams.Name));

			if (existingTag is not null)
			{
				return new BadRequestObjectResult("A tag with that name already exists.");
			} 
	    
			var tag = new Tag
			{
				Name = requestParams.Name,
				UUID = Guid.NewGuid()
			};
	    
			await _database.Tags.AddAsync(tag);
			await _database.SaveChangesAsync();
	    
			return new OkObjectResult(tag);
		}
		catch (Exception e)
		{
			return new BadRequestObjectResult("Error occured when adding tag: " + e.Message);
		}
	}

    public async Task<IActionResult> DeleteTag(Guid tagId)
    {
	    try
	    { 
		    Tag? existingTag = await _database.Tags
			    .FirstOrDefaultAsync(t => t.UUID == tagId);
		    
		    if (existingTag is null) 
		    {
			    return new NotFoundObjectResult("A tag with that ID doesn't exist");
		    } 
		    
		    _database.Remove(existingTag);
		    await _database.SaveChangesAsync();
		    
		    return new OkObjectResult("Tag removed successfully");
	    }
	    catch (Exception e)
	    { 
		    return new BadRequestObjectResult("Error occured when deleting tag: " + e.Message);
	    }
    }
    
    
    public async Task<IActionResult> GetTags(string? searchString, int? amount, int? page)
    {
	    int itemsPerPage = amount ?? 20;
	    int currentPage = page ?? 1;
    
	    IQueryable<Tag> query = _database.Tags;
    
	    if (!string.IsNullOrEmpty(searchString))
	    {
		    query = query.Where(tag => EF.Functions.Like(tag.Name, $"%{searchString}%") || 
		                               EF.Functions.Like(tag.UUID.ToString(), $"%{searchString}%"));
	    }

	    Task<int> count = query.CountAsync();
	    
	    Task<List<Tag>> tags = query
		    .OrderBy(t => t.Name)
		    .Skip((currentPage - 1) * itemsPerPage)
		    .Take(itemsPerPage)
		    .ToListAsync();

	    await Task.WhenAll(count, tags);

	    var response = new GetTagsResponse
	    {
		    Tags = tags.Result,
		    TotalCount = count.Result
	    };
        
	    return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetTagsOnAsset(Guid assetId)
    {
	    IQueryable<Tag> query = _database.Tags
		    .Where(tag => _database.AssetTags
			    .Any(assetTag => assetTag.AssetUUID.Equals(assetId) && assetTag.TagUUID.Equals(tag.UUID)));
	    
	    List<Tag> tags = await query.ToListAsync();

	    var response = new GetTagsResponse
	    {
		    Tags = tags
	    };
	    
	    return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetTagsGallery(Guid assetIdToAvoid, string? searchString, int? amount, int? page)
    {
	    int itemsPerPage = amount ?? 20;
	    int currentPage = page ?? 1;
    
	    IQueryable<Tag> query = _database.Tags
		    .Where(tag => !_database.AssetTags
			    .Any(assetTag => assetTag.AssetUUID.Equals(assetIdToAvoid) && assetTag.TagUUID.Equals(tag.UUID)));
    
	    if (!string.IsNullOrEmpty(searchString))
	    {
		    query = query.Where(tag => EF.Functions.Like(tag.Name, $"%{searchString}%") || 
		                             EF.Functions.Like(tag.UUID.ToString(), $"%{searchString}%"));
	    }

	    Task<int> count = query.CountAsync();
	    
	    Task<List<Tag>> tags = query
		    .OrderBy(t => t.Name)
		    .Skip((currentPage - 1) * itemsPerPage)
		    .Take(itemsPerPage)
		    .ToListAsync();

	    await Task.WhenAll(count, tags);

	    var response = new GetTagsResponse
	    {
		    Tags = tags.Result,
		    TotalCount = count.Result
	    };
        
	    return new OkObjectResult(response);
    }
}