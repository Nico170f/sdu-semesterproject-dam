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
     * Gets products with optional search parameters
     */
    [HttpGet()]
    public async Task<IActionResult> GetProducts([FromQuery] string? searchString = null, [FromQuery] int? amount = null, [FromQuery] int? page = null)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await productService.GetProducts(searchString, amount, page);
    }
        
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
    
    
    /*
     * GET /products/{productId}
     * Get product by id
     */
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProduct(string productId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.GetProduct(productId);
    }
    
    
    /*
     * GET /products/{productId}/assets
     * List all asset IDs associated with a specific product.
     */
    [HttpGet("{productId}/assets")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAssets(string productId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.GetProductAssets(productId);
    }
    
    /*
     * GET /products/{productId}/assets/count
     * Get the total number of assets for a product.
     */
    [HttpGet("{productId}/count")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAssetsAmount(string productId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.GetProductAssetsAmount(productId);
    }
    
    
    /*
     * GET /products/{productId}/assets/{priority}
     * Get a product's asset by priority.
     */
    [HttpGet("{productId}/assets/{priority}")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAsset(string productId, string priority)
    {
	    //TODO Change this method, so it can use the resizing algorithm
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.GetProductAsset(productId, priority);
    }
    
    /*
     * POST /products/{productId}/assets
     * Associate an existing asset with a product
     */
    [HttpPost("{productId}/assets")]
    public async Task<IActionResult> AssignProductAsset(string productId, [FromBody] AddProductAssetRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.AssignProductAsset(productId, body);
    }
    
    /*
     * DELETE /products/{productId}/assets/{assetId}
     * Remove an asset from a product.
     */
    [HttpDelete("{productId}/assets/{assetId}")]
    public async Task<IActionResult> RemoveProductAsset(string productId, string assetId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.UnassignProductAsset(productId, assetId);
    }
    
    
    /*
     * PATCH /products/{productId}/assets/{assetId}
     * Update relationship metadata (like priority) between product and asset.
     */
    [HttpPatch("{productId}/assets/{assetId}")]
    public async Task<IActionResult> PatchProductAsset(string productId, string assetId, [FromBody] JsonPatchDocument<ProductAsset> patchDoc)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await productService.PatchProductAsset(productId, assetId, patchDoc);
    }
    
    
     /*
     * GET /products/{productId}/assets/gallery
     * Gets all assets that are not associated with a specific product.
    */
    [HttpGet("{productId}/assets/gallery")]
    public async Task<IActionResult> GetProductGallery(string productId, [FromQuery] string? searchString = null, [FromQuery] string? selectedTagIds = null, [FromQuery] int? amount = null, [FromQuery] int? page = null)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await productService.GetProductGallery(productId, searchString, selectedTagIds, amount, page);
    }


    [HttpGet("syncWithPim")]
    public async Task<IActionResult> SynchronizeWithPim ()
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await productService.GetProductsFromPIM();
    }

    [HttpGet("{productId}/{priority}")]
    public async Task<IActionResult> GetAssetResizedByNewWidth(string productId, int priority,
	    [FromQuery] int? newWidth = null)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await productService.GetAssetResizedByNewWidth(productId, priority, newWidth);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCountOfProducts([FromQuery] string? searchString = null)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await productService.GetCountOfProducts(searchString);
    }
    
    [HttpGet("{productId}/assets/gallery/count")]
    public async Task<IActionResult> GetCountOfAssetsNotOnProduct (string productId, [FromQuery] string? searchString = null, [FromQuery] string? selectedTagIds = null)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await productService.GetCountOfAssetsNotOnProduct(productId, searchString, selectedTagIds);
    }
}