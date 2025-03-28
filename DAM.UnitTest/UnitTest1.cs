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
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public async Task TestGetProductAssets()
    {
        
        // Object
        IAssetService _assetService = new AssetService();
        
        // Act
        IActionResult actionResult = await _assetService.GetProductAssets("test");
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        string result = okResult.Value?.ToString();
        
        Assert.Equal("Product ID: test", result);
    }

    [Test]
    public async Task TestGetImage()
    {
        
    }
    

    [Test]
    public async Task TestCreateImage()
    {
        IAssetService _assetService = new AssetService();
        
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