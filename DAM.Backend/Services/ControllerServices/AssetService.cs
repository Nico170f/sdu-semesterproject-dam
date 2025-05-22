using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using DAM.Shared.Models;
using DAM.Shared.Requests;
using DAM.Shared.Responses;

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

        var response = new CreateAssetResponse
        {
	        AssetId = asset.UUID
        };
        
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> DeleteAsset(Guid assetId)
    {
	    Asset? asset = await _database.Asset.FindAsync(assetId);
	    if (asset is null)
	    {
		    return new NotFoundObjectResult("No asset found by that UUID");
	    }

	    bool deleted = await _database.Delete(asset);
	    if (!deleted)
	    {
		    return new BadRequestObjectResult("Failed to delete asset");
	    }

	    List<ProductAsset> productAssets = await _database.ProductAssets
		    .Where(pi => pi.AssetUUID == assetId)
		    .ToListAsync();
        
	    foreach (ProductAsset productAsset in productAssets)
	    {
		    await _database.Delete(productAsset);
	    }

	    return new OkObjectResult("Asset deleted successfully");
    }
    

    public async Task<IActionResult> GetAssets(string? searchString, string? selectedTags, int? amount, int? page)
    {
	    int itemsPerPage = amount ?? 20;
	    int currentPage = page ?? 1;
    
	    IQueryable<Asset> query = _database.Asset;
    
	    if (!string.IsNullOrEmpty(searchString))
	    {
		    query = query.Where(asset => EF.Functions.Like(asset.UUID.ToString(), $"%{searchString}%"));
	    }

	    if (!string.IsNullOrEmpty(selectedTags))
	    {
		    HashSet<Guid> tagIds = new HashSet<Guid>(selectedTags.Split(',').Select(id => new Guid(id)));

		    query = query.Where(asset => _database.AssetTags
			    .Any(assetTag => assetTag.AssetUUID.Equals(asset.UUID) && tagIds.Contains(assetTag.TagUUID)));
	    }
	    
	    Task<int> count = query.CountAsync();
	    
	    Task<List<Asset>> assets = query
		    .OrderByDescending(asset => asset.CreatedAt)
		    .Skip((currentPage - 1) * itemsPerPage)
		    .Take(itemsPerPage)
		    .ToListAsync();

	    await Task.WhenAll(count, assets);

	    var response = new GetAssetsResponse()
	    {
		    Assets = assets.Result,
		    TotalCount = count.Result
	    };
        
	    return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetAssetsOnProduct(Guid productId)
    {
	    IQueryable<Asset> query = _database.Asset
		    .Where(asset => _database.ProductAssets
			    .Any(productAsset => productAsset.ProductUUID.Equals(productId) && productAsset.AssetUUID.Equals(asset.UUID)));
	    
	    List<Asset> assets = await query.ToListAsync();

	    var response = new GetAssetsResponse()
	    {
		    Assets = assets
	    };
	    
	    return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetAssetsGallery(Guid productIdToAvoid, string? searchString, string? selectedTags, int? amount, int? page)
    {
	    int itemsPerPage = amount ?? 20;
	    int currentPage = page ?? 1;
    
	    IQueryable<Asset> query = _database.Asset
		    .Where(asset => !_database.ProductAssets
			    .Any(productAsset => productAsset.ProductUUID.Equals(productIdToAvoid) && productAsset.AssetUUID.Equals(asset.UUID)));
    
	    if (!string.IsNullOrEmpty(searchString))
	    {
		    query = query.Where(asset => EF.Functions.Like(asset.UUID.ToString(), $"%{searchString}%"));
	    }

	    if (!string.IsNullOrEmpty(selectedTags))
	    {
		    HashSet<Guid> tagIds = new HashSet<Guid>(selectedTags.Split(',').Select(id => new Guid(id)));

		    query = query.Where(asset => _database.AssetTags
			    .Any(assetTag => assetTag.AssetUUID.Equals(asset.UUID) && tagIds.Contains(assetTag.TagUUID)));
	    }
	    
	    Task<int> count = query.CountAsync();
	    
	    Task<List<Asset>> assets = query
		    .OrderByDescending(asset => asset.CreatedAt)
		    .Skip((currentPage - 1) * itemsPerPage)
		    .Take(itemsPerPage)
		    .ToListAsync();

	    await Task.WhenAll(count, assets);

	    var response = new GetAssetsResponse()
	    {
		    Assets = assets.Result,
		    TotalCount = count.Result
	    };
        
	    return new OkObjectResult(response);
    }
    
    
    public async Task<IActionResult> GetAssetContent(Guid assetId, int? width, int? height)
    {
        Asset finalAsset = await _database.Asset
	        .FirstOrDefaultAsync(i => i.UUID == assetId) ?? new Asset
        {
	        Content = HelperService.DefaultImage
        };

        if (height.HasValue || width.HasValue)
        {
            finalAsset.Content = HelperService.ResizeBase64WithPadding(finalAsset, width, height);
        }

        FileContentResult fileContentResult = HelperService.ConvertAssetToFileContent(finalAsset);
        return fileContentResult;
    }
    
    
    public async Task<IActionResult> AddAssetTag(Guid assetId, Guid tagId)
    {
        try
        {
            var asset = await _database.Asset.FindAsync(assetId);
            if (asset == null)
            {
                return new BadRequestObjectResult("Asset not found");
            }

            var tag = await _database.Tags.FindAsync(tagId);
            if (tag == null)
            {
                return new NotFoundObjectResult("Tag not found");
            }

            var existingRelationship = await _database.AssetTags
                .FirstOrDefaultAsync(it => it.AssetUUID == assetId && it.TagUUID == tagId);
            if (existingRelationship != null)
            {
                return new OkObjectResult("Tag is already associated with asset");
            }

            var assetTag = new AssetTags()
            {
                AssetUUID = assetId,
                TagUUID = tagId
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

    public async Task<IActionResult> RemoveAssetTag(Guid assetId, Guid tagId)
    {
        var assetTag = await _database.AssetTags
            .FirstOrDefaultAsync(it => it.AssetUUID == assetId && it.TagUUID == tagId);

        if (assetTag == null)
        {
            return new NotFoundObjectResult("Tag on asset not found");
        }

        _database.AssetTags.Remove(assetTag);
        await _database.SaveChangesAsync();

        return new OkObjectResult("Tag removed from asset");
    }
    
	
    public async Task<IActionResult> UpdateAsset(Guid assetId, UpdateAssetRequest requestParams)
    {
	    Asset? asset = await _database.Asset
		    .FirstOrDefaultAsync(i => i.UUID == assetId);
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
    
    public async Task<IActionResult> PatchAsset(Guid assetId, JsonPatchDocument<Asset> patchDoc)
    {
	    Asset? asset = await _database.Asset
		    .FirstOrDefaultAsync(i => i.UUID == assetId);

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
}