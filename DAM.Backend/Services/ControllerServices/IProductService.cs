using DAM.Shared.Models;
using DAM.Shared.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Services.ControllerServices;

public interface IProductService
{
	Task<IActionResult> GetProducts (string? searchString, int? amount, int? page);
    Task<IActionResult> CreateMockProduct(CreateMockProductRequest body);
    Task<IActionResult> CreateProduct(CreateProductRequest body);
    Task<IActionResult> GetProduct(string productId);
    Task<IActionResult> GetProductAssets(string productId);
    Task<IActionResult> GetProductAssetsAmount(string productId);
    Task<IActionResult> GetProductAsset(string productId, string priority);
    Task<IActionResult> AssignProductAsset(string productId, AddProductAssetRequest body);
    Task<IActionResult> UnassignProductAsset(string productId, string assetId);
    Task<IActionResult> PatchProductAsset(string productId, string assetId, JsonPatchDocument<ProductAsset> body);
    Task<IActionResult> GetProductGallery(string productId, string? searchString, string? selectedTagIds, int? amount, int? page);
    Task<IActionResult> GetProductsFromPIM();
    Task<IActionResult> GetAssetResizedByNewWidth(string productId, int priority, int? newWidth);
    Task<IActionResult> GetAssetResizedByFactor(string productId, int priority, int scaleFactor);

    Task<IActionResult> GetCountOfAssetsNotOnProduct(string? productId, string? searchString, string? selectedTagIds);
    Task<IActionResult> GetCountOfProducts(string? searchString);


}