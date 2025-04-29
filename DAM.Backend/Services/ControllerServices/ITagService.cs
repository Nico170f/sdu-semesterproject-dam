using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Services.ControllerServices;

public interface ITagService
{
    Task<IActionResult> GetAllTags();
    Task<IActionResult> CreateTag(CreateTagRequest body);
    Task<IActionResult> DeleteTag(string tagId);
}