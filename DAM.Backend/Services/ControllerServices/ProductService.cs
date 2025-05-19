using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using DAM.Shared.Models;
using DAM.Shared.Requests;
using DAM.Shared.Responses;
using Microsoft.EntityFrameworkCore;


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

    public async Task<IActionResult> GetProduct(Guid productId)
    {
	    Product? product = await _database.Products
		    .Where(p => p.UUID == productId)
		    .FirstOrDefaultAsync();

	    if (product is null) return new NotFoundObjectResult("No product found with UUID: " + productId + ".");

	    var response = new GetProductResponse
	    {
		    Product = product
	    };
	    
	    return new OkObjectResult(response);
    }
    
    public async Task<IActionResult> GetProducts(string? searchString = null, int? amount = null, int? page = null)
    {
        int itemsPerPage = amount ?? 20;
        int currentPage = page ?? 1;
    
        IQueryable<Product> query = _database.Products;
    
        if (!string.IsNullOrEmpty(searchString))
        {
			query = query.Where(product => 
				EF.Functions.Like(product.Name, $"%{searchString}%") ||
				EF.Functions.Like(product.UUID.ToString(), $"%{searchString}%"));
        }

        Task<int> count = query.CountAsync();
    
        Task<List<Product>> products = query
            .OrderBy(p => p.Name)
            .Skip((currentPage - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToListAsync();

        await Task.WhenAll(count, products);

        var response = new GetProductsResponse
        {
	        Products = products.Result,
	        TotalCount = count.Result
        };
        
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetProductAssets(Guid productId)
    {
        List<Guid> assetIds = await _database.ProductAssets
            .Where(i => i.ProductUUID == productId)
            .OrderBy(i => i.Priority)
            .Select(i => i.AssetUUID)
            .ToListAsync();

        var response = new GetProductAssetsIdsResponse
        {
	        AssetIds = assetIds
        };
        
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetProductAssetsAmount(Guid productId)
    {
        int assetCount = await _database.ProductAssets
            .Where(i => i.ProductUUID == productId)
            .CountAsync();

        var response = new GetProductAssetAmountResponse
        {
	        Amount = assetCount
        };
        
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetProductAsset(Guid productId, int priority, int? width = null, int? height = null)
    {
	    Asset finalAsset = await _database.Asset.Where(asset => _database.ProductAssets.Any(productAsset =>
			    productAsset.ProductUUID.Equals(productId) && productAsset.AssetUUID.Equals(asset.UUID) && productAsset.Priority.Equals(priority)))
		    .FirstOrDefaultAsync() ?? new Asset
	    {
		    Content = HelperService.DefaultImage
	    };

	    if (width.HasValue || height.HasValue)
	    {
		    finalAsset.Content = HelperService.ResizeBase64WithPadding(finalAsset, width, height);
	    }

	    FileContentResult fileContentResult = HelperService.ConvertAssetToFileContent(finalAsset);
	    return fileContentResult;
    }

    public async Task<IActionResult> AssignProductAsset(Guid productId, AddProductAssetRequest body)
    {
        Guid assetUUID = body.AssetId;

        Task<Asset?> asset = _database.Asset
            .FirstOrDefaultAsync(i => i.UUID == assetUUID);
        Task<Product?> product = _database.Products
            .FirstOrDefaultAsync(p => p.UUID == productId);

        await Task.WhenAll(asset, product);

        if (asset.Result == null || product.Result == null)
        {
            return new NotFoundObjectResult("No asset or product found by that UUID");
        }

        ProductAsset? existingProductAsset = await _database.ProductAssets
            .FirstOrDefaultAsync(pi => pi.AssetUUID == assetUUID && pi.ProductUUID == productId);
        if (existingProductAsset != null)
        {
            return new ConflictObjectResult("Asset is already associated with the product");
        }

        int priority = body.Priority;
        if (priority < 0)
        {
            return new BadRequestObjectResult("Invalid priority format");
        }
        
        List<ProductAsset> productAssets = await _database.ProductAssets
            .Where(pi => pi.ProductUUID == productId)
            .OrderBy(pi => pi.Priority)
            .ToListAsync();

        priority = Math.Min(Math.Max(priority, 0), productAssets.Count);

        ProductAsset newProductAsset = new ProductAsset
        {
            AssetUUID = assetUUID,
            ProductUUID = productId,
            Priority = priority
        };

        foreach (var img in productAssets.Where(pi => pi.Priority >= priority))
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
    
    public async Task<IActionResult> UnassignProductAsset(Guid productId, Guid assetId)
    {
        Task<Asset?> asset = _database.Asset
            .FirstOrDefaultAsync(i => i.UUID == assetId);
        Task<Product?> product = _database.Products
            .FirstOrDefaultAsync(p => p.UUID == productId);

        await Task.WhenAll(asset, product);

        if (asset.Result == null || product.Result == null)
        {
            return new NotFoundObjectResult("No asset or product found by that UUID");
        }
    
        ProductAsset? productAsset = await _database.ProductAssets
            .FirstOrDefaultAsync(pi => pi.AssetUUID == assetId && pi.ProductUUID == productId);
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
    
        List<ProductAsset> productAssets = await _database.ProductAssets
            .Where(pi => pi.ProductUUID == productId)
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

    public async Task<IActionResult> PatchProductAsset(Guid productId, Guid assetId, JsonPatchDocument<ProductAsset> patchDoc)
    {
        ProductAsset? asset = await _database.ProductAssets
	        .FirstOrDefaultAsync(pi => pi.ProductUUID == productId && pi.AssetUUID == assetId);

        if (asset == null)
        {
	        return new NotFoundObjectResult($"Asset with ID {assetId} not found");
        }

        int originalPriority = asset.Priority;
        patchDoc.ApplyTo(asset);

        if (asset.Priority != originalPriority)
        {
	        List<ProductAsset> productAssets = await _database.ProductAssets
		        .Where(pi => pi.ProductUUID == productId)
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
			                 pi.AssetUUID != assetId))
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
			                 pi.AssetUUID != assetId))
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

	    CreateMockProductResponse response = new CreateMockProductResponse
	    {
		    ProductId = mockProduct.UUID
	    };
	    return new OkObjectResult(response);
    }

    public async Task<IActionResult> CreateProduct(CreateProductRequest body)
    {
	    Product product = new Product()
	    {
		    Name = body.Name,
		    UUID = body.ProductId
	    };

	    _database.Products.Add(product);
	    
	    int productCreated = await _database.SaveChangesAsync();
	    if (productCreated <= 0)
	    {
		    return new BadRequestObjectResult("Failed to create product");
	    }

	    return new OkObjectResult(product);
    }
}