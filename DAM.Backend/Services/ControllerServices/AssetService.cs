using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Image = DAM.Backend.Data.Models.Image;
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

    //Method for returning all assets for a product by productId
    public async Task<IActionResult> GetProductAssetsIds(string productId)
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


    public async Task<IActionResult> GetProductAssetAmount(string productId)
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



    //Method for returning a single image by productId and priority
    public async Task<IActionResult> GetProductImage(string productId, string priority)
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

    //Method for creating a new image
    public async Task<IActionResult> CreateImage(CreateImageRequest requestParams)
    {
        if (requestParams.Content.Length < 30)
        {
            return new BadRequestObjectResult("Image content is too short");
        }
        
        Image image = new Image
        {
            UUID = Guid.NewGuid(),
            Content = requestParams.Content,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        (int Width, int Height) dimensions = HelperService.GetImageDimensions(image.Content);
        image.Width = dimensions.Width;
        image.Height = dimensions.Height;

        _database.Images.Add(image);
        
        int imageCreated = await _database.SaveChangesAsync();
        if (imageCreated <= 0)
        {
            return new BadRequestObjectResult("Failed to create image");
        }

        CreateImageResponse response = new CreateImageResponse(image);
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> UpdateImage(string imageId, UpdateImageRequest requestParams)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(imageId);
        if (imageUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }
        
        Image? image = await _database.Images
            .FirstOrDefaultAsync(i => i.UUID == imageUUID);
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }
        

        image.Content = requestParams.Content;
        image.UpdatedAt = DateTime.Now;

        (int Width, int Height) dimensions = HelperService.GetImageDimensions(image.Content);
        image.Width = dimensions.Width;
        image.Height = dimensions.Height;

        bool imageUpdated = await _database.Update(image);
        if (!imageUpdated)
        {
            return new BadRequestObjectResult("Failed to update image");
        }

        return new OkObjectResult("Image updated successfully");
    }

    
    public async Task<IActionResult> PatchImage(string imageId, JsonPatchDocument<Image> patchDoc)
    {
        if (patchDoc == null){
            return new BadRequestObjectResult("Patch document cannot be null");
        }
        
        Guid? imageUUID = HelperService.ParseStringGuid(imageId);
        if (imageUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        Image? image = await _database.Images
            .FirstOrDefaultAsync(i => i.UUID == imageUUID);

        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }

        patchDoc.ApplyTo(image);
        image.UpdatedAt = DateTime.Now;

        (int Width, int Height) dimensions = HelperService.GetImageDimensions(image.Content);
        image.Width = dimensions.Width;
        image.Height = dimensions.Height;

        bool updateResult = await _database.SaveChangesAsync() > 0;
        if (!updateResult)
        {
            return new BadRequestObjectResult("Failed to update image");
        }

        return new OkObjectResult("Image updated successfully");
    }
    
    
    
    public async Task<IActionResult> PatchProductImage(string productId, string imageId, JsonPatchDocument<ProductImage> patchDocument)
    {
        if (patchDocument == null)
        {
            return new BadRequestObjectResult("Patch document cannot be null");
        }


        Guid? imageUUID = HelperService.ParseStringGuid(imageId);
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (imageUUID == null || productUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

		ProductImage? image = await _database.ProductImages
		.FirstOrDefaultAsync(pi => pi.ProductUUID == productUUID && pi.ImageUUID == imageUUID);

        if (image == null)
        {
            return new NotFoundObjectResult($"Image with ID {imageId} not found");
        }

        int originalPriority = image.Priority;
        patchDocument.ApplyTo(image);

        if(image.Priority != originalPriority)
        {
            // Get all other images for the same product
            var otherImages = await _database.ProductImages
                .Where(pi => pi.ProductUUID == productUUID && pi.ImageUUID != imageUUID)
                .OrderBy(pi => pi.Priority)
                .ToListAsync();

            // Insert image at new priority and reorder all images
            var allImages = new List<ProductImage>(otherImages);
            int insertPosition = Math.Min(image.Priority, allImages.Count);
            allImages.Insert(insertPosition, image);

            // Reassign priorities sequentially
            for (int i = 0; i < allImages.Count; i++)
            {
                allImages[i].Priority = i;
            }

            bool updateResult = await _database.SaveChangesAsync() > 0;
            if (!updateResult)
            {
                return new ConflictObjectResult("Failed to update image priorities");
            }
        }

        return new OkObjectResult("Image updated successfully");
    }


    public async Task<IActionResult> DeleteProductImage(string productId, string imageId)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(imageId);
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (imageUUID == null || productUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        ProductImage? image = await _database.ProductImages
            .FirstOrDefaultAsync(i => i.ImageUUID == imageUUID && i.ProductUUID == productUUID);
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }

        var deleted = await _database.Delete(image);
        if(!deleted)
        {
            return new BadRequestObjectResult("Failed to delete image");
        }

        return new OkObjectResult("Image deleted successfully");
    }

    public async Task<IActionResult> AddProductImage(string productId, AddProductImageRequest request)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(request.ImageId);
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
        
        int? priority = HelperService.GetImagePriority(request.Priority);
        if (priority == null)
        {
            return new BadRequestObjectResult("Invalid priority format");
        }
        
        ProductImage newProductImage = new ProductImage
        {
            ImageUUID = imageUUID.Value,
            ProductUUID = productUUID.Value,
            Priority = (int)priority,
        };
        
        _database.ProductImages.Add(newProductImage);
        int imageCreated = await _database.SaveChangesAsync();
        if (imageCreated <= 0)
        {
            return new BadRequestObjectResult("Failed to add image to product");
        }

        return new OkObjectResult("Image added to product successfully");
    }
    
    // private async UpdateImagePriority(Guid productUUID, Guid imageUUID, int newPriority)
    // {
    //     var productImage = await _database.ProductImages
    //         .FirstOrDefaultAsync(pi => pi.ProductUUID == productUUID && pi.ImageUUID == imageUUID);
    //
    //     if (productImage != null)
    //     {
    //         productImage.Priority = newPriority;
    //         await _database.SaveChangesAsync();
    //     }
    // }
    
    public async Task<IActionResult> RemoveProductImage(string productId, RemoveProductImageRequest request)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(request.ImageId);
        Guid? productUUID = HelperService.ParseStringGuid(productId);
        if (imageUUID == null || productUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }
        
        Image? image = await _database.Images
            .FirstOrDefaultAsync(i => i.UUID == imageUUID);
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }
        
        ProductImage? newImage = await _database.ProductImages
            .FirstOrDefaultAsync(i => i.ImageUUID == imageUUID && i.ProductUUID == productUUID);
        if (newImage == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }
        
        var deleted = await _database.Delete(newImage);
        
        if(!deleted)
        {
            return new BadRequestObjectResult("Could not delete image");
        }
        
        return new OkObjectResult("Image deleted succesfully");
    }
    
    public async Task<IActionResult> DeleteImage(string imageId)
    {

        Guid? imageUUID = HelperService.ParseStringGuid(imageId);
        if (imageUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        var image = await _database.Images.FindAsync(imageUUID);
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }

        var deleted = await _database.Delete(image);
        if(!deleted)
        {
            return new BadRequestObjectResult("Failed to delete image");
        }

        var productImages = await _database.ProductImages
            .Where(pi => pi.ImageUUID == imageUUID)
            .ToListAsync();
        if (productImages.Count > 0)
        {
            foreach (var productImage in productImages)
            {
                await _database.Delete(productImage);
            }
        }

        return new OkObjectResult("Image deleted successfully");
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
    
    public async Task<IActionResult> GetImageIdPile(int size, int offset) {
        int currentRowNumber = offset;
        List<Guid> imageIds = await _database.Images
        .Select(img => img.UUID)
        .OrderBy(uuid => uuid)
        .Skip(offset)
        .Take(size)
        .ToListAsync();
        
        return new OkObjectResult(imageIds);
    }

    public async Task<IActionResult> GetImageIdPileFromSearch(int size, int offset, string searchquery)
    {
        List<Guid> imageIds = await _database.ProductImages
            .Join(_database.Products,
                pi => pi.ProductUUID,
                p => p.UUID,
                (pi, p) => new { ProductImage = pi, Product = p })
            .Where(joined => joined.Product.Name.Contains(searchquery))
            .OrderBy(joined => joined.Product.Name)
            .Skip(offset)
            .Take(size)
            .Select(joined => joined.ProductImage.ImageUUID)
            .ToListAsync();
            
        return new OkObjectResult(imageIds);
    }
    
    public async Task<IActionResult> GetImageByUUID(string uuid)
    {
        Image? finalImage = null;
        Guid? imageUUID = HelperService.ParseStringGuid(uuid);
        if (imageUUID != null)
        {
            finalImage = await _database.Images
                .FirstOrDefaultAsync(i => i.UUID == imageUUID);
        }

        if (finalImage == null)
        {
            finalImage = GetDefaultImage();
        }

        FileContentResult fileContentResult = HelperService.ConvertImageToFileContent(finalImage);
        return fileContentResult;
    }

    
    public async Task<IActionResult> GetAllImageUUIDs()
    {
	    List<Guid> uuids = await _database.Images.Select(img => img.UUID).ToListAsync();

	    return new OkObjectResult(uuids);
    }
    
    public async Task<IActionResult> CreateMockProduct(CreateMockProductRequest requestParams)
    {
        Product mockProduct = new Product
        {
            UUID = Guid.NewGuid(),
            Name = requestParams.Name
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

    public async Task<IActionResult> DeleteAllProducts()
    {
        var products = await _database.Products.ToListAsync();
        var deleted = await _database.Delete(products);
        int numDeleted = await _database.SaveChangesAsync();
        
        return new OkObjectResult(deleted);
    }
    
    public async Task<IActionResult> GetProduct(string productId)
    {
        Guid? productUUID = HelperService.ParseStringGuid(productId);

        if (productUUID == null)
        {
            return new BadRequestObjectResult($"Invalid product uuid");
        }

        Product? product = null;
        
        try {
            product = await _database.Products.Select(i => i)
                .Where(i => i.UUID == productUUID)
                .FirstOrDefaultAsync();

            if (product == null) throw new Exception("No image found by that priority");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        GetProductResponse response = new GetProductResponse(product.Name, product.UUID);
        
        return new OkObjectResult(response);
    }
    private Image GetDefaultImage()
    {
        Image image = new Image
        {
            Content = _configuration.GetSection("DefaultImages")["NotFound"] ?? throw new Exception("No default image found")
        };
        
        return image;
    }
    public async Task<IActionResult> GetProductsFromPIM()
    {
        using var client = new HttpClient();

        try
        {
            string pimApiUrl = "http://localhost:5084/api/products/list?page=99999";

            HttpResponseMessage response = await client.GetAsync(pimApiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return new BadRequestObjectResult("Failed to fetch all products from PIM.");
            }

            // Read the actual content from the response
            string result = await response.Content.ReadAsStringAsync();

            // Deserialize the actual content
            var products = JsonSerializer.Deserialize<List<Product>>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return new OkObjectResult(products);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new BadRequestObjectResult("Failed to fetch products from PIM.");
        }
    }


}
