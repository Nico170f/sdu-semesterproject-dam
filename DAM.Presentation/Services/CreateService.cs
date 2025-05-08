using System.Net.Http.Json;
using DAM.Presentation.Models;
using DAM.Presentation.Services.API;
using Microsoft.AspNetCore.Components.Forms;

namespace DAM.Presentation.Services;

public class CreateService
{

	/// <summary>
	/// Allows uploading of products with both name and uuid to the database.
	/// Used for testing purposes at the moment.
	/// </summary>
	/// <param name="product"></param>
	public async Task UploadProductWithUUID (Product product)
	{
		var payload = new CreateProductRequest()
		{
			Name = product.Name,
			UUID = product.UUID
		};
		
		// Make a new HttpClient
		using HttpClientHandler handler = new HttpClientHandler();
		using HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/")
		};
		// Post to the backend via HTTP
		var response = await Http.PostAsJsonAsync($"api/v1/products/add", payload);
		
		if (response.IsSuccessStatusCode)
		{
			Console.WriteLine($"Product \"{product.Name}\" uploaded successfully.");
		}
		else
		{
			var error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
	
	
	/// <summary>
	/// Uploads an image to the database via the api.
	/// </summary>
	/// <param name="e"></param>
	public async Task UploadImage(InputFileChangeEventArgs e)
	{
		// Select a file 
		IBrowserFile file = e.File;

		// Finding the datatype of the file
		var datatype = file.ContentType;
		using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB limit
		using var ms = new MemoryStream();
		await stream.CopyToAsync(ms);
		var bytes = ms.ToArray();
		string imageString = Convert.ToBase64String(bytes);
		string dataUrl = $"data:{datatype};base64,{imageString}";
		//Console.WriteLine(dataUrl);

		// Make payload for uploading an image to the backend
		var payload = new CreateImageRequest()
		{
			Content = dataUrl
		};

		// Make a new HttpClient
		using HttpClientHandler handler = new HttpClientHandler();
		using HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/") // Replace with your API's base URL
		};
		// Post to the backend via HTTP
		var response = await Http.PostAsJsonAsync("api/v1/assets", payload);

		if (response.IsSuccessStatusCode)
		{
			Console.WriteLine($"Image \"{file.Name}\" uploaded successfully.");
		}
		else
		{
			var error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
	
	/// <summary>
	/// Uploads a tag to the database via the api. 
	/// </summary>
	/// <param name="tagName"></param>
	public async Task UploadTag(string tagName)
	{
		var payload = new CreateTagRequest
		{
			Name = tagName
		};
		
		// Make a new HttpClient
		using HttpClientHandler handler = new HttpClientHandler();
		using HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/") // Replace with your API's base URL
		};
		// Post to the backend via HTTP
		var response = await Http.PostAsJsonAsync($"api/v1/tags", payload);
		
		if (response.IsSuccessStatusCode)
		{
			Console.WriteLine($"Tag \"{tagName}\" uploaded successfully.");
		}
		else
		{
			var error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
	
	/// <summary>
	/// Makes a relationship between an asset and a tag via the api.
	/// </summary>
	/// <param name="assetId"></param>
	/// <param name="tagId"></param>
	public async Task AddTagToImage(string assetId, string tagId)
	{
		// Make a new HttpClient
		using HttpClientHandler handler = new HttpClientHandler();
		using HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/") // Replace with your API's base URL
		};
		// Post to the backend via HTTP
		var response = await Http.PostAsync($"api/v1/assets/{assetId}/tags/{tagId}", null);
		
		if (response.IsSuccessStatusCode) {
			Console.WriteLine($"Tag \"{tagId}\" added to asset \"{assetId}\" successfully.");
		} 
		else {
			var error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
	
	/// <summary>
	/// Makes a relationship with the specified priority between a product and an asset via the api.
	/// </summary>
	/// <param name="productId"></param>
	/// <param name="assetId"></param>
	/// <param name="priority"></param>
	public async Task AddAssetToProduct(string productId, string assetId, string priority)
	{
		var payload = new AddProductImageRequest()
		{
			ImageId = assetId,
			Priority = priority
		};

		// Make a new HttpClient
		using HttpClientHandler handler = new HttpClientHandler();
		using HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/")
		};
		// Post to the backend via HTTP
		var response = await Http.PostAsJsonAsync($"api/v1/products/{productId}/assets", payload);

		if (response.IsSuccessStatusCode) {
			Console.WriteLine($"Asset \"{assetId}\" added to product \"{productId}\" successfully.");
		} 
		else {
			var error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
}
