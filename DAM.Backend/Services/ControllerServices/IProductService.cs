using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Services.ControllerServices;

public interface IProductService
{
	Task<IActionResult> GetAllProducts ();
    Task<IActionResult> CreateMockProduct(CreateMockProductRequest body);
    Task<IActionResult> CreateProduct(CreateProductRequest body);
    Task<IActionResult> GetProduct(string productId);
    Task<IActionResult> GetProductAssets(string productId);
    Task<IActionResult> GetProductAssetsAmount(string productId);
    Task<IActionResult> GetProductAsset(string productId, string priority);
    Task<IActionResult> AssignProductAsset(string productId, AddProductAssetRequest body);
    Task<IActionResult> UnassignProductAsset(string productId, string assetId);
    Task<IActionResult> PatchProductAsset(string productId, string assetId, JsonPatchDocument<ProductAsset> body);
    Task<IActionResult> GetProductGallery(string productId);
    Task<IActionResult> GetProductsFromPIM();
}