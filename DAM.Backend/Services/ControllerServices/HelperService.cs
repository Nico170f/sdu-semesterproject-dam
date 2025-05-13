using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
namespace DAM.Backend.Services;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;

public static class HelperService
{
    public static FileContentResult ConvertAssetToFileContent(DAM.Backend.Data.Models.Asset finalAsset)
    { 
        var assetParts = finalAsset.Content.Split(";base64,");
        var assetType = assetParts[0].Substring(5);
        
        byte[] bytes = Convert.FromBase64String(assetParts[1]);
        return new FileContentResult(bytes, assetType);
    }
    
    private static bool IsValidId(string id)
    {
        return Guid.TryParse(id, out Guid _);
    }
    
    public static (int Width, int Height) GetAssetDimensions(string base64Asset)
    {
        var base64Data = base64Asset.Contains(",") ? base64Asset.Split(',')[1] : base64Asset;
        byte[] assetBytes = Convert.FromBase64String(base64Data);

        using var ms = new MemoryStream(assetBytes);
        using var asset = SixLabors.ImageSharp.Image.Load<Rgba32>(ms);
        return (asset.Width, asset.Height);
    }

    public static int? GetAssetPriority(string priorityString)
    {
        bool isParsed = int.TryParse(priorityString, out int priority);
        if(!isParsed)
        {
            return null;
        }

        return priority;
    }


    public static Guid? ParseStringGuid(string guidString)
    {
        if (string.IsNullOrEmpty(guidString))
        {
            return null;
        }

        if (Guid.TryParse(guidString, out Guid guid))
        {
            return guid;
        }

        return null;
    }
    
    public static async Task<string> ResizeAssetFactorAsync(string base64Asset, int scaleFactor)
    {
        if (scaleFactor <= 0)
            throw new ArgumentOutOfRangeException(nameof(scaleFactor), "Scale factor must be greater than 0");
        
        byte[] assetBytes = Convert.FromBase64String(base64Asset);
        
        using (var inputStream = new MemoryStream(assetBytes))
        using (SixLabors.ImageSharp.Image asset = await SixLabors.ImageSharp.Image.LoadAsync(inputStream))
        {
            int newWidth = (int)(asset.Width * scaleFactor);
            int newHeight = (int)(asset.Height * scaleFactor);
            
            asset.Mutate(x=> x.Resize(newWidth, newHeight));

            using (var outputStream = new MemoryStream())
            {
                var format = asset.Metadata.DecodedImageFormat;
                IImageEncoder encoder = GetEncoder(format);

                await asset.SaveAsync(outputStream, encoder);
                return Convert.ToBase64String(outputStream.ToArray());
            }
        }
    }
        
    public static async Task<string> ResizeImageWidthAsync(string base64Asset, int newWidth)
    {
        byte[] assetBytes = Convert.FromBase64String(base64Asset);
        
        using (var inputStream = new MemoryStream(assetBytes))
        using (SixLabors.ImageSharp.Image asset = await SixLabors.ImageSharp.Image.LoadAsync(inputStream))
        {
            // var aspectRatio = (double)image.Height / image.Width;
            // int newHeight = (int)(newWidth * aspectRatio);

            // image.Mutate(x => x.Resize(newWidth, newHeight));
            
            using (var outputStream = new MemoryStream())
            {
                await asset.SaveAsync(outputStream, new JpegEncoder{Quality = 85});
                return Convert.ToBase64String(outputStream.ToArray());
            }
        } 
    }
    
    private static IImageEncoder GetEncoder(IImageFormat format)
    {
        if (format == JpegFormat.Instance)
            return new JpegEncoder { Quality = 85 };
        if (format == PngFormat.Instance)
            return new PngEncoder();
        if (format == WebpFormat.Instance)
            return new WebpEncoder();
        
        throw new NotSupportedException($"Unsupported asset format: {format.Name}");
    }
    
    
    public static string ResizeBase64WithPadding(
        DAM.Backend.Data.Models.Asset currentAsset,
        int? newWidth = null,
        int? newHeight = null)
    {
        if (newWidth is null && newHeight is null)
            throw new ArgumentException("At least one of newWidth or newHeight must be provided.");

        byte[] assetBytes = Convert.FromBase64String(currentAsset.Content);
        using var asset = Image.Load<Rgba32>(assetBytes);

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