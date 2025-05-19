using DAM.Shared.Models;
using DAM.Shared.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Services.ControllerServices;

public interface IProductService
{
    Task<IActionResult> GetProduct(Guid productId);
    
	Task<IActionResult> GetProducts (string? searchString, int? amount, int? page);
    Task<IActionResult> GetProductAssets(Guid productId);
    Task<IActionResult> GetProductAssetsAmount(Guid productId);
    
    Task<IActionResult> GetProductAsset(Guid productId, int priority, int? width = null, int? height = null);
    
    Task<IActionResult> AssignProductAsset(Guid productId, AddProductAssetRequest body);
    Task<IActionResult> UnassignProductAsset(Guid productId, Guid assetId);
    Task<IActionResult> PatchProductAsset(Guid productId, Guid assetId, JsonPatchDocument<ProductAsset> body);
    
    Task<IActionResult> GetProductsFromPIM();
    
    Task<IActionResult> CreateMockProduct(CreateMockProductRequest body);
    Task<IActionResult> CreateProduct(CreateProductRequest body);

}