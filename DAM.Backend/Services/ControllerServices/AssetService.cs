using DAM.Backend.Controllers;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using System.Linq;

namespace DAM.Backend.Services.ControllerServices;

public class AssetService : IAssetService
{

    public AssetService()
    {
    }

    public async Task<IActionResult> GetProductAssets(string productId)
    {
        return new OkObjectResult("Produkt ID: " + productId);
    }

    public async Task<IActionResult> GetImage(string productId, string priority)
    {
        
        
        
        throw new NotImplementedException();
        
        // Image? foundImage = DummyDataGenerator.GetImageByProductAndPriority(Guid.Parse(productId), int.Parse(priority));
        // if (foundImage == null)
        // {
        //     return new NotFoundObjectResult("No image found by that UUID and priority");
        // }
        // return new OkObjectResult(foundImage);
    }

    public async Task<IActionResult> CreateImage(string productId, CreateImageRequest requestParams)
    {
        if (!Guid.TryParse(productId, out Guid productGuid))
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        Console.WriteLine(Database.Instance.Product.ToList().Count);
        foreach(var p in Database.Instance.Product.ToList())
        {
            Console.WriteLine(p.UUID);
        }

        Product? product = Database.Instance.Product.ToList().Find(p => p.UUID == productGuid);
        if (product == null)
        {
            return new NotFoundObjectResult("No product found by that UUID");
        }

        
        Image image = new Image();
        image.UUID = Guid.NewGuid();
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
}



