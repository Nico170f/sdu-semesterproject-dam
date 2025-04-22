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
    
    
    /*
     * Retrieves all asset IDs associated with a specific product.
     */
    [HttpGet("{productId}/all")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAssetsIds(string productId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetProductAssetsIds(productId);
    }

    /*
     * Gets the total number of assets for a specific product.
     */
    [HttpGet("{productId}/amount")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAssetsAmount(string productId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetProductAssetAmount(productId);
    }

    /*
     * Retrieves an image from a product based on priority level.
     */
    [HttpGet("{productId}/{priority}")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetImageFromProduct(string productId, string priority)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetProductImage(productId, priority);
    }
    
    /*
     * Creates a new image from a provided request body.
     */
    [HttpPost()]
    public async Task<IActionResult> PostCreateImage([FromBody] CreateImageRequest requestParams)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.CreateImage(requestParams);
    }
    
    [HttpPost("{productId}/add")]
    public async Task<IActionResult> AddImageToProduct(string productId, [FromBody] AddProductImageRequest requestParams)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.AddProductImage(productId, requestParams);
    }
    
    [HttpPost("{productId}/remove")]
    public async Task<IActionResult> RemoveImageFromProduct(string productId, [FromBody] RemoveProductImageRequest requestParams)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.RemoveProductImage(productId, requestParams);
    }
    
    /*
     * Updates an existing image with the specified ID.
     */
    [HttpPut("{imageId}")]
    public async Task<IActionResult> PutUpdateImage(string imageId, [FromBody] UpdateImageRequest requestParams)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.UpdateImage(imageId, requestParams);
    }
    
    /*
     * Partially updates an image using JSON Patch document.
     */
    [HttpPatch("{imageId}")]
    public async Task<IActionResult> PatchImage(string imageId, [FromBody] JsonPatchDocument<Image> patchDoc)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.PatchImage(imageId, patchDoc);
    }
    
    /*
     * Partially updates a product-image relationship using JSON Patch.
     */
    [HttpPatch("{productId}/{imageId}")]
    public async Task<IActionResult> PatchProductImage(string productId, string imageId, [FromBody] JsonPatchDocument<ProductImage> patchDoc)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.PatchProductImage(productId, imageId, patchDoc);
    }
    
    /*
     * Removes an image from a specific product.
     */
    [HttpDelete("{productId}/{imageId}")]
    public async Task<IActionResult> DeleteProductImage(string productId, string imageId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.DeleteProductImage(productId, imageId);
    }
    
    /*
     * Deletes an image by its ID.
     */
    [HttpDelete("{imageId}")]
    public async Task<IActionResult> DeleteImage(string imageId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.DeleteImage(imageId);
    }

    
    /*
     * Returns a paginated list of image IDs.
     */
    [HttpGet("imageIdPile")]
    public async Task<IActionResult> GetImageIdPile([FromQuery] int size, [FromQuery] int page) 
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        int offset = page*size;
        return await _assetService.GetImageIdPile(size, offset);
    }

    
    /*
     * Returns a paginated list of image IDs filtered by search query.
     */
    [HttpGet("imageIdPileFromSearch")]
    public async Task<IActionResult> GetImageIdPileFromSearch([FromQuery] int size, [FromQuery] int page, [FromQuery] string searchquery) 
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        int offset = page*size;
        return await _assetService.GetImageIdPileFromSearch(size, offset, searchquery);
    }

    
    /*
     * Retrieves an image by its UUID.
     */
    [HttpGet("getImageByUUID")]
    public async Task<IActionResult> GetImageByUUID([FromQuery] string uuid)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await _assetService.GetImageByUUID(uuid);
    }
    

    [HttpGet("{productId}/gallery")]
    public async Task<IActionResult> GetProductGallery(string productId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await _assetService.GetProductGallery(productId);
    }
    
    
    /*
     * Testing endpoint to delete all images (currently disabled).
     */
    [HttpPost("delete-all")]
    public async Task<IActionResult> DeleteAllImages()
    {
        // var allImages = await Database.Instance.Images.ToListAsync();
        // Database.Instance.Images.RemoveRange(allImages);
        // await Database.Instance.SaveChangesAsync();

        return Ok();
    }
}