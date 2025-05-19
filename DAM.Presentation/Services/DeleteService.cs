namespace DAM.Presentation.Services;

public class DeleteService(IHttpClientFactory httpClientFactory) : BaseService(httpClientFactory)
{

	/// <summary>
	/// Deletes a tag from the database via the API.
	/// </summary>
	/// <param name="tagId">The ID of the tag to delete.</param>
	public async Task DeleteTag(Guid tagId)
	{
		var response = await HttpClient.DeleteAsync($"api/v1/tags/{tagId}");
    
		if (response.IsSuccessStatusCode)
		{
			Console.WriteLine($"Tag \"{tagId}\" deleted successfully.");
		}
		else
		{
			string error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
	
	/// <summary>
	/// Removes a relationship between an asset and a tag via the API.
	/// </summary>
	/// <param name="assetId">The ID of the asset.</param>
	/// <param name="tagId">The ID of the tag.</param>
	public async Task RemoveTagFromAsset(Guid assetId, Guid tagId)
	{
		var response = await HttpClient.DeleteAsync($"api/v1/assets/{assetId}/tags/{tagId}");
    
		if (response.IsSuccessStatusCode)
		{
			Console.WriteLine($"Tag \"{tagId}\" removed from asset \"{assetId}\" successfully.");
		}
		else
		{
			string error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
	
	/// <summary>
	/// Removes the relationship between a product and an asset via the API.
	/// </summary>
	/// <param name="productId">The ID of the product.</param>
	/// <param name="assetId">The ID of the asset.</param>
	public async Task RemoveAssetFromProduct(Guid productId, Guid assetId)
	{
		var response = await HttpClient.DeleteAsync($"api/v1/products/{productId}/assets/{assetId}");

		if (response.IsSuccessStatusCode) {
			Console.WriteLine($"Asset \"{assetId}\" removed from product \"{productId}\" successfully.");
		} 
		else {
			string error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}

	/// <summary>
	/// Deletes an asset from the database via the API.
	/// </summary>
	/// <param name="assetId">The ID of the asset to delete.</param>
	public async Task DeleteAsset(Guid assetId)
	{
		var response = await HttpClient.DeleteAsync($"api/v1/Assets/{assetId}");

		if (response.IsSuccessStatusCode)
		{
			Console.WriteLine($"Asset \"{assetId}\" was deleted successfully");
		}
		else
		{
			string error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
}
