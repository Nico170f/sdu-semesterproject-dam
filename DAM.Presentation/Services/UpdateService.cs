using System.Net.Http.Json;

namespace DAM.Presentation.Services;

public class UpdateService(IHttpClientFactory httpClientFactory) : BaseService(httpClientFactory)
{

	/// <summary>
	/// Updates the relationship priority between a product and an asset via the API.
	/// </summary>
	/// <param name="productId">The ID of the product.</param>
	/// <param name="assetId">The ID of the asset.</param>
	/// <param name="newPriority">The new priority value to set.</param>
	public async Task UpdatePriority(Guid productId, Guid assetId, int newPriority)
	{
		// Create an array of operations in the correct JSON Patch format
		var patchOperation = new[] 
		{
			new 
			{ 
				op = "replace", 
				path = "/priority", 
				value = newPriority 
			}
		};

		var response = await HttpClient.PatchAsJsonAsync($"api/v1/products/{productId}/assets/{assetId}", patchOperation);
		
		if (response.IsSuccessStatusCode)
		{
			Console.WriteLine("Priority updated successfully.");
		}
		else
		{
			var error = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Error: {response.StatusCode} - {error}");
		}
	}
	
}
