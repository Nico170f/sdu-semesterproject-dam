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
    public async Task<IActionResult> GetProductAssets(string productId)
    {
        return await _assetService.GetProductAssets(productId);
    }

    [HttpGet("{productId}/{priority}")]
    public async Task<IActionResult> GetImageFromProduct(string productId, string priority)
    {
        return await _assetService.GetAssetImage(productId, priority);
    }
    
    [HttpPost("{productId}")]
    public async Task<IActionResult> PostCreateImage(string productId, [FromBody] CreateImageRequest requestParams)
    {
        return await _assetService.CreateImage(productId, requestParams);
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