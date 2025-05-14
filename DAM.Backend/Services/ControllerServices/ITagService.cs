using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Services.ControllerServices;

public interface ITagService
{
    Task<IActionResult> GetTags(string? searchString, int? amount, int? page);
    Task<IActionResult> CreateTag(CreateTagRequest body);
    Task<IActionResult> DeleteTag(string tagId);
    Task<IActionResult> GetAssetsTags(GetAssetsTagsRequest body);
    Task<IActionResult> GetCountOfTags(string? searchString, string? assetId);
}