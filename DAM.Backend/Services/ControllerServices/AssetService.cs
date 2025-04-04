using DAM.Backend.Controllers;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Image = DAM.Backend.Data.Models.Image;

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
    public async Task<IActionResult> GetProductAssets(string productId)
    {
        return new OkObjectResult("Produkt ID: " + productId);
    }

    //Method for returning a single image by productId and priority
    public async Task<IActionResult> GetImage(string productId, string priority)
    {

        Image? finalImage = null;
        

        try
        {
            int priorityNum = int.Parse(priority);
            
            finalImage = await _database.Images
                .Include(i => i.Product)
                .Where(i => i.Product != null && 
                            i.Product.UUID.ToUpper() == productId.ToUpper() && 
                            i.Priority == priorityNum)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            if (finalImage == null)
            {
                Console.WriteLine("Image was not found");
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
        image.UUID = Guid.NewGuid().ToString();
        image.Content = requestParams.Content;
        image.Priority = 0;
        image.IsShown = true;
        image.CreatedAt = DateTime.Now;
        image.UpdatedAt = DateTime.Now;
        
        (int Width, int Height) dimensions = GetImageDimensions(image.Content);
        image.Width = dimensions.Width;
        image.Height = dimensions.Height;

        // bool imageCreated = await _database.Create(image);
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
        // if (!Guid.TryParse(imageId, out Guid imageGuid))
        // {
        //     return new BadRequestObjectResult("Invalid UUID format");
        // }
        //
        // var image = await _database.Images.FindAsync(imageGuid);
        // if (image == null)
        // {
        //     return new NotFoundObjectResult("No image found by that UUID");
        // }
        //
        // try
        // {
        //     image.Content = requestParametre.Content;
        //     image.IsShown = requestParametre.IsShown;
        //     image.Width = requestParametre.Width ?? 0;
        //     image.Height = requestParametre.Height ?? 0;
        //     image.Priority = requestParametre.Priority ?? 0;
        //     image.UpdatedAt = DateTime.Now;
        // }
        // catch (Exception e)
        // {
        //     return new BadRequestObjectResult("Failed to update image");
        // }
        //
        // return new OkObjectResult("Image updated");
        return null;
    }

    
    
    
public async Task<IActionResult> PatchImage(string imageId, JsonPatchDocument<Image> patchDoc)
{
    if (patchDoc == null)
    {
        return new BadRequestObjectResult("Patch document cannot be null");
    }

    var image = await _database.Images
        .Include(i => i.Product)
        .FirstOrDefaultAsync(i => i.UUID == imageId);

    if (image == null)
    {
        return new NotFoundObjectResult($"Image with ID {imageId} not found");
    }

    // Store original priority for comparison
    int originalPriority = image.Priority;
    bool priorityChanged = false;
    int newPriority = originalPriority;

    // Check if priority is being updated
    var priorityOperation = patchDoc.Operations.FirstOrDefault(op => 
        op.path.Equals("/priority", StringComparison.OrdinalIgnoreCase) && 
        op.op.Equals("replace", StringComparison.OrdinalIgnoreCase));
    
    if (priorityOperation != null && priorityOperation.value != null)
    {
        priorityChanged = true;
        newPriority = Convert.ToInt32(priorityOperation.value);
    }
    
    // Handle special case for ProductUUID path
    foreach (var operation in patchDoc.Operations.ToList())
    {
        if (operation.path.Equals("/ProductUUID", StringComparison.OrdinalIgnoreCase) 
            && operation.op.Equals("replace", StringComparison.OrdinalIgnoreCase))
        {
            string productUuid = operation.value?.ToString();
        
            // Remove the original ProductUUID operation
            patchDoc.Operations.Remove(operation);
        
            if (string.IsNullOrEmpty(productUuid))
            {
                image.Product = null;
            }
            else
            {
                var product = await _database.Product.FirstOrDefaultAsync(p => p.UUID == productUuid);
                if (product == null)
                {
                    return new BadRequestObjectResult($"Product with ID {productUuid} not found");
                }
                image.Product = product;
            }
        }
    }

    // Apply patch operations
    patchDoc.ApplyTo(image);
    
    // Handle priority changes if needed
    if (priorityChanged && image.Product != null)
    {
        // Get all other images for the same product
        var otherImages = await _database.Images
            .Where(img => img.Product != null && 
                          img.Product.UUID.ToUpper() == image.Product.UUID.ToUpper() && 
                          img.UUID != image.UUID)
            .OrderBy(img => img.Priority)
            .ToListAsync();
        
        // Insert image at new priority and reorder all images
        var allImages = new List<Image>(otherImages);
        int insertPosition = Math.Min(newPriority, allImages.Count);
        allImages.Insert(insertPosition, image);
        
        // Reassign priorities sequentially
        for (int i = 0; i < allImages.Count; i++)
        {
            allImages[i].Priority = i;
        }
    }
    
    // Update timestamp
    image.UpdatedAt = DateTime.UtcNow;
    
    // Save changes
    bool updateResult = await _database.SaveChangesAsync() > 0;
    
    if (!updateResult)
    {
        return new StatusCodeResult(500);
    }
    
    return new OkObjectResult(image);
}
    
    
    
    //Does this work?
    public async Task<IActionResult> PatchImage2(string imageId, JsonPatchDocument<Image> patchDocument)
    {
        if(!IsValidId(imageId)) return new BadRequestObjectResult("Invalid UUID format");

        Image? image = await _database.Images
            .Where(i => i.UUID.ToUpper() == imageId.ToUpper())
            .FirstOrDefaultAsync();
        
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }
        
        int beforePatchPriority = image.Priority;
        patchDocument.ApplyTo(image);
        
        image.UpdatedAt = DateTime.Now;
        int afterPatchPriority = image.Priority;
        
        if (beforePatchPriority != afterPatchPriority && image.Product != null)
        {
            
            Product? product = await _database.Product
                .Where(p => p.UUID.ToUpper() == image.Product.UUID.ToUpper())
                .FirstOrDefaultAsync();
            
            var otherImages = await _database.Images
                .Where(img => img.Product != null && 
                              img.Product.UUID.ToUpper() == image.Product.UUID.ToUpper() && 
                              img.UUID != image.UUID)  // Exclude current image
                .OrderBy(img => img.Priority)
                .ToListAsync();

            otherImages.Insert(Math.Min(afterPatchPriority, otherImages.Count), image);
        
            for (int i = 0; i < otherImages.Count; i++)
            {
                otherImages[i].Priority = i;
            }

        }
        await _database.SaveChangesAsync();

        // bool updateResult = await _database.Update(image);
        // if (!updateResult)
        // {
        //     return new BadRequestObjectResult("Failed to update image");
        // }

        return new OkObjectResult("Image updated successfully");
        
        
        
        
        
        
        // throw new NotImplementedException();
        

        // var productImages = await _database.Images
        //     .Where(i => i.Product != null && i.Product.UUID == image.Product.UUID)
        //     .OrderBy(i => i.Priority)
        //     .ToListAsync();
        //
        // var imagePriority =
        //     patchDocument.Operations.FirstOrDefault(op =>
        //         op.path.Equals("/priority", StringComparison .OrdinalIgnoreCase));
        // int.TryParse(imagePriority.value.ToString(), out int newPriority);
        //     
        // if (newPriority != image.Priority && productImages[newPriority] != null)
        // {
        //     productImages.Insert(newPriority, image);
        //     
        //     for (int i = 0; i < productImages.Count; i++)
        //     {
        //         productImages[i].Priority = i;
        //     }
        // }
        
        // // Apply the patch document to the entity
        // patchDocument.ApplyTo(image);
        //
        // // Update timestamp
        // image.UpdatedAt = DateTime.Now;
        //
        // // Save changes to database
        // var result = await _database.Update(image);
        // if (!result)
        // {
        //     return new BadRequestObjectResult("Failed to update image");
        // }
        //
        // return new OkObjectResult("Image updated successfully");
    }

    public async Task<IActionResult> DeleteImage(string imageId)
    {
        if (!Guid.TryParse(imageId, out Guid imageGuid))
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        var image = await _database.Images.FindAsync(imageGuid);
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }

        var result = await _database.Delete(image);
        if (!result)
        {
            return new BadRequestObjectResult("Failed to delete image");
        }

        return new OkObjectResult("Image deleted successfully");
    }

    private Image GetDefaultImage()
    {
        Image image = new Image();
        image.Content = _configuration.GetSection("DefaultImages")["NotFound"] ?? throw new Exception("No default image found");
        return image;
    }

    private FileContentResult ConvertImageToFileContent(Image finalImage)
    { 
        var imageParts = finalImage.Content.Split(";base64,");
        var imageType = imageParts[0].Substring(5);
        
        byte[] imageBytes = Convert.FromBase64String(imageParts[1]);
        return new FileContentResult(imageBytes, imageType);
    }

    public async Task<IActionResult> GetImageIdPile(int size, int offset) {
        int currentRowNumber = offset;
        List<string> imageIds = await _database.Images
        .Select(img => img.UUID)
        .OrderBy(uuid => uuid)
        .Skip(offset)
        .Take(size)
        .ToListAsync();
        
        return new OkObjectResult(imageIds);
    }

    public async Task<IActionResult> GetImageIdPileFromSearch(int size, int offset, string searchquery)
    {
        List<string> imageIds = await _database.Images
            .Where(img => img.Product != null)
            .Where(img => img.Product!.Name.Contains(searchquery)) // Filter by search query
            .OrderBy(img => img.Product!.Name) // Order by name
            .Skip(offset) // Skip offset
            .Take(size) // Take only the required size
            .Select(img => img.UUID) // Select UUID instead of name
            .ToListAsync();

        return new OkObjectResult(imageIds);
    }

    public async Task<IActionResult> GetImageByUUID(string uuid)
    {
		// Image? image = await _database.Images.FirstOrDefaultAsync(i => i.UUID == uuid);
		
		Image? image = await _database.Images.FirstOrDefaultAsync(i => i.UUID == uuid);
        
        if (image == null)
        {
            image = new Image();
            image.Content = _configuration.GetSection("DefaultImages")["NotFound"] ?? throw new Exception("No default image found");
        }

        var imageParts = image.Content.Split(";base64,");
        var imageType = imageParts[0].Substring(5);
        
        byte[] imageBytes = Convert.FromBase64String(imageParts[1]);
        return new FileContentResult(imageBytes, imageType);   
    }
    
    private bool IsValidId(string id)
    {
        return Guid.TryParse(id, out Guid _);
    }
    
    public(int Width, int Height) GetImageDimensions(string base64Image)
    {
        // Strip data URL prefix if it exists
        var base64Data = base64Image.Contains(",") ? base64Image.Split(',')[1] : base64Image;
        byte[] imageBytes = Convert.FromBase64String(base64Data);

        using var ms = new MemoryStream(imageBytes);
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(ms); // Load as RGBA pixel format
        return (image.Width, image.Height);
    }
}



