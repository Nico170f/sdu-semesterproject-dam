using DAM.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Services.ControllerServices;

public interface ITagService
{
    Task<IActionResult> CreateTag(CreateTagRequest body);
    Task<IActionResult> DeleteTag(Guid tagId);
    Task<IActionResult> GetTags(string? searchString, int? amount, int? page);
    Task<IActionResult> GetCountOfTags(string? searchString, Guid? assetId);
}