using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.Formats;

namespace DAM.Backend.Services.ControllerServices;

public interface IHelperService
{
    FileContentResult ConvertAssetToFileContent(DAM.Backend.Data.Models.Asset finalAsset);
    bool IsValidId(string id);
    (int Width, int Height) GetAssetDimensions(string base64Asset);
    int? GetAssetPriority(string priorityString);
    Guid? ParseStringGuid(string guidString);
    Task<string> ResizeAssetByFactorAsync(string base64Asset, int scaleFactor);
    Task<string> ResizeAssetByWidthAsync(string base64Asset, int newWidth);
    IImageEncoder getEncoder(IImageFormat format);

    string ResizeBase64WithPadding(
        DAM.Backend.Data.Models.Asset currentAsset,
        int? newWidth = null,
        int? newHeight = null);
}