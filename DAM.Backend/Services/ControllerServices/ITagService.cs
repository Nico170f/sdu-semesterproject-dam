using DAM.Backend.Controllers.API;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Services.ControllerServices;

public interface ITagService
{
    Task<IActionResult> GetImageTag(string imageId);
    Task<IActionResult> GetTags();
    Task<IActionResult> AddTagsToImage(string imageId, AddTagsToImageRequest requestParametre);
}