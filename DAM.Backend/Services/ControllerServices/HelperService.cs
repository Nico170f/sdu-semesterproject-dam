using DAM.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DAM.Backend.Services.ControllerServices;

public static class HelperService
{
	public static string DefaultImage = "";
	
	public static FileContentResult ConvertAssetToFileContent(Asset finalAsset)
	{
		string content = finalAsset.Content;
		string contentType;
		string base64Data;
    
		// Check if content has the data URL format (data:type;base64,...)
		if (content.Contains(";base64,"))
		{
			var assetParts = content.Split(";base64,");
			contentType = assetParts[0].Substring(5); // Remove "data:" prefix
			base64Data = assetParts[1];
		}
		else
		{
			// Assume it's raw base64 data with default content type
			contentType = "image/png";
			base64Data = content;
		}
    
		byte[] bytes = Convert.FromBase64String(base64Data);
		return new FileContentResult(bytes, contentType);
	}
    
    public static (int Width, int Height) GetAssetDimensions(string base64Asset)
    {
        var base64Data = base64Asset.Contains(',') ? base64Asset.Split(',')[1] : base64Asset;
        byte[] assetBytes = Convert.FromBase64String(base64Data);

        using var ms = new MemoryStream(assetBytes);
        using var asset = Image.Load<Rgba32>(ms);
        return (asset.Width, asset.Height);
    }
    
    public static string ResizeBase64WithPadding(Asset currentAsset, int? newWidth = null, int? newHeight = null)
    {
	    if (newWidth is null && newHeight is null)
		    throw new ArgumentException("At least one of newWidth or newHeight must be provided.");
    
	    // Extract the base64 data portion after the comma
	    string content = currentAsset.Content;
	    string base64Data = content.Contains(",") ? content.Split(',')[1] : content;
    
	    byte[] assetBytes = Convert.FromBase64String(base64Data);
	    using var asset = Image.Load<Rgba32>(assetBytes);

	    // Rest of the method remains the same
	    int originalWidth = asset.Width;
	    int originalHeight = asset.Height;

	    float ratio;

	    if (newWidth.HasValue && newHeight.HasValue)
	    {
		    ratio = Math.Min((float)newWidth.Value / originalWidth, (float)newHeight.Value / originalHeight);
	    }
	    else if (newWidth.HasValue)
	    {
		    ratio = (float)newWidth.Value / originalWidth;
		    newHeight = (int)(originalHeight * ratio);
	    }
	    else // only newHeight.HasValue
	    {
		    ratio = (float)newHeight.Value / originalHeight;
		    newWidth = (int)(originalWidth * ratio);
	    }

	    int resizedWidth = (int)(originalWidth * ratio);
	    int resizedHeight = (int)(originalHeight * ratio);

	    asset.Mutate(x => x.Resize(resizedWidth, resizedHeight));

	    using var canvas = new Image<Rgba32>(newWidth.Value, newHeight.Value, new Rgba32(0, 0, 0, 0));

	    int posX = (newWidth.Value - resizedWidth) / 2;
	    int posY = (newHeight.Value - resizedHeight) / 2;

	    canvas.Mutate(x => x.DrawImage(asset, new Point(posX, posY), 1f));

	    using var ms = new MemoryStream();
	    canvas.Save(ms, new PngEncoder());
	    return Convert.ToBase64String(ms.ToArray());
    }
}