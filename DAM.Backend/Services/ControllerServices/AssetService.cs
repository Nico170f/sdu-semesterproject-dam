using DAM.Backend.Controllers;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
        if (!Guid.TryParse($"{requestParams.ProductId}", out Guid productGuid))
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        // Console.WriteLine(_database.Product.ToList().Count);
        // foreach(var p in _database.Product.ToList())
        // {
        //     Console.WriteLine(p.UUID);
        // }

        Product? product = _database.Product.ToList().Find(p => p.UUID == requestParams.ProductId.ToUpper());
        if (product == null)
        {
            return new NotFoundObjectResult("No product found by that UUID");
        }

        
        Image image = new Image();
        image.UUID = Guid.NewGuid().ToString();
        image.Content = requestParams.Content;
        image.IsShown = requestParams.IsShown;
        image.Product = product;
        image.Width = requestParams.Width ?? 0;
        image.Height = requestParams.Height ?? 0;
        
        
        //Todo: If no priority is set, it should default to the last element in the list.
        //todo: if priority is set, other elements should be shifted.
        image.Priority = requestParams.Priority ?? 0;
        
        image.CreatedAt = DateTime.Now;
        image.UpdatedAt = DateTime.Now;
        
        var result = await _database.Create(image);
        if(result == false) {
            return new BadRequestObjectResult("Failed to create image");
        }

        CreateImageResponse response = new CreateImageResponse(image);
        return new OkObjectResult(response);
    }

    public async Task<IActionResult> UpdateImage(string imageId, UpdateImageRequest requestParametre)
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
        
        try
        {
            image.Content = requestParametre.Content;
            image.IsShown = requestParametre.IsShown;
            image.Width = requestParametre.Width ?? 0;
            image.Height = requestParametre.Height ?? 0;
            image.Priority = requestParametre.Priority ?? 0;
            image.UpdatedAt = DateTime.Now;
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult("Failed to update image");
        }

        return new OkObjectResult("Image updated");
    }

    
    //Does this work?
    public async Task<IActionResult> PatchImage(string imageId, JsonPatchDocument<Image> patchDocument)
    {
        if (!Guid.TryParse(imageId, out Guid imageGuid))
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }
        
        var image = await _database.Images.FindAsync(imageId);
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }



        var productImages = await _database.Images
            .Where(i => i.Product != null && i.Product.UUID == image.Product.UUID)
            .OrderBy(i => i.Priority)
            .ToListAsync();

        var imagePriority =
            patchDocument.Operations.FirstOrDefault(op =>
                op.path.Equals("/priority", StringComparison .OrdinalIgnoreCase));
        int.TryParse(imagePriority.value.ToString(), out int newPriority);
            
        if (newPriority != image.Priority && productImages[newPriority] != null)
        {
            productImages.Insert(newPriority, image);
            
            for (int i = 0; i < productImages.Count; i++)
            {
                productImages[i].Priority = i;
            }
        }
        
        // Apply the patch document to the entity
        patchDocument.ApplyTo(image);
    
        // Update timestamp
        image.UpdatedAt = DateTime.Now;

        // Save changes to database
        var result = await _database.Update(image);
        if (!result)
        {
            return new BadRequestObjectResult("Failed to update image");
        }

        return new OkObjectResult("Image updated successfully");
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
}



