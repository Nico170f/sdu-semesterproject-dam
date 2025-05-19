using DAM.Shared.Models;
using DAM.Shared.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Services.ControllerServices;

public interface IAssetService
{
    Task<IActionResult> CreateAsset(CreateAssetRequest body);
    Task<IActionResult> DeleteAsset(Guid assetId);
    
    Task<IActionResult> GetAssets(string? searchString, string? selectedTags, int? amount, int? page);
    Task<IActionResult> GetAssetsOnProduct(Guid productId);
    Task<IActionResult> GetAssetsGallery(Guid productIdToAvoid, string? searchString, string? selectedTags, int? amount, int? page);
    
    Task<IActionResult> GetAssetContent(Guid assetId, int? width, int? height);
    
    Task<IActionResult> AddAssetTag(Guid assetId, Guid tagId);
    Task<IActionResult> RemoveAssetTag(Guid assetId, Guid tagId);
    
    Task<IActionResult> UpdateAsset(Guid assetId, UpdateAssetRequest body);
    Task<IActionResult> PatchAsset(Guid assetId, JsonPatchDocument<Asset> patchDoc);
    
}
