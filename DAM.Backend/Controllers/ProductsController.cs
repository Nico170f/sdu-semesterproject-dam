using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using DAM.Backend.Services.ControllerServices;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Controllers;

public class ProductsController : ApiController
{

    private readonly IProductService _productService;
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    /*
     * GET /products
     * Gets all products
     */
    [HttpGet()]
    public async Task<IActionResult> GetAllProducts()
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await _productService.GetAllProducts();
    }
        
    /*
     * POST /products/mock
     * Add a mock product.
     */
    [HttpPost("mock")]
    public async Task<IActionResult> CreateMockProduct([FromBody] CreateMockProductRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _productService.CreateMockProduct(body);
    }
    
    /*
     * POST /products/add
     * Add a product with both name and uuid
     */
    [HttpPost("add")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest body)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await _productService.CreateProduct(body);
    }
    
    
    /*
     * GET /products/{productId}
     * Get product by id
     */
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProduct(string productId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _productService.GetProduct(productId);
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
        return await _productService.GetProductAssets(productId);
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
        return await _productService.GetProductAssetsAmount(productId);
    }
    
    
    /*
     * GET /products/{productId}/assets/{priority}
     * Get a product's asset by priority.
     */
    [HttpGet("{productId}/assets/{priority}")]
    //[AllowAnonymous]
    public async Task<IActionResult> GetProductAsset(string productId, string priority)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _productService.GetProductAsset(productId, priority);
    }
    
    /*
     * POST /products/{productId}/assets
     * Associate an existing asset with a product
     */
    [HttpPost("{productId}/assets")]
    public async Task<IActionResult> AssignProductAsset(string productId, [FromBody] AddProductAssetRequest body)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _productService.AssignProductAsset(productId, body);
    }
    
    /*
     * DELETE /products/{productId}/assets/{assetId}
     * Remove an asset from a product.
     */
    [HttpDelete("{productId}/assets/{assetId}")]
    public async Task<IActionResult> RemoveProductAsset(string productId, string assetId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _productService.UnassignProductAsset(productId, assetId);
    }
    
    
    /*
     * PATCH /products/{productId}/assets/{assetId}
     * Update relationship metadata (like priority) between product and asset.
     */
    [HttpPatch("{productId}/assets/{assetId}")]
    public async Task<IActionResult> PatchProductAsset(string productId, string assetId, [FromBody] JsonPatchDocument<ProductAsset> patchDoc)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await _productService.PatchProductAsset(productId, assetId, patchDoc);
    }
    
    
     /*
     * GET /products/{productId}/assets/gallery
     * Gets all assets that are not associated with a specific product.
    */
    [HttpGet("{productId}/assets/gallery")]
    public async Task<IActionResult> GetProductGallery(string productId)
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await _productService.GetProductGallery(productId);
    }


    [HttpGet("syncWithPim")]
    public async Task<IActionResult> SynchronizeWithPim ()
    {
	    if (!ModelState.IsValid) return BadRequest(ModelState);
	    return await _productService.GetProductsFromPIM();
    }
}