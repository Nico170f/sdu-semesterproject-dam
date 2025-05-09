namespace DAM.Presentation.Services;

public class DeleteService
{
	/// <summary>
	/// Deletes a tag from the database via the api.
	/// </summary>
	/// <param name="tagId"></param>
	public async Task DeleteTag(string tagId)
	{
		// Make a new HttpClient
		using HttpClientHandler handler = new HttpClientHandler();
		using HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/") // Base URL
		};
    
		// Send DELETE request to the backend API
		var response = await Http.DeleteAsync($"api/v1/tags/{tagId}");
    
		if (response.IsSuccessStatusCode)
		{
			Console.WriteLine($"Tag \"{tagId}\" deleted successfully.");
		}
		else
		{
			var error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
	
	/// <summary>
	/// Removes a relationship between an asset and a tag via the api.
	/// </summary>
	/// <param name="assetId"></param>
	/// <param name="tagId"></param>
	public async Task RemoveTagFromAsset(string assetId, string tagId)
	{
		// Make a new HttpClient
		using HttpClientHandler handler = new HttpClientHandler();
		using HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/") // Base URL
		};
    
		// Send DELETE request to the backend API
		var response = await Http.DeleteAsync($"api/v1/assets/{assetId}/tags/{tagId}");
    
		if (response.IsSuccessStatusCode)
		{
			Console.WriteLine($"Tag \"{tagId}\" removed from asset \"{assetId}\" successfully.");
		}
		else
		{
			var error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
	
	/// <summary>
	/// Removes the relationship between a product and an asset via the api.
	/// </summary>
	/// <param name="productId"></param>
	/// <param name="assetId"></param>
	public async Task RemoveAssetFromProduct(string productId, string assetId)
	{
		// Make a new HttpClient
		using HttpClientHandler handler = new HttpClientHandler();
		using HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/") // Base URL
		};
		// Post to the backend via HTTP
		var response = await Http.DeleteAsync($"api/v1/products/{productId}/assets/{assetId}");

		if (response.IsSuccessStatusCode) {
			Console.WriteLine($"Asset \"{assetId}\" removed from product \"{productId}\" successfully.");
		} 
		else {
			var error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
}