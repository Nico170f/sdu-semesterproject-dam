using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Services.ControllerServices;

public interface ITagService
{
    Task<IActionResult> CreateTag(CreateTagRequest requestParams);
    Task<IActionResult> DeleteTag(DeleteTagRequest requestParams);
    Task<IActionResult> GetImageTag(string imageId);
    Task<IActionResult> GetTags();
    Task<IActionResult> AddTagToImage(string imageId, string tagId);
    Task<IActionResult> RemoveTagFromImage(string imageId, string tagId);
    Task<IActionResult> GetTagsNotOnImage(string imageUUID);
}