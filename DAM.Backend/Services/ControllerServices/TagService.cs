using DAM.Backend.Data;
using DAM.Backend.Services.ControllerServices;
using DAM.Shared.Models;
using DAM.Shared.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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


    /// <summary>
	/// Creates a new tag if a tag with the same name does not already exist (case-insensitive).
	/// </summary>
	/// <param name="requestParams">The request parameters containing the tag name.</param>
	/// <returns>
	/// <see cref="BadRequestObjectResult"/> if a tag with the same name exists; 
	/// otherwise, <see cref="OkObjectResult"/> with the created tag;
	/// or <see cref="BadRequestObjectResult"/> if an error occurs.
	/// </returns>
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
    
    /// <summary>
    /// Deletes a tag by its unique identifier.
    /// </summary>
    /// <param name="tagId">The UUID of the tag to delete, as a string.</param>
    /// <returns>
    /// <see cref="NotFoundObjectResult"/> if the tag does not exist;
    /// otherwise, <see cref="OkObjectResult"/> if the tag is deleted successfully;
    /// or <see cref="BadRequestObjectResult"/> if an error occurs.
    /// </returns>
    public async Task<IActionResult> DeleteTag(string tagId)
    {
	    try
	    { 
		    Tag? existingTag = await _database.Tags
			    .FirstOrDefaultAsync(t => t.UUID.ToString() == tagId);
		    
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
		    query = query.Where(t => EF.Functions.Like(t.Name, "%" + searchString + "%") || 
		                             EF.Functions.Like(t.UUID.ToString(), "%" + searchString + "%"));
	    }

	    int tagCount = await query.CountAsync();
	    
	    List<Tag> tags = await query
		    .OrderBy(t => t.Name)
		    .Skip((currentPage - 1) * itemsPerPage)
		    .Take(itemsPerPage)
		    .ToListAsync();
        
	    return new OkObjectResult(tags);
    }
    
    class TagsSearchResponse 
	{
		public List<Tag> Tags { get; set; }
		public int TotalCount { get; set; }
	}
    
    public async Task<IActionResult> GetCountOfTags(string? searchString, string? assetId)
    {
	    // Start with all assets query
	    IQueryable<Tag> query = _database.Tags;

	    // Filter by UUID if searchString is provided
	    if (!string.IsNullOrEmpty(searchString))
	    {
		    query = query.Where(tag => EF.Functions.Like(tag.UUID.ToString(), "%" + searchString + "%"));
	    }

	    if (assetId != null)
	    {
		    query = query
			    .Join(_database.AssetTags, t => t.UUID, at => at.TagUUID, (t, at) => new { Tag = t, AssetTag = at })
			    .Where(joined => !joined.Tag.UUID.ToString().Equals(joined.AssetTag.AssetUUID.ToString()))
			    .Select(joined => joined.Tag);
	    }

	    // Count total matching assets
	    int count = await query.CountAsync();
	    return new OkObjectResult(count);
    }
    
}