using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace DAM.Backend.Services.ControllerServices;

public interface IAssetService
{
    Task<IActionResult> CreateImage(CreateImageRequest body);
    Task<IActionResult> GetAssetsPage(int? size, int? offset);
    Task<IActionResult> GetImageById(string assetId, int? height, int? width);
    Task<IActionResult> UpdateAsset(string assetId, UpdateAssetRequest body);
    Task<IActionResult> PatchAsset(string assetId, JsonPatchDocument<Image> patchDoc);
    Task<IActionResult> DeleteAsset(string assetId);
    Task<IActionResult> GetImageIdPileFromSearch(int size, int offset, string? searchQuery);
    Task<IActionResult> GetAssetTagsGallery(string assetId);
    Task<IActionResult> GetAssetTags(string assetId);
    Task<IActionResult> AddAssetTag(string assetId, string tagId);
    Task<IActionResult> RemoveAssetTag(string assetId, string tagId);



    // Task<IActionResult> GetProductAssetsIds(string productId);
    // Task<IActionResult> GetProductAssetAmount(string productId);
    // Task<IActionResult> GetProductImage(string productId, string priority);
    // Task<IActionResult> CreateImage(CreateImageRequest requestParametre);
    // Task<IActionResult> PatchProductImage(string productId, string imageId, JsonPatchDocument<ProductImage> patchDocument);
    // Task<IActionResult> AddProductImage(string productId, AddProductImageRequest request);
    // Task<IActionResult> RemoveProductImage(string productId, RemoveProductImageRequest request);
    // Task<IActionResult> GetProductGallery(string productId);
    // Task<IActionResult> CreateMockProduct(CreateMockProductRequest requestParametre);
    // Task<IActionResult> GetProduct(string productId);
    // Task<IActionResult> GetProductsFromPIM();
    // Task<IActionResult> DeleteAllProducts();
}
