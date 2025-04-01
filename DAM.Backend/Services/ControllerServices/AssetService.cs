using DAM.Backend.Controllers;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using System.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DAM.Backend.Services.ControllerServices;

public class AssetService : IAssetService
{

    private readonly IConfiguration _configuration;

    public AssetService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> GetProductAssets(string productId)
    {
        return new OkObjectResult("Produkt ID: " + productId);
    }

    public async Task<IActionResult> GetImage(string productId, string priority)
    {

        List<Image> images = await Database.Instance.Images.ToListAsync();
        Image? image = images.FirstOrDefault(i => i.Product != null && i.Product.UUID == productId);
        
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

        Product? product = Database.Instance.Product.ToList().Find(p => p.UUID == requestParams.ProductId);
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

    public Task<IActionResult> UpdateImage(string imageId, UpdateImageRequest requestParametre)
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> PatchImage(string imageId, PatchImageRequest requestParametre)
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> DeleteImage(string imageId)
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> GetImageIdPile(int size, int offset) {
        int currentRowNumber = offset;
        List<string> imageIds = await Database.Instance.Images
        .Select(img => img.UUID)
        .OrderBy(uuid => uuid)
        .Skip(offset)
        .Take(size)
        .ToListAsync();
        
        return new OkObjectResult(imageIds);
    }

public async Task<IActionResult> GetImageIdPileFromSearch(int size, int offset, string searchquery)
{
    List<string> imageIds = await Database.Instance.Images
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
        List<Image> images = await Database.Instance.Images.ToListAsync();
        Image? image = images.FirstOrDefault(i => i.UUID == uuid);
        
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



