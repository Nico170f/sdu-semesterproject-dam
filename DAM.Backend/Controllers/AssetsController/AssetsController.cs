using System.Threading.Tasks;
using DAM.Backend.Controllers.API;
using DAM.Backend.Data;
using DAM.Backend.Data.Models;
using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    
    [HttpGet("{productId}/all")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAssetsIds(string productId)
    {
        return await _assetService.GetProductAssetsIds(productId);
    }

    [HttpGet("{productId}/amount")]

    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAssetsAmount(string productId)
    {
        return await _assetService.GetProductAssetAmount(productId);
    }

    [HttpGet("{productId}/{priority}")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetImageFromProduct(string productId, string priority)
    {
        return await _assetService.GetImage(productId, priority);
    }
    
    [HttpPost("add")]
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
    public async Task<IActionResult> PatchUpdateImage(string imageId, [FromBody] JsonPatchDocument<Image> patchDoc)
    {
        return await _assetService.PatchImage(imageId, patchDoc);
    }
    
    [HttpDelete("{imageId}")]
    public async Task<IActionResult> DeleteImage(string imageId)
    {
        return await _assetService.DeleteImage(imageId);
    }

    [HttpGet("imageIdPile")]
    public async Task<IActionResult> GetImageIdPile([FromQuery] int size, [FromQuery] int page) 
    {
        int offset = page*size;
        return await _assetService.GetImageIdPile(size, offset);
    }

    [HttpGet("imageIdPileFromSearch")]
    public async Task<IActionResult> GetImageIdPileFromSearch([FromQuery] int size, [FromQuery] int page, [FromQuery] string searchquery) 
    {
        int offset = page*size;
        return await _assetService.GetImageIdPileFromSearch(size, offset, searchquery);
    }

    [HttpGet("getImageByUUID")]
    public async Task<IActionResult> GetImageByUUID([FromQuery] string uuid)
    {
	    return await _assetService.GetImageByUUID(uuid);
    }
    
    //Test method to delete all images
    [HttpPost("delete-all")]
    public async Task<IActionResult> DeleteAllImages()
    {
        // var allImages = await Database.Instance.Images.ToListAsync();
        // Database.Instance.Images.RemoveRange(allImages);
        // await Database.Instance.SaveChangesAsync();

        return Ok();
    }
}