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
        return await _assetService.HandleGetAssetsFromProduct(productId);
    }

    [HttpGet("{productId}/{imageId}")]
    public async Task<IActionResult> GetImageFromProduct(string productId, string imageId)
    {
        return await _assetService.HandleGetImageFromProduct(productId, imageId);
    }
    
    [HttpPost("{productId}")]
    public async Task<IActionResult> PostCreateImage(string productId, [FromBody] Image requestParametre)
    {
        return await _assetService.HandlePostCreateImage(productId, requestParametre);
    }
    
}


public class RequestParametre
{

    public string navn;
    public string magnus;

}