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
    Task<IActionResult> GetProduct(Guid productId);
    Task<IActionResult> GetProductAssets(Guid productId);
    Task<IActionResult> GetProductAssetsAmount(Guid productId);
    Task<IActionResult> GetProductAsset(Guid productId, int priority);
    Task<IActionResult> AssignProductAsset(Guid productId, AddProductAssetRequest body);
    Task<IActionResult> UnassignProductAsset(Guid productId, Guid assetId);
    Task<IActionResult> PatchProductAsset(Guid productId, Guid assetId, JsonPatchDocument<ProductAsset> body);
    Task<IActionResult> GetProductGallery(Guid productId, string? searchString, string? selectedTagIds, int? amount, int? page);
    Task<IActionResult> GetProductsFromPIM();
    Task<IActionResult> GetAssetResizedByNewWidth(Guid productId, int priority, int? newWidth);
    Task<IActionResult> GetCountOfAssetsNotOnProduct(Guid? productId, string? searchString, string? selectedTagIds);
    Task<IActionResult> GetCountOfProducts(string? searchString);


}