using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DAM.Backend.Services.ControllerServices;

public class AssetService : IAssetService
{

    private readonly IConfiguration _configuration;
    private readonly Database _database;

    public AssetService(IConfiguration configuration, Database database)
    {
        _configuration = configuration;
        _database = database;
    }


    public async Task<IActionResult> CreateAsset(CreateAssetRequest body)
    {
        if (body.Content.Length < 30)
        {
            return new BadRequestObjectResult("Asset content is too short");
        }

        Asset asset = new Asset
        {
            UUID = Guid.NewGuid(),
            Content = body.Content,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        (int Width, int Height) dimensions = HelperService.GetAssetDimensions(asset.Content);
        asset.Width = dimensions.Width;
        asset.Height = dimensions.Height;

        _database.Asset.Add(asset);

        int assetCreated = await _database.SaveChangesAsync();
        if (assetCreated <= 0)
        {
            return new BadRequestObjectResult("Failed to create asset");
        }

        CreateAssetResponse response = new CreateAssetResponse(asset);
        return new OkObjectResult(response);
    }


    public async Task<IActionResult> GetAssets(string? searchString, string? selectedTagIds, int? amount, int? page)
    {
	    // Set default values if parameters are null
	    int itemsPerPage = amount ?? 20;
	    int currentPage = page ?? 1;
    
	    // Start with all assets query
	    IQueryable<Asset> query = _database.Asset;
    
	    // Filter by UUID if searchString is provided
	    if (!string.IsNullOrEmpty(searchString))
	    {
		    query = query.Where(img => img.UUID.ToString().Contains(searchString));
	    }
    
	    // Filter by selected tags if provided
	    if (!string.IsNullOrEmpty(selectedTagIds))
	    {
		    // Split the comma-separated string and parse to GUIDs
		    List<Guid> tagUUIDs = selectedTagIds.Split(',')
			    .Select(id => HelperService.ParseStringGuid(id))
			    .Where(guid => guid.HasValue)
			    .Select(guid => guid.Value)
			    .ToList();
        
		    if (tagUUIDs.Any())
		    {
			    // Get assets that have ANY of the specified tags
			    query = query.Where(img => 
				    _database.AssetTags
					    .Any(it => it.AssetUUID == img.UUID && tagUUIDs.Contains(it.TagUUID)));
		    }
	    }
    
	    // Apply pagination (page starts at 1)
	    List<Guid> uuids = await query
		    .OrderBy(img => img.UUID) // Ensure consistent pagination order
		    .Skip((currentPage - 1) * itemsPerPage)
		    .Take(itemsPerPage)
		    .Select(img => img.UUID)
		    .ToListAsync();
    
	    return new OkObjectResult(uuids);
    }


    public async Task<IActionResult> GetAssetById(string assetId, int? width, int? height)
    {
        Asset? finalAsset = null;
        Guid? assetUuid = HelperService.ParseStringGuid(assetId);
        if (assetUuid != null)
        {
            finalAsset = await _database.Asset
                .FirstOrDefaultAsync(i => i.UUID == assetUuid);
        }

        if (finalAsset == null)
        {
            finalAsset = new Asset
            {
                Content = HelperService.DefaultImage
            };
        }

        if (height.HasValue || width.HasValue)
        {
            finalAsset.Content = HelperService.ResizeBase64WithPadding(finalAsset, width, height);
        }

        FileContentResult fileContentResult = HelperService.ConvertAssetToFileContent(finalAsset);
        return fileContentResult;
    }
	
    public async Task<IActionResult> UpdateAsset(string assetId, UpdateAssetRequest requestParams)
    {
        Guid? assetUuid = HelperService.ParseStringGuid(assetId);
        if (assetUuid == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        Asset? asset = await _database.Asset
            .FirstOrDefaultAsync(i => i.UUID == assetUuid);
        if (asset == null)
        {
            return new NotFoundObjectResult("No asset found by that UUID");
        }

        asset.Content = requestParams.Content;
        asset.UpdatedAt = DateTime.Now;

        (int Width, int Height) dimensions = HelperService.GetAssetDimensions(asset.Content);
        asset.Width = dimensions.Width;
        asset.Height = dimensions.Height;

        bool assetUpdated = await _database.Update(asset);
        if (!assetUpdated)
        {
            return new BadRequestObjectResult("Failed to update asset");
        }

        return new OkObjectResult("Asset updated successfully");
    }



    public async Task<IActionResult> PatchAsset(string assetId, JsonPatchDocument<Asset> patchDoc)
    {
        if (patchDoc == null)
        {
            return new BadRequestObjectResult("Patch document cannot be null");
        }

        Guid? assetUuid = HelperService.ParseStringGuid(assetId);
        if (assetUuid == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        Asset? asset = await _database.Asset
            .FirstOrDefaultAsync(i => i.UUID == assetUuid);

        if (asset == null)
        {
            return new NotFoundObjectResult("No asset found by that UUID");
        }

        patchDoc.ApplyTo(asset);
        asset.UpdatedAt = DateTime.Now;

        (int Width, int Height) dimensions = HelperService.GetAssetDimensions(asset.Content);
        asset.Width = dimensions.Width;
        asset.Height = dimensions.Height;

        bool updateResult = await _database.SaveChangesAsync() > 0;
        if (!updateResult)
        {
            return new BadRequestObjectResult("Failed to update asset");
        }

        return new OkObjectResult("Asset updated successfully");
    }


    public async Task<IActionResult> DeleteAsset(string assetId)
    {
        Guid? assetUuid = HelperService.ParseStringGuid(assetId);
        if (assetUuid == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        var asset = await _database.Asset.FindAsync(assetUuid);
        if (asset == null)
        {
            return new NotFoundObjectResult("No asset found by that UUID");
        }

        var deleted = await _database.Delete(asset);
        if (!deleted)
        {
            return new BadRequestObjectResult("Failed to delete asset");
        }

        var productAssets = await _database.ProductAssets
            .Where(pi => pi.AssetUUID == assetUuid)
            .ToListAsync();
        if (productAssets.Any())
        {
            foreach (var productAsset in productAssets)
            {
                await _database.Delete(productAsset);
            }
        }

        return new OkObjectResult("Asset deleted successfully");
    }


    public async Task<IActionResult> GetAssetIdPileFromSearch(int size, int offset, string? searchquery)
    {
	    searchquery = searchquery ?? "";
        List<Guid> assetIds = await _database.ProductAssets
            .Join(_database.Products,
                pi => pi.ProductUUID,
                p => p.UUID,
                (pi, p) => new { ProductAsset = pi, Product = p })
            .Where(joined => joined.Product.Name.Contains(searchquery))
            .OrderBy(joined => joined.Product.Name)
            .Skip(offset)
            .Take(size)
            .Select(joined => joined.ProductAsset.AssetUUID)
            .ToListAsync();

        return new OkObjectResult(assetIds);
    }


    public async Task<IActionResult> GetAssetTagsGallery(string assetId, string? searchString, int? amount, int? page)
    {
	    int itemsPerPage = amount ?? 20;
	    int currentPage = page ?? 1;
	    
	    IQueryable<Tag> query = _database.Tags
	        .Where(tag => !_database.AssetTags
	            .Any(it => it.AssetUUID.ToString() == assetId && it.TagUUID == tag.UUID));
	    
	    if (!string.IsNullOrEmpty(searchString))
	    {
	        query = query.Where(tag => tag.Name.Contains(searchString) || tag.UUID.ToString().Contains(searchString));
	    }
	    
	    var tagsNotOnAsset = await query
	        .OrderBy(tag => tag.Name)
	        .Skip((currentPage - 1) * itemsPerPage)
	        .Take(itemsPerPage)
	        .ToListAsync();
	    
	    return new OkObjectResult(tagsNotOnAsset);
    }
    

    public async Task<IActionResult> GetAssetTags(string assetId)
    {
        Guid? assetUUID = HelperService.ParseStringGuid(assetId);
        if (assetUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        List<Tag> assetTagsList = new List<Tag>();

        assetTagsList = await _database.Tags
            .Where(tag => _database.AssetTags
                .Any(it => it.AssetUUID == assetUUID && it.TagUUID == tag.UUID))
            .ToListAsync();

        return new OkObjectResult(assetTagsList);
    }


    public async Task<IActionResult> AddAssetTag(string assetId, string tagId)
    {
        Guid? assetUUID = HelperService.ParseStringGuid(assetId);
        Guid? tagUUID = HelperService.ParseStringGuid(tagId);

        if (assetUUID == null || tagUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        try
        {
            var asset = await _database.Asset.FindAsync(assetUUID);
            if (asset == null)
            {
                return new BadRequestObjectResult("Asset not found");
            }

            var tag = await _database.Tags.FindAsync(tagUUID);
            if (tag == null)
            {
                return new NotFoundObjectResult("Tag not found");
            }

            var existingRelationship = await _database.AssetTags
                .FirstOrDefaultAsync(it => it.AssetUUID == assetUUID && it.TagUUID == tagUUID);
            if (existingRelationship != null)
            {
                return new OkObjectResult("Tag is already associated with asset");
            }

            var assetTag = new AssetTags()
            {
                AssetUUID = (Guid)assetUUID,
                TagUUID = (Guid)tagUUID
            };

            await _database.AssetTags.AddAsync(assetTag);
            await _database.SaveChangesAsync();

            return new OkObjectResult("Tag associated with asset completed");
        }
        catch
        {
            return new BadRequestObjectResult("Failed to associate tag with asset");
        }
    }

    public async Task<IActionResult> RemoveAssetTag(string assetId, string tagId)
    {
        Guid? assetUUID = HelperService.ParseStringGuid(assetId);
        Guid? tagUUID = HelperService.ParseStringGuid(tagId);

        if (assetUUID == null || tagUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        var assetTag = await _database.AssetTags
            .FirstOrDefaultAsync(it => it.AssetUUID == assetUUID && it.TagUUID == tagUUID);

        if (assetTag == null)
        {
            return new NotFoundObjectResult("Tag on asset not found");
        }

        _database.AssetTags.Remove(assetTag);
        await _database.SaveChangesAsync();

        return new OkObjectResult("Tag removed from asset");
    }

    public async Task<IActionResult> GetCountOfAssets(string? searchString, string? selectedTagIds)
    {
	    // Start with all assets query
	    IQueryable<Asset> query = _database.Asset;

	    // Filter by UUID if searchString is provided
	    if (!string.IsNullOrEmpty(searchString))
	    {
		    query = query.Where(img => img.UUID.ToString().Contains(searchString));
	    }

	    // Filter by selected tags if provided
	    if (!string.IsNullOrEmpty(selectedTagIds))
	    {
		    // Split the comma-separated string and parse to GUIDs
		    List<Guid> tagUUIDs = selectedTagIds.Split(',')
			    .Select(id => HelperService.ParseStringGuid(id))
			    .Where(guid => guid.HasValue)
			    .Select(guid => guid.Value)
			    .ToList();

		    if (tagUUIDs.Any())
		    {
			    // Get assets that have ANY of the specified tags
			    query = query.Where(img =>
				    _database.AssetTags
					    .Any(it => it.AssetUUID == img.UUID && tagUUIDs.Contains(it.TagUUID)));
		    }
	    }

	    // Count total matching assets
	    int count = await query.CountAsync();
	    return new OkObjectResult(count);
    }
}