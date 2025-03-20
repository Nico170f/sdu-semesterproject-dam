using DAM.Backend.Controllers;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Services.ControllerServices;

public interface IAssetService
{
    Task<IActionResult> GetProductAssets(string productId);
    Task<IActionResult> GetAssetImage(string productId, string priority);
    Task<IActionResult> CreateImage(string productId, CreateImageRequest requestParametre);
    Task<IActionResult> UpdateImage(string imageId, UpdateImageRequest requestParametre);
    Task<IActionResult> PatchImage(string imageId, PatchImageRequest requestParametre);
    Task<IActionResult> DeleteImage(string imageId);
}