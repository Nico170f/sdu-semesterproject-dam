using System.Threading.Tasks;
using DAM.Backend.Data.Models;
using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DAM.Backend.Controllers;

public class AssetsController : ApiController
{
    private readonly IAssetService _assetService;

    public AssetsController(
        IAssetService assetService
    )
    {
        _assetService = assetService;
    }
    
    [HttpGet("{productId}")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAssets(string productId)
    {
        return await _assetService.GetProductAssets(productId);
    }

    [HttpGet("{productId}/{priority}")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetImageFromProduct(string productId, string priority)
    {
        // Image image = await _assetService.GetImage(productId, priority);
        // byte[] imageBytes = Convert.FromBase64String(image.Content);
        
         // string imageData =
         //        "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";
         //
         // byte[] imageBytes = Convert.FromBase64String(imageData);
         
        // return new FileContentResult(imageBytes, "image/png");
        
        return await _assetService.GetImage(productId, priority);
    }
    
    [HttpPost("{productId}")]
    public async Task<IActionResult> PostCreateImage([FromBody] CreateImageRequest requestParams)
    {
        return await _assetService.CreateImage(requestParams);
    }
    
    [HttpPut("{imageId}")]
    public async Task<IActionResult> PutUpdateImage(string imageId, [FromBody] UpdateImageRequest requestParams)
    {
        return await _assetService.UpdateImage(imageId, requestParams);
    }
    
    [HttpPatch("{imageId}")]
    public async Task<IActionResult> PatchUpdateImage(string imageId, [FromBody] PatchImageRequest requestParams)
    {
        return await _assetService.PatchImage(imageId, requestParams);
    }
    
    [HttpDelete("{imageId}")]
    public async Task<IActionResult> DeleteImage(string imageId)
    {
        return await _assetService.DeleteImage(imageId);
    }
    
}