using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace DAM.Backend.Services.ControllerServices;

public interface IAssetService
{
    Task<IActionResult> CreateAsset(CreateAssetRequest body);
    Task<IActionResult> GetAssets(string? searchString, string? selectedTagIds, int? amount, int? page);
    Task<IActionResult> GetAssetById(string assetId, int? height, int? width);
    Task<IActionResult> UpdateAsset(string assetId, UpdateAssetRequest body);
    Task<IActionResult> PatchAsset(string assetId, JsonPatchDocument<Asset> patchDoc);
    Task<IActionResult> DeleteAsset(string assetId);
    Task<IActionResult> GetAssetIdPileFromSearch(int size, int offset, string? searchQuery);
    Task<IActionResult> GetAssetTagsGallery(string assetId);
    Task<IActionResult> GetAssetTags(string assetId);
    Task<IActionResult> AddAssetTag(string assetId, string tagId);
    Task<IActionResult> RemoveAssetTag(string assetId, string tagId);
}
