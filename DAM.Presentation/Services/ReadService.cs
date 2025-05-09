using System.Net.Http.Json;
using DAM.Presentation.Models;
using DAM.Presentation.Services.API;

namespace DAM.Presentation.Services;

public class ReadService
{
	
	/// <summary>
	/// Returns a list of asset IDs by searching for a specific query.
	/// </summary>
	/// <param name="size">Number of results per page.</param>
	/// <param name="pageNumber">Page number to retrieve.</param>
	/// <param name="searchText">Search query text.</param>
	/// <returns>List of asset IDs matching the search query.</returns>
	public async Task<List<string>> GetAssetIdsBySearching(int size, int pageNumber, string searchText)
	{
		List<string> assetIds = new List<string>();

		HttpClientHandler handler = new HttpClientHandler();
		HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/") // Replace with your API's base URL
		};

		string apiUrl = $"api/v1/assets/search?size={size}&page={pageNumber}";

		if (!string.IsNullOrEmpty(searchText))
		{
			apiUrl += $"&searchQuery={searchText}";
		}
		
		assetIds = await Http.GetFromJsonAsync<List<string>>(apiUrl);

		return assetIds;
	}
	
	/// <summary>
	/// Simply converts a assetId into a url for that asset.
	/// </summary>
	/// <param name="assetId"></param>
	/// <returns>The url that points to that asset.</returns>
	public async Task<string> GetAssetContentById(string assetId)
	{
		return $"http://localhost:5115/api/v1/assets/{assetId}";
	}
	
	/// <summary>
	/// Returns a list of assetIds associated with a product.
	/// </summary>
	/// <param name="productId"></param>
	/// <returns></returns>
	public async Task<List<string>> GetAssetsByProduct(string productId)
	{
		List<string> assets = new List<string>();

		HttpClientHandler handler = new HttpClientHandler();
		HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/")
		};

		var response = await Http.GetFromJsonAsync<GetProductAssetsIdsResponse>($"api/v1/products/{productId}/assets");
		foreach (Guid guid in response.ImageIds)
		{
			assets.Add(guid.ToString());
		}
		
		return assets;
	}
	
	/// <summary>
	/// Returns a complete list of all asset ids.
	/// </summary>
	/// <returns></returns>
	public async Task<List<string>> GetAllAssetIds ()
	{
		List<string> assetIds = [];

		HttpClientHandler handler = new HttpClientHandler();
		HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/")
		};

		var guids = await Http.GetFromJsonAsync<List<Guid>>("api/v1/assets");

		foreach (var guid in guids)
		{
			assetIds.Add(guid.ToString());
		}

		return assetIds;
	}
	
	/// <summary>
	/// Returns a list of all tags
	/// </summary>
	/// <returns></returns>
	public async Task<List<Tag>> GetAllTags()
	{
		List<Tag> tags = [];

		HttpClientHandler handler = new HttpClientHandler();
		HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/")
		};

		tags = await Http.GetFromJsonAsync<List<Tag>>("api/v1/tags");

		return tags;
	}
	
	// TODO: Implement endpoint for this action
	public static async Task<List<string>> GetAssetsByTags(List<string> selectedTags)
	{
		List<string> imageSources = new List<string>();

		try
		{
			HttpClientHandler handler = new HttpClientHandler();
			HttpClient Http = new HttpClient(handler)
			{
				BaseAddress = new Uri("http://localhost:5115/") 
			};

			string tagQuery = string.Join(",", selectedTags);
			string apiUrl = $"api/v1/assets/byTags?tags={tagQuery}";

			List<string> imageIds = await Http.GetFromJsonAsync<List<string>>(apiUrl);

			foreach (string id in imageIds)
			{
				imageSources.Add($"http://localhost:5115/api/v1/assets/GetImageByUUID?uuid={id}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading images by tags: {ex.Message}");
		}

		return imageSources;
	}
	
	/// <summary>
	/// Returns a list of tags based on an assetId
	/// </summary>
	/// <param name="assetId"></param>
	/// <returns></returns>
	public async Task<List<Tag>> GetTagsByAsset (string assetId)
	{
		HttpClientHandler handler = new HttpClientHandler();
		HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/")
		};
		try{
		var response = await Http.GetFromJsonAsync<List<Tag>>($"api/v1/assets/{assetId}/tags");
		return response;
		}
		catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			// Handle 404 specifically
			Console.WriteLine("Resource not found (404)");
			// Return empty list or default value
			return [];
		}
	}
	
	public async Task<List<Tag>> GetTagsNotOnAsset (string assetId)
	{
		HttpClientHandler handler = new HttpClientHandler();
		HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/")
		};
		
		var response = await Http.GetFromJsonAsync<List<Tag>>($"api/v1/assets/{assetId}/tags/gallery");
    
		return response;
	}
	
	/// <summary>
	/// Returns a product name based on a product id
	/// </summary>
	/// <param name="productId"></param>
	/// <returns></returns>
	public async Task<string> GetProductName(string productId)
	{
		string apiUrl = $"api/v1/products/{productId}";
    
		HttpClientHandler handler = new HttpClientHandler();
		HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/")
		};
		
		var response = await Http.GetFromJsonAsync<GetProductResponse>(apiUrl);
    
		return response.Name;
		
	}

	public async Task<List<Product>> GetAllProducts ()
	{
		List<Product> products = [];

		HttpClientHandler handler = new HttpClientHandler();
		HttpClient Http = new HttpClient(handler)
		{
			BaseAddress = new Uri("http://localhost:5115/")
		};

		products = await Http.GetFromJsonAsync<List<Product>>("api/v1/products");

		return products;
	}
	
}