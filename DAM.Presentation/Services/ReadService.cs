using System.Net.Http.Json;
using DAM.Presentation.EnhancedModels;
using DAM.Presentation.Models;
using DAM.Presentation.Services.API;

namespace DAM.Presentation.Services;

public class ReadService : BaseService
{

	public ReadService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
	{
	}
	
	
	//Not used
	/// <summary>
	/// Returns a list of asset IDs by searching for a specific query.
	/// </summary>
	/// <param name="size">Number of results per page.</param>
	/// <param name="pageNumber">Page number to retrieve.</param>
	/// <param name="searchText">Search query text.</param>
	/// <returns>List of asset IDs matching the search query.</returns>
	public async Task<List<string>> GetAssetIdsBySearching(int size, int pageNumber, string searchText)
	{
		List<string> assetIds = [];

		string apiUrl = $"api/v1/assets/search?size={size}&page={pageNumber}";

		if (!string.IsNullOrEmpty(searchText))
		{
			apiUrl += $"&searchQuery={searchText}";
		}
		
		assetIds = await _httpClient.GetFromJsonAsync<List<string>>(apiUrl);

		return assetIds;
	}
	
	/// <summary>
	/// Converts a assetId into a url for that asset.
	/// </summary>
	/// <param name="assetId"></param>
	/// <returns>The url that points to that asset.</returns>
	public string GetAssetContentById(Guid assetId)
	{
		return $"http://localhost:5115/api/v1/assets/{assetId}";
	}
	
	/// <summary>
	/// Returns a list of assetIds associated with a product.
	/// </summary>
	/// <param name="productId"></param>
	/// <returns></returns>
	public async Task<List<Guid>> GetAssetsByProduct(Guid productId)
	{
		var response = await _httpClient.GetFromJsonAsync<GetProductAssetsIdsResponse>($"api/v1/products/{productId}/assets");
		return response?.ImageIds ?? [];
	}
	
	/// <summary>
	/// Returns a complete list of all asset ids.
	/// </summary>
	/// <returns></returns>
	public async Task<List<Guid>> GetAllAssetIds ()
	{
		List<Guid>? guids = await _httpClient.GetFromJsonAsync<List<Guid>>("api/v1/assets");
		return guids ?? [];
	}
	
	/// <summary>
	/// Returns a list of all tags
	/// </summary>
	/// <returns></returns>
	public async Task<List<Tag>> GetAllTags()
	{
		List<Tag>? tags = await _httpClient.GetFromJsonAsync<List<Tag>>("api/v1/tags");
		return tags ?? [];
	}
	
	/// <summary>
	/// Returns a list of assetIds based on a list of tagIds
	/// </summary>
	/// <param name="selectedTagsIds"></param>
	/// <returns></returns>
	public async Task<List<Guid>> GetAssetsByTags(List<Guid> selectedTagsIds)
	{
		string tagQuery = string.Join(",", selectedTagsIds);
		string apiUrl = $"api/v1/tags/search?tagList={tagQuery}";

		List<Asset>? response = await _httpClient.GetFromJsonAsync<List<Asset>>(apiUrl);

		List<Guid> assetIds = [];
		assetIds.AddRange((response ?? []).Select(asset => asset.UUID));

		return assetIds;
	}
	
	/// <summary>
	/// Returns a list of tags based on an assetId
	/// </summary>
	/// <param name="assetId"></param>
	/// <returns></returns>
	public async Task<List<Tag>> GetTagsByAsset (Guid assetId)
	{
		List<Tag>? response = await _httpClient.GetFromJsonAsync<List<Tag>>($"api/v1/assets/{assetId}/tags");
		return response ?? [];
	}
	
	/// <summary>
	/// Returns a list of tags that are not associated with the specified asset.
	/// </summary>
	/// <param name="assetId">The ID of the asset to check for unassigned tags.</param>
	/// <returns>A list of tags not present on the asset, or empty list if none found.</returns>
	public async Task<List<Tag>> GetTagsNotOnAsset (Guid assetId)
	{
		List<Tag>? response = await _httpClient.GetFromJsonAsync<List<Tag>>($"api/v1/assets/{assetId}/tags/gallery");
		return response ?? [];
	}
	
	/// <summary>
	/// Returns a product name based on a product id
	/// </summary>
	/// <param name="productGuid">The ID of the product</param>
	/// <returns>The name of the product. If not exist, "No product found!"</returns>
	public async Task<string> GetProductName(Guid productGuid)
	{
		var response = await _httpClient.GetFromJsonAsync<GetProductResponse>($"api/v1/products/{productGuid}");
		return response?.Name ?? "No product found!";
	}

	public async Task<List<EnhancedProduct>> GetAllProducts()
	{
		List<EnhancedProduct> enhancedProducts = [];
		List<Product>? products = await _httpClient.GetFromJsonAsync<List<Product>>("api/v1/products") ?? [];

		foreach (Product product in products)
		{
			var enhancedProduct = new EnhancedProduct
			{
				UUID = product.UUID,
				Name = product.Name,
			};

			Guid assetId = (await GetAssetsByProduct(product.UUID))[0];
			
			enhancedProduct.MainAssetUUID = assetId;
			
			enhancedProducts.Add(enhancedProduct);
		}

		return enhancedProducts;
	}

	public async Task<List<Guid>> GetAssetIds(string searchText = "", HashSet<Guid>? selectedTagIds = null, int amount = 20, int page = 1)
	{
		string apiUrl = "api/v1/Assets?";
		List<string> parameters = [];
		
		if (!string.IsNullOrEmpty(searchText))
		{
			parameters.Add($"searchString={searchText}");
		}
		
		if (selectedTagIds is not null && selectedTagIds.Count > 0)
		{
			parameters.Add($"selectedTagIds={string.Join(',', selectedTagIds)}");
		}
		
		parameters.Add($"amount={amount}");
		parameters.Add($"page={page}");

		apiUrl += string.Join('&', parameters);

		List<Guid>? assetIds = await _httpClient.GetFromJsonAsync<List<Guid>>(apiUrl);
		return assetIds ?? [];
	}
}