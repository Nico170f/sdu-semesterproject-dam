using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace DAM.Backend.Services.ControllerServices;

public interface IAssetService
{
    Task<IActionResult> GetProductAssetsIds(string productId);

    Task<IActionResult> GetProductAssetAmount(string productId);

    Task<IActionResult> GetImage(string productId, string priority);
    Task<IActionResult> CreateImage(CreateImageRequest requestParametre);
    Task<IActionResult> UpdateImage(string imageId, UpdateImageRequest requestParametre);
    Task<IActionResult> PatchImage(string imageId, JsonPatchDocument<Image> patchDocument);
    Task<IActionResult> PatchProductImage(string productId, string imageId, JsonPatchDocument<ProductImage> patchDocument);
    
    Task<IActionResult> DeleteProductImage(string productId, string imageId);
    Task<IActionResult> AddProductImage(AddProductImageRequest request);

    Task<IActionResult> DeleteImage(string imageId);
    Task<IActionResult> GetImageIdPile(int size, int offset);
    Task<IActionResult> GetImageIdPileFromSearch(int size, int offset, string searchquery);
    Task<IActionResult> GetImageByUUID(string uuid);

}