using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using Image = DAM.Backend.Data.Models.Image;
namespace DAM.Backend.Services;

public static class HelperService
{
    public static FileContentResult ConvertImageToFileContent(Image finalImage)
    { 
        var imageParts = finalImage.Content.Split(";base64,");
        var imageType = imageParts[0].Substring(5);
        
        byte[] imageBytes = Convert.FromBase64String(imageParts[1]);
        return new FileContentResult(imageBytes, imageType);
    }
    
    private static bool IsValidId(string id)
    {
        return Guid.TryParse(id, out Guid _);
    }
    
    public static (int Width, int Height) GetImageDimensions(string base64Image)
    {
        var base64Data = base64Image.Contains(",") ? base64Image.Split(',')[1] : base64Image;
        byte[] imageBytes = Convert.FromBase64String(base64Data);

        using var ms = new MemoryStream(imageBytes);
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(ms);
        return (image.Width, image.Height);
    }

    public static int? GetImagePriority(string priorityString)
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

    public static async Task<string> ResizeImageWiedthAsync(string base64Image, int newWidth)
    {
        byte[] imageBytes = Convert.FromBase64String(base64Image);
        
        using (var inputStream = new MemoryStream(imageBytes))
        using (SixLabors.ImageSharp.Image image = await SixLabors.ImageSharp.Image.LoadAsync(inputStream))
        {
            var aspectRatio = (double)image.Height / image.Width;
            int newHeight = (int)(newWidth * aspectRatio);

            image.Mutate(x => x.Resize(newWidth, newHeight));

            using (var outputStream = new MemoryStream())
            {
                await image.SaveAsync(outputStream, new JpegEncoder{Quality = 85});
                return Convert.ToBase64String(outputStream.ToArray());
            }
        }
    }

    public static async Task<string> ResizeImageFactorAsync(string base64Image, int scaleFactor)
    {
        if (scaleFactor <= 0)
            throw new ArgumentOutOfRangeException(nameof(scaleFactor), "Scale factor must be greater than 0");

        byte[] imageBytes = Convert.FromBase64String(base64Image);
        
        using (var inputStream = new MemoryStream(imageBytes))
        using (SixLabors.ImageSharp.Image image = await SixLabors.ImageSharp.Image.LoadAsync(inputStream))
        {
            int newWidth = (int)(image.Width * scaleFactor);
            int newHeight = (int)(image.Height * scaleFactor);
            
            image.Mutate(x=> x.Resize(newWidth, newHeight));

            using (var outputStream = new MemoryStream())
            {
                var format = image.Metadata.DecodedImageFormat;
                IImageEncoder encoder = GetEncoder(format);

                await image.SaveAsync(outputStream, encoder);
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
        
        throw new NotSupportedException($"Unsupported image format: {format.Name}");
    }
}