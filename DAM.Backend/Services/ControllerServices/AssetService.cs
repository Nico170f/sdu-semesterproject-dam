using DAM.Backend.Controllers.API;
using DAM.Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using DAM.Backend.Data;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Image = DAM.Backend.Data.Models.Image;
using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DAM.Backend.Services.ControllerServices;

public class AssetService : IAssetService
{

    private readonly IConfiguration _configuration;
    private readonly Database _database;

    public AssetService(IConfiguration configuration, Database database)
    {
        _configuration = configuration;
        _database = database;
    }


    public async Task<IActionResult> CreateImage(CreateImageRequest body)
    {
        if (body.Content.Length < 30)
        {
            return new BadRequestObjectResult("Image content is too short");
        }

        Image image = new Image
        {
            UUID = Guid.NewGuid(),
            Content = body.Content,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        (int Width, int Height) dimensions = HelperService.GetImageDimensions(image.Content);
        image.Width = dimensions.Width;
        image.Height = dimensions.Height;

        _database.Images.Add(image);

        int imageCreated = await _database.SaveChangesAsync();
        if (imageCreated <= 0)
        {
            return new BadRequestObjectResult("Failed to create image");
        }

        CreateImageResponse response = new CreateImageResponse(image);
        return new OkObjectResult(response);
    }


    public async Task<IActionResult> GetAssetsPage(int? size, int? page)
    {
        List<Guid> uuids = await _database.Images.Select(img => img.UUID).ToListAsync();
        return new OkObjectResult(uuids);
    }


    public async Task<IActionResult> GetImageById(string assetId, int? height, int? width)
    {
        Image? finalImage = null;
        Guid? imageUUID = HelperService.ParseStringGuid(assetId);
        if (imageUUID != null)
        {
            finalImage = await _database.Images
                .FirstOrDefaultAsync(i => i.UUID == imageUUID);
        }

        if (finalImage == null)
        {
            finalImage = GetDefaultImage();
        }

        if (height.HasValue || width.HasValue)
        {
            finalImage.Content = HelperService.ResizeBase64WithPadding(finalImage, height, width);
        }

        FileContentResult fileContentResult = HelperService.ConvertImageToFileContent(finalImage);
        return fileContentResult;
    }



    public async Task<IActionResult> UpdateAsset(string imageId, UpdateAssetRequest requestParams)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(imageId);
        if (imageUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        Image? image = await _database.Images
            .FirstOrDefaultAsync(i => i.UUID == imageUUID);
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }

        image.Content = requestParams.Content;
        image.UpdatedAt = DateTime.Now;

        (int Width, int Height) dimensions = HelperService.GetImageDimensions(image.Content);
        image.Width = dimensions.Width;
        image.Height = dimensions.Height;

        bool imageUpdated = await _database.Update(image);
        if (!imageUpdated)
        {
            return new BadRequestObjectResult("Failed to update image");
        }

        return new OkObjectResult("Image updated successfully");
    }



    public async Task<IActionResult> PatchAsset(string assetId, JsonPatchDocument<Image> patchDoc)
    {
        if (patchDoc == null)
        {
            return new BadRequestObjectResult("Patch document cannot be null");
        }

        Guid? imageUUID = HelperService.ParseStringGuid(assetId);
        if (imageUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        Image? image = await _database.Images
            .FirstOrDefaultAsync(i => i.UUID == imageUUID);

        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }

        patchDoc.ApplyTo(image);
        image.UpdatedAt = DateTime.Now;

        (int Width, int Height) dimensions = HelperService.GetImageDimensions(image.Content);
        image.Width = dimensions.Width;
        image.Height = dimensions.Height;

        bool updateResult = await _database.SaveChangesAsync() > 0;
        if (!updateResult)
        {
            return new BadRequestObjectResult("Failed to update image");
        }

        return new OkObjectResult("Image updated successfully");
    }


    public async Task<IActionResult> DeleteAsset(string assetId)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(assetId);
        if (imageUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        var image = await _database.Images.FindAsync(imageUUID);
        if (image == null)
        {
            return new NotFoundObjectResult("No image found by that UUID");
        }

        var deleted = await _database.Delete(image);
        if (!deleted)
        {
            return new BadRequestObjectResult("Failed to delete image");
        }

        var productImages = await _database.ProductImages
            .Where(pi => pi.ImageUUID == imageUUID)
            .ToListAsync();
        if (productImages.Any())
        {
            foreach (var productImage in productImages)
            {
                await _database.Delete(productImage);
            }
        }

        return new OkObjectResult("Image deleted successfully");
    }


