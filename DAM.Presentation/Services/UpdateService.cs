using System.Net.Http.Json;
using Microsoft.AspNetCore.JsonPatch;
using DAM.Presentation.Models;

namespace DAM.Presentation.Services;

public class UpdateService : BaseService
{

	public UpdateService (IHttpClientFactory httpClientFactory) : base(httpClientFactory)
	{
	}

	/// <summary>
	/// Updates the relationship priority between a product and an asset.
	/// </summary>
	/// <param name="productId"></param>
	/// <param name="assetId"></param>
	/// <param name="newPriority"></param>
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

		var response = await _httpClient.PatchAsJsonAsync($"api/v1/products/{productId}/assets/{assetId}", patchOperation);

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