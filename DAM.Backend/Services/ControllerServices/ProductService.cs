using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
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

    public async Task<IActionResult> GetAllProducts ()
    {
	    var response = await _database.Products.ToListAsync();
	    return new OkObjectResult(response);
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

        List<Guid> imageIds = await _database.ProductImages
            .Where(i => i.ProductUUID == productUUID)
            .OrderBy(i => i.Priority)
            .Select(i => i.ImageUUID)
            .ToListAsync();

        GetProductAssetsIdsResponse response = new GetProductAssetsIdsResponse(imageIds);
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetProductAssetsAmount(string productId)
    {
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (productUUID == null)
        {
            return new BadRequestObjectResult("Invalid product uuid");
        }

        int imageCount = await _database.ProductImages
            .Where(i => i.ProductUUID == productUUID)
            .CountAsync();

        GetProductAssetAmountResponse response = new GetProductAssetAmountResponse(imageCount);
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> GetProductAsset(string productId, string priority)
    {
        int? imagePriority = HelperService.GetImagePriority(priority);
        Guid? productUUID = HelperService.ParseStringGuid(productId);

        if (imagePriority == null || productUUID == null)
        {
            return new BadRequestObjectResult($"Invalid {(imagePriority == null ? "priority": "product uuid")}");
        }
        

        Image? finalImage = null;
        
        try {
            ProductImage? productImage = await _database.ProductImages
                //.Include()
                .Where(i => i.ProductUUID == productUUID && i.Priority == imagePriority)
                .FirstOrDefaultAsync();

            if (productImage == null) throw new Exception("No image found by that priority");

            finalImage = await _database.Images
                .Where(i => i.UUID == productImage.ImageUUID)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally 
        {
            if(finalImage == null)
            {
                finalImage = GetDefaultImage();
            }
        }
        return HelperService.ConvertImageToFileContent(finalImage);
    }

    public async Task<IActionResult> AssignProductAsset(string productId, AddProductImageRequest body)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(body.ImageId);
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (imageUUID == null || productUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        Task<Image?> image = _database.Images
            .FirstOrDefaultAsync(i => i.UUID == imageUUID);
        Task<Product?> product = _database.Products
            .FirstOrDefaultAsync(p => p.UUID == productUUID);

        await Task.WhenAll(image, product);

        if (image.Result == null || product.Result == null)
        {
            return new NotFoundObjectResult("No image or product found by that UUID");
        }

        ProductImage? existingProductImage = await _database.ProductImages
            .FirstOrDefaultAsync(pi => pi.ImageUUID == imageUUID && pi.ProductUUID == productUUID);
        if (existingProductImage != null)
        {
            return new ConflictObjectResult("Image is already associated with the product");
        }

        int? priority = HelperService.GetImagePriority(body.Priority);
        if (priority == null || priority < 0)
        {
            return new BadRequestObjectResult("Invalid priority format");
        }
        
        List<ProductImage>? productImages = await _database.ProductImages
            .Where(pi => pi.ProductUUID == productUUID)
            .OrderBy(pi => pi.Priority)
            .ToListAsync();

        priority = Math.Min(Math.Max(priority.Value, 0), productImages.Count);

        ProductImage newProductImage = new ProductImage
        {
            ImageUUID = imageUUID.Value,
            ProductUUID = productUUID.Value,
            Priority = priority.Value
        };

        foreach (var img in productImages.Where(pi => pi.Priority >= priority.Value))
        {
            img.Priority += 1;
            _database.ProductImages.Update(img);
        }

        _database.ProductImages.Add(newProductImage);

        int imageCreated = await _database.SaveChangesAsync();
        if (imageCreated <= 0)
        {
            return new BadRequestObjectResult("Failed to add image to product");
        }

        return new OkObjectResult("Image added to product successfully");
    }
    
    public async Task<IActionResult> UnassignProductAsset(string productId, string imageId)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(imageId);
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (imageUUID == null || productUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        Task<Image?> image = _database.Images
            .FirstOrDefaultAsync(i => i.UUID == imageUUID);
        Task<Product?> product = _database.Products
            .FirstOrDefaultAsync(p => p.UUID == productUUID);

        await Task.WhenAll(image, product);

        if (image.Result == null || product.Result == null)
        {
            return new NotFoundObjectResult("No image or product found by that UUID");
        }
    
        ProductImage? productImage = await _database.ProductImages
            .FirstOrDefaultAsync(pi => pi.ImageUUID == imageUUID && pi.ProductUUID == productUUID);
        if (productImage == null)
        {
            return new ConflictObjectResult("Product image relation does not exist");
        }

        // Store the priority before deletion for reference
        int removedPriority = productImage.Priority;
    
        var deleted = await _database.Delete(productImage);
        if(!deleted)
        {
            return new BadRequestObjectResult("Could not delete image");
        }
    
        List<ProductImage>? productImages = await _database.ProductImages
            .Where(pi => pi.ProductUUID == productUUID)
            .OrderBy(pi => pi.Priority)
            .ToListAsync();
    
        foreach (var img in productImages.Where(pi => pi.Priority > removedPriority))
        {
            img.Priority -= 1;
            _database.ProductImages.Update(img);
        }
    
        // Save the priority updates to the database
        await _database.SaveChangesAsync();

        return new OkObjectResult("Image removed from product successfully");    }

    public async Task<IActionResult> PatchProductAsset(string productId, string assetId, JsonPatchDocument<ProductImage> patchDoc)
    {
        if (patchDoc == null)
        {
	        return new BadRequestObjectResult("Patch document cannot be null");
        }

        Guid? imageUUID = HelperService.ParseStringGuid(assetId);
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (imageUUID == null || productUUID == null)
        {
	        return new BadRequestObjectResult("Invalid UUID format");
        }

        ProductImage? image = await _database.ProductImages
	        .FirstOrDefaultAsync(pi => pi.ProductUUID == productUUID && pi.ImageUUID == imageUUID);

        if (image == null)
        {
	        return new NotFoundObjectResult($"Image with ID {assetId} not found");
        }

        int originalPriority = image.Priority;
        patchDoc.ApplyTo(image);

        if (image.Priority != originalPriority)
        {
	        List<ProductImage> productImages = await _database.ProductImages
		        .Where(pi => pi.ProductUUID == productUUID)
		        .OrderBy(pi => pi.Priority)
		        .ToListAsync();

	        // Ensure priority is within valid range
	        image.Priority = Math.Max(0, Math.Min(image.Priority, productImages.Count - 1));

	        // Moving to higher priority (smaller number)
	        if (image.Priority < originalPriority)
	        {
		        foreach (var img in productImages.Where(pi => 
			                 pi.Priority >= image.Priority && 
			                 pi.Priority < originalPriority && 
			                 pi.ImageUUID != imageUUID))
		        {
			        img.Priority += 1;
			        _database.ProductImages.Update(img);
		        }
	        }
	        // Moving to lower priority (larger number)
	        else if (image.Priority > originalPriority)
	        {
		        foreach (var img in productImages.Where(pi => 
			                 pi.Priority > originalPriority && 
			                 pi.Priority <= image.Priority && 
			                 pi.ImageUUID != imageUUID))
		        {
			        img.Priority -= 1;
			        _database.ProductImages.Update(img);
		        }
	        }

	        // Update the image with its new priority
	        _database.ProductImages.Update(image);
        
	        // Save all changes to the database
	        await _database.SaveChangesAsync();
        }

        return new OkObjectResult("Image updated successfully");
    }

    public async Task<IActionResult> GetProductGallery(string productId)
    {
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (productUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        List<Image> images = await _database.Images
            .Where(i => !_database.ProductImages
                .Any(pi => pi.ImageUUID == i.UUID && pi.ProductUUID == productUUID))
            .ToListAsync();
        if (images == null || images.Count == 0)
        {
            return new NotFoundObjectResult("No images found");
        }
        images = images.OrderBy(i => i.CreatedAt).ToList();
        
        return new OkObjectResult(images);
    }
    
    
    
    // This method should probably be in the helper service
    private Image GetDefaultImage()
    {
        Image image = new Image
        {
            Content = _configuration.GetSection("DefaultImages")["NotFound"] ?? throw new Exception("No default image found")
        };
        
        return image;
    }
}