    public async Task<IActionResult> GetImageIdPileFromSearch(int size, int offset, string? searchquery)
    {
	    searchquery = searchquery ?? "";
        List<Guid> imageIds = await _database.ProductImages
            .Join(_database.Products,
                pi => pi.ProductUUID,
                p => p.UUID,
                (pi, p) => new { ProductImage = pi, Product = p })
            .Where(joined => joined.Product.Name.Contains(searchquery))
            .OrderBy(joined => joined.Product.Name)
            .Skip(offset)
            .Take(size)
            .Select(joined => joined.ProductImage.ImageUUID)
            .ToListAsync();

        return new OkObjectResult(imageIds);
    }


    public async Task<IActionResult> GetAssetTagsGallery(string assetId)
    {
        //Hvad sker der lige her? Mathias....
        Guid ImageUUID = HelperService.ParseStringGuid(assetId).Value;

        List<Tag> tagsNotOnImage = new List<Tag>();

        tagsNotOnImage = await _database.Tags
            .Where(tag => !_database.ImageTags
                .Any(it => it.ImageUUID == ImageUUID && it.TagUUID == tag.UUID))
            .ToListAsync();

        return new OkObjectResult(tagsNotOnImage);
    }
    

    public async Task<IActionResult> GetAssetTags(string assetId)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(assetId);
        if (imageUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        List<Tag> imageTagsList = new List<Tag>();

        imageTagsList = await _database.Tags
            .Where(tag => _database.ImageTags
                .Any(it => it.ImageUUID == imageUUID && it.TagUUID == tag.UUID))
            .ToListAsync();

        if (imageTagsList == null || imageTagsList.Count == 0)
        {
            return new NotFoundObjectResult("No tags found for that UUID");
        }

        return new OkObjectResult(imageTagsList);
    }


    public async Task<IActionResult> AddAssetTag(string assetId, string tagId)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(assetId);
        Guid? tagUUID = HelperService.ParseStringGuid(tagId);

        if (imageUUID == null || tagUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        try
        {
            var image = await _database.Images.FindAsync(imageUUID);
            if (image == null)
            {
                return new BadRequestObjectResult("Image not found");
            }

            var tag = await _database.Tags.FindAsync(tagUUID);
            if (tag == null)
            {
                return new NotFoundObjectResult("Tag not found");
            }

            var existingRelationship = await _database.ImageTags
                .FirstOrDefaultAsync(it => it.ImageUUID == imageUUID && it.TagUUID == tagUUID);
            if (existingRelationship != null)
            {
                return new OkObjectResult("Tag is already associated with image");
            }

            var imageTag = new ImageTags()
            {
                ImageUUID = (Guid)imageUUID,
                TagUUID = (Guid)tagUUID
            };

            await _database.ImageTags.AddAsync(imageTag);
            await _database.SaveChangesAsync();

            return new OkObjectResult("Tag associated with image completed");
        }
        catch
        {
            return new BadRequestObjectResult("Bacons mom");
        }
    }

    public async Task<IActionResult> RemoveAssetTag(string assetId, string tagId)
    {
        Guid? imageUUID = HelperService.ParseStringGuid(assetId);
        Guid? tagUUID = HelperService.ParseStringGuid(tagId);

        if (imageUUID == null || tagUUID == null)
        {
            return new BadRequestObjectResult("Invalid UUID format");
        }

        var imageTag = await _database.ImageTags
            .FirstOrDefaultAsync(it => it.ImageUUID == imageUUID && it.TagUUID == tagUUID);

        if (imageTag == null)
        {
            return new NotFoundObjectResult("Tag on image not found");
        }

        _database.ImageTags.Remove(imageTag);
        await _database.SaveChangesAsync();

        return new OkObjectResult("Tag removed from image");
    }



    // This method should probably be in the helper service
    private Image GetDefaultImage()
    {
        Image image = new Image
        {
            Content = _configuration.GetSection("DefaultImages")["NotFound"] ??
                      throw new Exception("No default image found")
        };

        return image;
    }

    // public async Task<IActionResult> GetImageIdPile(int size, int offset) {
    //     int currentRowNumber = offset;
    //     List<Guid> imageIds = await _database.Images
    //         .Select(img => img.UUID)
    //         .OrderBy(uuid => uuid)
    //         .Skip(offset)
    //         .Take(size)
    //         .ToListAsync();
    //     
    //     return new OkObjectResult(imageIds);
    // }
}