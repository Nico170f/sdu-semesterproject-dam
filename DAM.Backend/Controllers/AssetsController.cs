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
     * Upload a new asset (expects base64 asset content).
     */
    [HttpPost()]
    public async Task<IActionResult> PostCreateAsset([FromBody] CreateAssetRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.CreateAsset(body);
    }
    
    
    /*
     * GET /assets
     * Get all assets (optionally with pagination and search filters).
     */
    [HttpGet()]
    public async Task<IActionResult> GetAssetsWithOptionalParameters([FromQuery] string? searchString, [FromQuery] string? selectedTagIds, [FromQuery] int? amount, [FromQuery] int? page)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetAssets(searchString, selectedTagIds, amount, page);
    }
    
    
    /*
     * GET /assets/{assetId}
     * Retrieves an asset by its UUID.
     */
    [HttpGet("{assetId}")]
    public async Task<IActionResult> GetAsset(string assetId, [FromQuery] int? height, [FromQuery] int? width)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetAssetById(assetId, width, height);
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
     * Partially updates an asset using JSON Patch document.
     */
    [HttpPatch("{assetId}")]
    public async Task<IActionResult> PatchAsset(string assetId, [FromBody] JsonPatchDocument<Asset> patchDoc)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.PatchAsset(assetId, patchDoc);
    }
    
    
    /*
     * DELETE /assets/{assetId}
     * Deletes an asset by its ID.
     */
    [HttpDelete("{assetId}")]
    public async Task<IActionResult> DeleteAsset(string assetId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.DeleteAsset(assetId);
    }
    
    
    /*
     * GET /assets/search | /assets/search?size=10&page=0&searchQuery=example
     * Returns a paginated list of asset IDs filtered by search query.
     */
    [HttpGet("search")]
    public async Task<IActionResult> SearchAssets([FromQuery] int size, [FromQuery] int page, [FromQuery] string? searchQuery = null) 
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetAssetIdPileFromSearch(size, page * size, searchQuery);
    }
    
    
    /*
     * GET assets/{assetId}/tags/gallery
     * Gets all tags that are not already associated with an asset with optional searching for name/uuid and pagination
     */
    [HttpGet("{assetId}/tags/gallery")]
    public async Task<IActionResult> GetAssetTagsGallery(string assetId, [FromQuery] string? searchString = null, [FromQuery] int? amount = null, [FromQuery] int? page = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetAssetTagsGallery(assetId, searchString, amount, page);
    }
    
    
    /*
     * GET assets/{assetId}/tags
     * Gets all tags associated with an asset
     */
    [HttpGet("{assetId}/tags")]
    public async Task<IActionResult> GetAssetTags(string assetId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.GetAssetTags(assetId);
    }
    
    /*
     * POST /assets/{assetId}/tags/{tagId}
     * Adds a tag to an asset.
     */
    [HttpPost("{assetId}/tags/{tagId}")]
    public async Task<IActionResult> AddAssetTag(string assetId, string tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.AddAssetTag(assetId, tagId);
    }
    
    
    /*
     * DELETE /assets/{assetId}/tags/{tagId}
     * Removes a tag from an asset.
     */
    [HttpDelete("{assetId}/tags/{tagId}")]
    public async Task<IActionResult> DeleteAssetTag(string assetId, string tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _assetService.RemoveAssetTag(assetId, tagId);
    }
    
    /*
     * GET /assets/count
     * 
     */
    [HttpGet("count")]
    public async Task<IActionResult> GetCountOfAssets([FromQuery] string? searchString = null, [FromQuery] string? selectedTagIds = null)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await _assetService.GetCountOfAssets(searchString, selectedTagIds);
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