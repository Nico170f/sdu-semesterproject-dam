using DAM.Shared.Models;
using DAM.Shared.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace DAM.Backend.Services.ControllerServices;

public interface IAssetService
{
    Task<IActionResult> CreateAsset(CreateAssetRequest body);
    Task<IActionResult> GetAssets(string? searchString, string? selectedTagIds, int? amount, int? page);
    Task<IActionResult> GetAssetById(Guid assetId, int? width, int? height);
    Task<IActionResult> UpdateAsset(Guid assetId, UpdateAssetRequest body);
    Task<IActionResult> PatchAsset(Guid assetId, JsonPatchDocument<Asset> patchDoc);
    Task<IActionResult> DeleteAsset(Guid assetId);
    Task<IActionResult> GetAssetIdPileFromSearch(int size, int offset, string? searchQuery);
    Task<IActionResult> GetAssetTagsGallery(Guid assetId, string? searchString, int? amount, int? page);
    Task<IActionResult> GetAssetTags(Guid assetId);
    Task<IActionResult> AddAssetTag(Guid assetId, Guid tagId);
    Task<IActionResult> RemoveAssetTag(Guid assetId, Guid tagId);
    Task<IActionResult> GetCountOfAssets(string? searchString, string? selectedTagIds);
}
