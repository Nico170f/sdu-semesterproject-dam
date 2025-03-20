using DAM.Backend.Controllers;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;

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

    public async Task<IActionResult> GetAssetImage(string productId, string priority)
    {
        Image? foundImage = DummyDataGenerator.GetImageByProductAndPriority(Guid.Parse(productId), int.Parse(priority));
        if (foundImage == null)
        {
            return new NotFoundObjectResult("No image found by that UUID and priority");
        }
        return new OkObjectResult(foundImage);
    }

    public async Task<IActionResult> CreateImage(string productId, CreateImageRequest requestParams)
    {
       Image image = DummyDataGenerator.CreateImage(requestParams);
       return new OkObjectResult("Created image: " + image.UUID);
    }
    
    public async Task<IActionResult> UpdateImage(string productId, CreateImageRequest requestParams)
    {
        return null;
    }
    
}



