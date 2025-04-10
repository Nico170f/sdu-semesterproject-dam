using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Image = DAM.Backend.Data.Models.Image;
using Microsoft.AspNetCore.Http.HttpResults;

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
        Guid? productUUID = ParseStringGuid(productId);
        if (productUUID == null)
        {
            return new BadRequestObjectResult("Invalid product uuid");
        }

        List<Guid> imageIds = await _database.ProductImages
            .Where(i => i.ProductUUID == productUUID)
            .Select(i => i.ImageUUID)
            .ToListAsync();

        GetProductAssetsIdsResponse response = new GetProductAssetsIdsResponse(imageIds);
        return new OkObjectResult(response);
    }


    public async Task<IActionResult> GetProductAssetAmount(string productId)
    {
        Guid? productUUID = ParseStringGuid(productId);
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
    public async Task<IActionResult> GetImage(string productId, string priority)
    {

        int? imagePriority = GetImagePriority(priority);
        Guid? productUUID = ParseStringGuid(productId);

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


        return ConvertImageToFileContent(finalImage);
    }

    //Method for creating a new image
    public async Task<IActionResult> CreateImage(CreateImageRequest requestParams)
    {
        if (requestParams.Content.Length < 30)
        {
            return new BadRequestObjectResult("Image content is too short");
        }
        
        Image image = new Image();
        image.UUID = Guid.NewGuid();
        image.Content = requestParams.Content;
        image.CreatedAt = DateTime.Now;
        image.UpdatedAt = DateTime.Now;
        
        (int Width, int Height) dimensions = GetImageDimensions(image.Content);
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

    public async Task<IActionResult> UpdateImage(string imageId, UpdateImageRequest requestParametre)
    {
        Guid? imageUUID = ParseStringGuid(imageId);
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
        

            image.Content = requestParametre.Content;
            image.UpdatedAt = DateTime.Now;

            (int Width, int Height) dimensions = GetImageDimensions(image.Content);
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
        
        Guid? imageUUID = ParseStringGuid(imageId);
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

        (int Width, int Height) dimensions = GetImageDimensions(image.Content);
        image.Width = dimensions.Width;
        image.Height = dimensions.Height;

        bool updateResult = await _database.SaveChangesAsync() > 0;
        if (!updateResult)
        {
            return new BadRequestObjectResult("Failed to update image");
        }

        return new OkObjectResult("Image updated successfully");
    }
    public async Task<IActionResult> PatchProductImage(string productId, string imageId, JsonPatchDocument<ProductImage> patchDoc)
    {
        if (patchDoc == null)
        {
            return new BadRequestObjectResult("Patch document cannot be null");
        }


        Guid? imageUUID = ParseStringGuid(imageId);
        Guid? productUUID = ParseStringGuid(productId);
        if (imageUUID == null || productUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        ProductImage? image = await _database.ProductImages
            .Include(pi => pi.ProductUUID)
            .Include(pi => pi.ImageUUID)
            .FirstOrDefaultAsync(pi => pi.ProductUUID == productUUID && pi.ImageUUID == imageUUID);

        if (image == null)
        {
            return new NotFoundObjectResult($"Image with ID {imageId} not found");
        }

        int originalPriority = image.Priority;
        patchDoc.ApplyTo(image);

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
    

    public async Task<IActionResult> DeleteImage(string imageId)
    {

        Guid? imageUUID = ParseStringGuid(imageId);
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

        return new OkObjectResult("Image deleted successfully");
    }


    public async Task<IActionResult> GetImageIdPile(int size, int offset) {
        int currentRowNumber = offset;
        List<Guid> imageIds = await _database.ProductImages
        .Select(img => img.ImageUUID)
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
        Guid? imageUUID = ParseStringGuid(uuid);
        if (imageUUID != null)
        {
            finalImage = await _database.Images
                .FirstOrDefaultAsync(i => i.UUID == imageUUID);
        }

        if (finalImage == null)
        {
            finalImage = GetDefaultImage();
        }

        FileContentResult fileContentResult = ConvertImageToFileContent(finalImage);
        return fileContentResult;
    }


    private FileContentResult ConvertImageToFileContent(Image finalImage)
    { 
        var imageParts = finalImage.Content.Split(";base64,");
        var imageType = imageParts[0].Substring(5);
        
        byte[] imageBytes = Convert.FromBase64String(imageParts[1]);
        return new FileContentResult(imageBytes, imageType);
    }
    
    private bool IsValidId(string id)
    {
        return Guid.TryParse(id, out Guid _);
    }
    
    private (int Width, int Height) GetImageDimensions(string base64Image)
    {
        var base64Data = base64Image.Contains(",") ? base64Image.Split(',')[1] : base64Image;
        byte[] imageBytes = Convert.FromBase64String(base64Data);

        using var ms = new MemoryStream(imageBytes);
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(ms);
        return (image.Width, image.Height);
    }
    
    private Image GetDefaultImage()
    {
        Image image = new Image();
        image.Content = _configuration.GetSection("DefaultImages")["NotFound"] ?? throw new Exception("No default image found");
        return image;
    }

    private int? GetImagePriority(string priorityString)
    {
        
        bool isParsed = int.TryParse(priorityString, out int priority);
        if(!isParsed)
        {
            return null;
        }

        return priority;
    }


    private Guid? ParseStringGuid(string guidString)
    {
        if (string.IsNullOrEmpty(guidString))
        {
            return null;
        }

        if (Guid.TryParse(guidString, out Guid guid))
        {
            return guid;
        }

        return null;
    }
}



