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
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.CreateAsset(body);
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
     * GET /assets
     * Gets assets
     */
    [HttpGet]
    public async Task<IActionResult> GetAssets(
	    [FromQuery] Guid? productIdParent = null,
	    [FromQuery] Guid? productIdToAvoid = null,
	    [FromQuery] string? searchString = null,
	    [FromQuery] string? selectedTags = null,
	    [FromQuery] int? amount = null, 
	    [FromQuery] int? page = null)
    {
	    if (!ModelState.IsValid) 
		    return BadRequest(ModelState);

	    if (productIdParent is not null)
		    return await assetService.GetAssetsOnProduct(productIdParent.Value);
	    
	    if (productIdToAvoid is not null) 
		    return await assetService.GetAssetsGallery(productIdToAvoid.Value, searchString, selectedTags, amount, page);
	    
	    return await assetService.GetAssets(searchString, selectedTags, amount, page);
    }
    
    /*
     * GET /assets/{assetId}
     * Returns the asset content
     */
    [HttpGet("{assetId:guid}")]
    public async Task<IActionResult> GetAsset(Guid assetId, [FromQuery] int? width, [FromQuery] int? height)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await assetService.GetAssetContent(assetId, width, height);
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
}