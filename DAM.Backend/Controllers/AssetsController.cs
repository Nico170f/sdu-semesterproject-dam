using DAM.Backend.Services.ControllerServices;
using DAM.Shared.Models;
using DAM.Shared.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Controllers;

public class AssetsController(IAssetService assetService) : ApiController
{
	
	/*
     * POST /assets
     * Upload a new asset (expects base64 asset content).
     */
    [HttpPost()]
    public async Task<IActionResult> PostCreateAsset([FromBody] CreateAssetRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.CreateAsset(body);
    }
    
    
    /*
     * GET /assets
     * Get all assets (optionally with pagination and search filters).
     */
    [HttpGet()]
    public async Task<IActionResult> GetAssetsWithOptionalParameters([FromQuery] string? searchString, [FromQuery] string? selectedTagIds, [FromQuery] int? amount, [FromQuery] int? page)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.GetAssets(searchString, selectedTagIds, amount, page);
    }
    
    
    /*
     * GET /assets/{assetId}
     * Retrieves an asset by its UUID.
     */
    [HttpGet("{assetId:guid}")]
    public async Task<IActionResult> GetAsset(Guid assetId, [FromQuery] int? width, [FromQuery] int? height)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.GetAssetById(assetId, width, height);
    }
    
    
    /*
     * PUT /assets/{assetId}
     * Fully update an asset by ID.
     */
    [HttpPut("{assetId:guid}")]
    public async Task<IActionResult> PutUpdateAsset(Guid assetId, [FromBody] UpdateAssetRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.UpdateAsset(assetId, body);
    }
    
    
    
    /*
     * PATCH /assets/{assetId}
     * Partially updates an asset using JSON Patch document.
     */
    [HttpPatch("{assetId:guid}")]
    public async Task<IActionResult> PatchAsset(Guid assetId, [FromBody] JsonPatchDocument<Asset> patchDoc)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.PatchAsset(assetId, patchDoc);
    }
    
    
    /*
     * DELETE /assets/{assetId}
     * Deletes an asset by its ID.
     */
    [HttpDelete("{assetId:guid}")]
    public async Task<IActionResult> DeleteAsset(Guid assetId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.DeleteAsset(assetId);
    }
    
    
    /*
     * GET /assets/search | /assets/search?size=10&page=0&searchQuery=example
     * Returns a paginated list of asset IDs filtered by search query.
     */
    [HttpGet("search")]
    public async Task<IActionResult> SearchAssets([FromQuery] int size, [FromQuery] int page, [FromQuery] string? searchQuery = null) 
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.GetAssetIdPileFromSearch(size, page * size, searchQuery);
    }
    
    
    /*
     * GET assets/{assetId}/tags/gallery
     * Gets all tags that are not already associated with an asset with optional searching for name/uuid and pagination
     */
    [HttpGet("{assetId:guid}/tags/gallery")]
    public async Task<IActionResult> GetAssetTagsGallery(Guid assetId, [FromQuery] string? searchString = null, [FromQuery] int? amount = null, [FromQuery] int? page = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.GetAssetTagsGallery(assetId, searchString, amount, page);
    }
    
    
    /*
     * GET assets/{assetId}/tags
     * Gets all tags associated with an asset
     */
    [HttpGet("{assetId:guid}/tags")]
    public async Task<IActionResult> GetAssetTags(Guid assetId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.GetAssetTags(assetId);
    }
    
    /*
     * POST /assets/{assetId}/tags/{tagId}
     * Adds a tag to an asset.
     */
    [HttpPost("{assetId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> AddAssetTag(Guid assetId, Guid tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.AddAssetTag(assetId, tagId);
    }
    
    
    /*
     * DELETE /assets/{assetId}/tags/{tagId}
     * Removes a tag from an asset.
     */
    [HttpDelete("{assetId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> DeleteAssetTag(Guid assetId, Guid tagId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.RemoveAssetTag(assetId, tagId);
    }
    
    /*
     * GET /assets/count
     * 
     */
    [HttpGet("count")]
    public async Task<IActionResult> GetCountOfAssets([FromQuery] string? searchString = null, [FromQuery] string? selectedTagIds = null)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await assetService.GetCountOfAssets(searchString, selectedTagIds);
    }
}