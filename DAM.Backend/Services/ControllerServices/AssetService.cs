using DAM.Backend.Controllers;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.JsonPatch;

namespace DAM.Backend.Services.ControllerServices;

public class AssetService : IAssetService
{

    private readonly IConfiguration _configuration;

    public AssetService(IConfiguration configuration)
    {
        _configuration = configuration;
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
            List<Image> images = Database.Instance.Images.ToList();
            finalImage = images.FirstOrDefault(i => i.Product != null && i.Product.UUID == productId);
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

        Console.WriteLine(Database.Instance.Product.ToList().Count);
        foreach(var p in Database.Instance.Product.ToList())
        {
            Console.WriteLine(p.UUID);
        }

        Product? product = Database.Instance.Product.ToList().Find(p => p.UUID == requestParams.ProductId.ToUpper());
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
        
        var result = await Database.Instance.Create(image);
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

        var image = await Database.Instance.Images.FindAsync(imageGuid);
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

    public async Task<IActionResult> PatchImage(string imageId, JsonPatchDocument<Image> patchDocument)
    {
        if (!Guid.TryParse(imageId, out Guid imageGuid))
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }
        
        var image = await Database.Instance.Images.FindAsync(imageGuid);
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }
        
        
        // Apply the patch document to the entity
        patchDocument.ApplyTo(image);
    
        // Update timestamp
        image.UpdatedAt = DateTime.Now;

        // Save changes to database
        var result = await Database.Instance.Update(image);
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

        var image = await Database.Instance.Images.FindAsync(imageGuid);
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }

        var result = await Database.Instance.Delete(image);
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
}



