using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Controllers;

public class AssetsController : ApiController
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }
    
    
    /*
     * POST /assets
     * Upload a new asset (expects base64 image content).
     */
    [HttpPost()]
    public async Task<IActionResult> PostCreateAsset([FromBody] CreateImageRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.CreateImage(body);
    }
    
    
    /*
     * GET /assets
     * Get all assets (optionally with pagination and search filters).
     */
    [HttpGet()]
    public async Task<IActionResult> GetAssetsPage([FromQuery] int? size, [FromQuery] int? page)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        int? offset = page*size;
        return await _assetService.GetAssetsPage(size, offset);
    }
    
    
    /*
     * GET /assets/{assetId}
     * Retrieves an asset by its UUID.
     */
    [HttpGet("{assetId}")]
    public async Task<IActionResult> GetAsset(string assetId, [FromQuery] int? height, [FromQuery] int? width)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetImageById(assetId, height, width);
    }
    
    
    /*
     * PUT /assets/{assetId}
     * Fully update an asset by ID.
     */
    [HttpPut("{assetId}")]
    public async Task<IActionResult> PutUpdateAsset(string assetId, [FromBody] UpdateAssetRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.UpdateAsset(assetId, body);
    }
    
    
    
    /*
     * PATCH /assets/{assetId}
     * Partially updates an image using JSON Patch document.
     */
    [HttpPatch("{assetId}")]
    public async Task<IActionResult> PatchAsset(string assetId, [FromBody] JsonPatchDocument<Image> patchDoc)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.PatchAsset(assetId, patchDoc);
    }
    
    
    /*
     * DELETE /assets/{imageId}
     * Deletes an image by its ID.
     */
    [HttpDelete("{assetId}")]
    public async Task<IActionResult> DeleteAsset(string assetId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.DeleteAsset(assetId);
    }
    
    
    /*
     * GET /assets/search | /assets/search?size=10&page=0&searchQuery=example
     * Returns a paginated list of image IDs filtered by search query.
     */
    [HttpGet("search")]
    public async Task<IActionResult> SearchAssets([FromQuery] int size, [FromQuery] int page, [FromQuery] string? searchQuery = null) 
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetImageIdPileFromSearch(size, page * size, searchQuery);
    }
    
    
    /*
     * GET assets/{assetId}/tags/gallery
     * Gets all tags that are not already associated with an asset
     */
    [HttpGet("{assetId}/tags/gallery")]
    public async Task<IActionResult> GetAssetTagsGallery(string assetId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetAssetTagsGallery(assetId);
    }
    
    
    /*
     * GET assets/{assetId}/tags
     * Gets all tags associated with an image
     */
    [HttpGet("{assetId}/tags")]
    public async Task<IActionResult> GetAssetTags(string assetId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetAssetTags(assetId);
    }
    
    /*
     * POST /assets/{assetId}/tags/{tagId}
     * Adds a tag to an image.
     */
    [HttpPost("{imageId}/tags/{tagId}")]
    public async Task<IActionResult> AddAssetTag(string imageId, string tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.AddAssetTag(imageId, tagId);
    }
    
    
    /*
     * DELETE /assets/{imageId}/tags/{tagId}
     * Removes a tag from an image.
     */
    [HttpDelete("{imageId}/tags/{tagId}")]
    public async Task<IActionResult> DeleteAssetTag(string imageId, string tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.RemoveAssetTag(imageId, tagId);
    }
    
}


// [HttpGet("getProductsFromPIM")]
// public async Task<IActionResult> GetProductsFromPIM()
// {
//     if (!ModelState.IsValid) return BadRequest(ModelState);
//     return await _assetService.GetProductsFromPIM();
// }
//
//
// [HttpDelete("delete-all-products")]
// public async Task<IActionResult> DeleteAllProducts()
// {
//     if(!ModelState.IsValid) return BadRequest(ModelState);
//     return await _assetService.DeleteAllProducts();
// }