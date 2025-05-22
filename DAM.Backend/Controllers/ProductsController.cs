using DAM.Backend.Services.ControllerServices;
using DAM.Shared.Models;
using DAM.Shared.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Controllers;

public class ProductsController(IProductService productService) : ApiController
{

	/*
     * GET /products
     * Gets products
     */
    [HttpGet()]
    public async Task<IActionResult> GetProducts(
	    [FromQuery] string? searchString = null, 
	    [FromQuery] int? amount = null, 
	    [FromQuery] int? page = null)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await productService.GetProducts(searchString, amount, page);
    }
    
    /*
     * GET /products/{productId}
     * Get product by id
     */
    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetProduct(Guid productId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.GetProduct(productId);
    }
    
    /*
     * GET /products/{productId}/assets
     * List all asset IDs associated with a specific product.
     */
    [HttpGet("{productId:guid}/assets")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAssets(Guid productId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.GetProductAssets(productId);
    }
    
    /*
     * GET /products/{productId}/assets/count
     * Get the total number of assets for a product.
     */
    [HttpGet("{productId:guid}/count")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAssetsAmount(Guid productId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.GetProductAssetsAmount(productId);
    }
    
    /*
     * GET /products/{productId}/assets/{priority}
     * Get a product's asset by priority.
     */
    [HttpGet("{productId:guid}/assets/{priority:int}")]
    public async Task<IActionResult> GetProductAsset(
	    Guid productId, 
	    int priority, 
	    [FromQuery] int? width = null, 
	    [FromQuery] int? height = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.GetProductAsset(productId, priority, width, height);
    }
    
    
    /*
     * POST /products/{productId}/assets
     * Associate an existing asset with a product
     */
    [HttpPost("{productId:guid}/assets")]
    public async Task<IActionResult> AssignProductAsset(Guid productId, [FromBody] AddProductAssetRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.AssignProductAsset(productId, body);
    }
    
    /*
     * DELETE /products/{productId}/assets/{assetId}
     * Remove an asset from a product.
     */
    [HttpDelete("{productId:guid}/assets/{assetId:guid}")]
    public async Task<IActionResult> RemoveProductAsset(Guid productId, Guid assetId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.UnassignProductAsset(productId, assetId);
    }
    
    /*
     * PATCH /products/{productId}/assets/{assetId}
     * Update relationship metadata (like priority) between product and asset.
     */
    [HttpPatch("{productId:guid}/assets/{assetId:guid}")]
    public async Task<IActionResult> PatchProductAsset(Guid productId, Guid assetId, [FromBody] JsonPatchDocument<ProductAsset> patchDoc)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.PatchProductAsset(productId, assetId, patchDoc);
    }
    
    
	/*
	 * GET /products/syncWithPim
	 * Triggers backend to sync up products with pim
	 */
    [HttpGet("syncWithPim")]
    public async Task<IActionResult> SynchronizeWithPim ()
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await productService.GetProductsFromPIM();
    }


    #region Testing/Unused

    /*
     * POST /products/mock
     * Add a mock product.
     */
    [HttpPost("mock")]
    public async Task<IActionResult> CreateMockProduct([FromBody] CreateMockProductRequest body)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await productService.CreateMockProduct(body);
    }
    
    /*
     * POST /products/add
     * Add a product with both name and uuid
     */
    [HttpPost("add")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest body)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await productService.CreateProduct(body);
    }
    
    #endregion
}