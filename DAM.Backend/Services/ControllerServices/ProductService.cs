using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace DAM.Backend.Services.ControllerServices;

public class ProductService : IProductService
{
    
    private readonly Database _database;
    private readonly IConfiguration _configuration;

    
    public ProductService(IConfiguration configuration, Database database)
    {
        _database = database;
        _configuration = configuration;
    }

    public async Task<IActionResult> GetProducts(string? searchString = null, int? amount = null, int? page = null)
    {
        int itemsPerPage = amount ?? 20;
        int currentPage = page ?? 1;
    
        IQueryable<Product> query = _database.Products;
    
        if (!string.IsNullOrEmpty(searchString))
        {
			query = query.Where(p => p.Name.Contains(searchString) || 
			                         p.UUID.ToString().Contains(searchString));
        }
    
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((currentPage - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToListAsync();
        
        return new OkObjectResult(products);
    }

    public async Task<IActionResult> CreateMockProduct(CreateMockProductRequest body)
    {
        Product mockProduct = new Product
        {
            UUID = Guid.NewGuid(),
            Name = body.Name
        };

        _database.Products.Add(mockProduct);
        
        int productCreated = await _database.SaveChangesAsync();
        if (productCreated <= 0)
        {
            return new BadRequestObjectResult("Failed to create product");
        }

        CreateMockProductResponse response = new CreateMockProductResponse(mockProduct);
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> CreateProduct(CreateProductRequest body)
    {
	    Product product = new Product()
	    {
		    Name = body.Name,
		    UUID = body.UUID
	    };

	    _database.Products.Add(product);
	    
	    int productCreated = await _database.SaveChangesAsync();
	    if (productCreated <= 0)
	    {
		    return new BadRequestObjectResult("Failed to create product");
	    }

	    return new OkObjectResult(product);
    }

    public async Task<IActionResult> GetProduct(string productId)
    {
	    Guid? productUUID = HelperService.ParseStringGuid(productId);
	    if (productUUID == null)
	    {
		    return new BadRequestObjectResult("Invalid product uuid");
	    }

	    Product? product = await _database.Products
		    .Where(p => p.UUID == productUUID)
		    .FirstOrDefaultAsync();

	    if (product is null) return new NotFoundObjectResult("No product found with UUID: " + productId + ".");

	    GetProductResponse response = new GetProductResponse(product.Name, product.UUID);
	    return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetProductAssets(string productId)
    {

        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (productUUID == null)
        {
            return new BadRequestObjectResult("Invalid product uuid");
        }

        List<Guid> assetIds = await _database.ProductAssets
            .Where(i => i.ProductUUID == productUUID)
            .OrderBy(i => i.Priority)
            .Select(i => i.AssetUUID)
            .ToListAsync();

        GetProductAssetsIdsResponse response = new GetProductAssetsIdsResponse(assetIds);
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetProductAssetsAmount(string productId)
    {
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (productUUID == null)
        {
            return new BadRequestObjectResult("Invalid product uuid");
        }

        int assetCount = await _database.ProductAssets
            .Where(i => i.ProductUUID == productUUID)
            .CountAsync();

        GetProductAssetAmountResponse response = new GetProductAssetAmountResponse(assetCount);
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetProductAsset(string productId, string priority)
    {
        int? assetPriority = HelperService.GetAssetPriority(priority);
        Guid? productUUID = HelperService.ParseStringGuid(productId);

        if (assetPriority == null || productUUID == null)
        {
            return new BadRequestObjectResult($"Invalid {(assetPriority == null ? "priority": "product uuid")}");
        }
        

        Asset? finalAsset = null;
        
        try {
            ProductAsset? productAsset = await _database.ProductAssets
                //.Include()
                .Where(i => i.ProductUUID == productUUID && i.Priority == assetPriority)
                .FirstOrDefaultAsync();

            if (productAsset == null) throw new Exception("No asset found by that priority");

            finalAsset = await _database.Asset
                .Where(i => i.UUID == productAsset.AssetUUID)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally 
        {
            if(finalAsset == null)
            {
                finalAsset = GetDefaultAsset();
            }
        }
        return HelperService.ConvertAssetToFileContent(finalAsset);
    }

    public async Task<IActionResult> AssignProductAsset(string productId, AddProductAssetRequest body)
    {
        Guid? assetUUID = HelperService.ParseStringGuid(body.AssetId);
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (assetUUID == null || productUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        Task<Asset?> asset = _database.Asset
            .FirstOrDefaultAsync(i => i.UUID == assetUUID);
        Task<Product?> product = _database.Products
            .FirstOrDefaultAsync(p => p.UUID == productUUID);

        await Task.WhenAll(asset, product);

        if (asset.Result == null || product.Result == null)
        {
            return new NotFoundObjectResult("No asset or product found by that UUID");
        }

        ProductAsset? existingProductAsset = await _database.ProductAssets
            .FirstOrDefaultAsync(pi => pi.AssetUUID == assetUUID && pi.ProductUUID == productUUID);
        if (existingProductAsset != null)
        {
            return new ConflictObjectResult("Asset is already associated with the product");
        }

        int? priority = HelperService.GetAssetPriority(body.Priority);
        if (priority == null || priority < 0)
        {
            return new BadRequestObjectResult("Invalid priority format");
        }
        
        List<ProductAsset>? productAssets = await _database.ProductAssets
            .Where(pi => pi.ProductUUID == productUUID)
            .OrderBy(pi => pi.Priority)
            .ToListAsync();

        priority = Math.Min(Math.Max(priority.Value, 0), productAssets.Count);

        ProductAsset newProductAsset = new ProductAsset
        {
            AssetUUID = assetUUID.Value,
            ProductUUID = productUUID.Value,
            Priority = priority.Value
        };

        foreach (var img in productAssets.Where(pi => pi.Priority >= priority.Value))
        {
            img.Priority += 1;
            _database.ProductAssets.Update(img);
        }

        _database.ProductAssets.Add(newProductAsset);

        int assetCreated = await _database.SaveChangesAsync();
        if (assetCreated <= 0)
        {
            return new BadRequestObjectResult("Failed to add asset to product");
        }

        return new OkObjectResult("Asset added to product successfully");
    }
    
    public async Task<IActionResult> UnassignProductAsset(string productId, string assetId)
    {
        Guid? assetUUID = HelperService.ParseStringGuid(assetId);
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (assetUUID == null || productUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        Task<Asset?> asset = _database.Asset
            .FirstOrDefaultAsync(i => i.UUID == assetUUID);
        Task<Product?> product = _database.Products
            .FirstOrDefaultAsync(p => p.UUID == productUUID);

        await Task.WhenAll(asset, product);

        if (asset.Result == null || product.Result == null)
        {
            return new NotFoundObjectResult("No asset or product found by that UUID");
        }
    
        ProductAsset? productAsset = await _database.ProductAssets
            .FirstOrDefaultAsync(pi => pi.AssetUUID == assetUUID && pi.ProductUUID == productUUID);
        if (productAsset == null)
        {
            return new ConflictObjectResult("Product asset relation does not exist");
        }

        // Store the priority before deletion for reference
        int removedPriority = productAsset.Priority;
    
        var deleted = await _database.Delete(productAsset);
        if(!deleted)
        {
            return new BadRequestObjectResult("Could not delete asset");
        }
    
        List<ProductAsset>? productAssets = await _database.ProductAssets
            .Where(pi => pi.ProductUUID == productUUID)
            .OrderBy(pi => pi.Priority)
            .ToListAsync();
    
        foreach (var img in productAssets.Where(pi => pi.Priority > removedPriority))
        {
            img.Priority -= 1;
            _database.ProductAssets.Update(img);
        }
    
        // Save the priority updates to the database
        await _database.SaveChangesAsync();

        return new OkObjectResult("Asset removed from product successfully");    }

    public async Task<IActionResult> PatchProductAsset(string productId, string assetId, JsonPatchDocument<ProductAsset> patchDoc)
    {
	    Guid? assetUUID = HelperService.ParseStringGuid(assetId);
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (assetUUID == null || productUUID == null)
        {
	        return new BadRequestObjectResult("Invalid UUID format");
        }

        ProductAsset? asset = await _database.ProductAssets
	        .FirstOrDefaultAsync(pi => pi.ProductUUID == productUUID && pi.AssetUUID == assetUUID);

        if (asset == null)
        {
	        return new NotFoundObjectResult($"Asset with ID {assetId} not found");
        }

        int originalPriority = asset.Priority;
        patchDoc.ApplyTo(asset);

        if (asset.Priority != originalPriority)
        {
	        List<ProductAsset> productAssets = await _database.ProductAssets
		        .Where(pi => pi.ProductUUID == productUUID)
		        .OrderBy(pi => pi.Priority)
		        .ToListAsync();

	        // Ensure priority is within valid range
	        asset.Priority = Math.Max(0, Math.Min(asset.Priority, productAssets.Count - 1));

	        // Moving to higher priority (smaller number)
	        if (asset.Priority < originalPriority)
	        {
		        foreach (var img in productAssets.Where(pi => 
			                 pi.Priority >= asset.Priority && 
			                 pi.Priority < originalPriority && 
			                 pi.AssetUUID != assetUUID))
		        {
			        img.Priority += 1;
			        _database.ProductAssets.Update(img);
		        }
	        }
	        // Moving to lower priority (larger number)
	        else if (asset.Priority > originalPriority)
	        {
		        foreach (var img in productAssets.Where(pi => 
			                 pi.Priority > originalPriority && 
			                 pi.Priority <= asset.Priority && 
			                 pi.AssetUUID != assetUUID))
		        {
			        img.Priority -= 1;
			        _database.ProductAssets.Update(img);
		        }
	        }

	        // Update the asset with its new priority
	        _database.ProductAssets.Update(asset);
        
	        // Save all changes to the database
	        await _database.SaveChangesAsync();
        }

        return new OkObjectResult("Asset updated successfully");
    }

    public async Task<IActionResult> GetProductGallery(string productId, string? searchString, string? selectedTagIds, int? amount, int? page)
    {
        int itemsPerPage = amount ?? 20;
        int currentPage = page ?? 1;
        
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (productUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }
        
        IQueryable<Asset> query = _database.Asset
            .Where(i => !_database.ProductAssets
                .Any(pi => pi.AssetUUID == i.UUID && pi.ProductUUID == productUUID));
        
        // Filter by searchString if provided
        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(i => i.UUID.ToString().Contains(searchString));
        }
        
        // Filter by selectedTagIds if provided
        if (!string.IsNullOrEmpty(selectedTagIds))
        {
            var tagGuids = selectedTagIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => HelperService.ParseStringGuid(id))
                .Where(guid => guid.HasValue)
                .Select(guid => guid.Value)
                .ToList();
        
            if (tagGuids.Count > 0)
            {
                query = query.Where(asset =>
                    _database.AssetTags.Any(at => at.AssetUUID == asset.UUID && tagGuids.Contains(at.TagUUID)));
            }
        }
        
        var assets = await query
            .OrderBy(i => i.CreatedAt)
            .Skip((currentPage - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToListAsync();
        
        return new OkObjectResult(assets);
    }
    
    // This method should probably be in the helper service
    private Asset GetDefaultAsset()
    {
        Asset asset = new Asset
        {
            Content = _configuration.GetSection("DefaultImages")["NotFound"] ?? throw new Exception("No default asset found")
        };
        
        return asset;
    }

    public async Task<IActionResult> GetProductsFromPIM ()
    {
	    HttpClientHandler handler = new HttpClientHandler();
	    HttpClient _httpClient = new HttpClient(handler)
	    {
		    BaseAddress = new Uri("http://localhost:5084/")
	    };
	    PimResponse? response = await _httpClient.GetFromJsonAsync<PimResponse>($"api/products/list/?page=0&page-size=9999");

	    List<Product> pimProducts = [];

	    foreach (var item in response?.items ?? [])
	    {
		    pimProducts.Add(new Product()
		    {
			    Name = item.name,
			    UUID = new Guid(item.product_id)
		    });
	    }
	    
	    SyncListWithDatabase<Product>(_database.Products, pimProducts, item => item.UUID, _database);
	    
	    return new OkObjectResult(response);
    }
    
    void SyncListWithDatabase<T>(DbSet<T> dbSet, List<T> newList, Func<T, object> keySelector, DbContext context) where T : class
    {
        var dbItems = dbSet.ToList();
    
        // Delete items not in newList
        var toDelete = dbItems
            .Where(dbItem => !newList.Any(newItem =>
                keySelector(newItem).Equals(keySelector(dbItem))))
            .ToList();
        dbSet.RemoveRange(toDelete);
    
        // Add items in newList not in DB
        var toAdd = newList
            .Where(newItem => !dbItems.Any(dbItem =>
                keySelector(dbItem).Equals(keySelector(newItem))))
            .ToList();
        dbSet.AddRange(toAdd);
    
        context.SaveChanges();
    }

  public async Task<IActionResult> GetAssetResizedByNewWidth(string productId, int priority, int? newWidth)
	{
	    Guid? productUUID = HelperService.ParseStringGuid(productId);
	    if (productUUID == null)
	    {
	        return new BadRequestObjectResult("Invalid product UUID format");
	    }

	    if (newWidth <= 0)
	    {
	        return new BadRequestObjectResult("New width must be greater than zero");
	    }

	    // Log productUUID and priority
	    Console.WriteLine($"ProductUUID: {productUUID}, Priority: {priority}");

	    // Find the product image by product ID and priority
	    ProductAsset? productAsset = await _database.ProductAssets
	        .Where(pi => pi.ProductUUID == productUUID && pi.Priority == priority)
	        .FirstOrDefaultAsync();

	    if (productAsset == null)
	    {
	        Console.WriteLine("No ProductAsset found for the given productId and priority.");
	        return new NotFoundObjectResult("No image found for the given product and priority");
	    }

	    Asset? asset = await _database.Asset
	        .Where(i => i.UUID == productAsset.AssetUUID)
	        .FirstOrDefaultAsync();

	    if (asset == null)
	    {
	        Console.WriteLine("No Asset found for the given AssetUUID.");
	        return new NotFoundObjectResult("Image not found");
	    }

	    // Resize the image
	    string resizedImageContent;
	    try
	    {
	        resizedImageContent = await HelperService.ResizeAssetByNewWidth(asset.Content, (int)newWidth);
	    }
	    catch (Exception ex)
	    {
	        Console.WriteLine($"Error resizing image: {ex.Message}");
	        return new BadRequestObjectResult($"Failed to resize image: {ex.Message}");
	    }

	    var response = new Asset()
	    {
	        Content = resizedImageContent,
	        UUID = asset.UUID,
	        Width = (int)newWidth,
	        Height = (int)(asset.Height * ((double)newWidth / asset.Width))
	    };

	    // Return the resized image as a file
	    FileContentResult fileContentResult = HelperService.ConvertAssetToFileContent(response);
	    return fileContentResult;
	}

    public Task<IActionResult> GetAssetResizedByFactor(string productId, int priority, int scaleFactor)
    {
        throw new NotImplementedException();
    }
    
    public async Task<IActionResult> GetCountOfAssetsNotOnProduct(string? productId, string? searchString, string? selectedTagIds)
    {
	    // Check if productId is valid
	    Guid? productUUID = HelperService.ParseStringGuid(productId);
	    if (productUUID == null)
	    {
		    return new BadRequestObjectResult("Invalid product UUID format");
	    }

	    // Verify the product exists
	    var product = await _database.Products
		    .FirstOrDefaultAsync(p => p.UUID == productUUID);

	    if (product == null)
	    {
		    return new NotFoundObjectResult("Product not found");
	    }

	    // Get the assets already associated with this product
	    var assetsOnProduct = _database.ProductAssets
		    .Where(pa => pa.ProductUUID == productUUID)
		    .Select(pa => pa.AssetUUID);

	    // Start with query for assets not on the product
	    IQueryable<Asset> query = _database.Asset
		    .Where(a => !assetsOnProduct.Contains(a.UUID));

	    // Filter by UUID if searchString is provided
	    if (!string.IsNullOrEmpty(searchString))
	    {
		    query = query.Where(asset => asset.UUID.ToString().Contains(searchString));
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
			    query = query.Where(asset =>
				    _database.AssetTags
					    .Any(it => it.AssetUUID == asset.UUID && tagUUIDs.Contains(it.TagUUID)));
		    }
	    }

	    // Count total matching assets
	    int count = await query.CountAsync();

	    return new OkObjectResult(count);
    }

    public async Task<IActionResult> GetCountOfProducts(string searchString)
    {
	    List<Product> products = new List<Product>();

	    products = await _database.Products.Where(p => p.Name.Contains(searchString) || p.UUID.ToString().Contains(searchString)).ToListAsync();

	    return new OkObjectResult(products.Count);
    }
}