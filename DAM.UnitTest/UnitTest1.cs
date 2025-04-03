//namespace DAM.UnitTest;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;
using Assert = Xunit.Assert;
using DAM.Backend.Services.ControllerServices;
using DAM.Backend.Controllers;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAM.UnitTest;

public class Tests
{
    
    // inMemorySettings = new Dictionary<string, string> {
    //     {"DefaultImages:NotFound", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUg=="}
    // };

    private Dictionary<string, string> inMemorySettings = new Dictionary<string, string>();

    private IConfiguration _configuration;
    
    private IAssetService _assetService;
    
    [SetUp]
    public void Setup()
    {
        inMemorySettings["DefaultImages:NotFound"] = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUg==";
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
        _assetService = new AssetService(_configuration);
    }

    [Test]
    public async Task TestGetProductAssets()
    {
        IActionResult actionResult = await _assetService.GetProductAssets("test");
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        string result = okResult.Value?.ToString();
        
        Assert.Equal("Product ID: test", result);
    }

    [Test]
    public async Task TestGetImage()
    {
        IActionResult actionResult = await _assetService.GetImage("nonexistentId", "1");

        var fileResult = Assert.IsType<FileContentResult>(actionResult);
        Assert.Equal("image/png", fileResult.ContentType);
    }
    

    [Test]
    public async Task TestCreateImage()
    {
        CreateImageRequest imageRequest = new CreateImageRequest();
        imageRequest.Content = "test";
        imageRequest.ProductId = System.Guid.NewGuid().ToString();
        imageRequest.IsShown = true;
        imageRequest.Priority = 1;
        
        IActionResult actionResult = await _assetService.CreateImage(imageRequest);
        
        var okResult = Assert.IsType<OkObjectResult>(actionResult); 
        string result = okResult.Value?.ToString();
        
    }

    [Test]
    public async Task TestUpdateImage()
    {
        
    }

    [Test]
    public async Task TestPatchImage()
    {
        
    }
    
    [Test]
    public async Task TestDeleteImage()
    {
        
    }
    
    [TearDown]
    public void TearDown()
    {
        
    }
}