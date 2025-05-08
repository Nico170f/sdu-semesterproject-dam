using System.Net.Http.Json;

namespace DAM.Presentation.Services;

public class UpdateService
{

	/// <summary>
	/// Updates the relationship priority between a product and an asset.
	/// </summary>
	/// <param name="productId"></param>
	/// <param name="assetId"></param>
	/// <param name="newPriority"></param>
	public async Task UpdatePriority(string productId, string assetId, int newPriority)
	{
		var patchDoc = new[]
		{
			new
			{
				op = "replace",
				path = "/priority",
				value = newPriority
			}
		};

		using HttpClientHandler handler = new HttpClientHandler();
		using HttpClient http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/")
		};

		var response = await http.PatchAsJsonAsync($"api/v1/assets/{productId}/assets/{assetId}", patchDoc);

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