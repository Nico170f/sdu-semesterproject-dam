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

	/// <summary>
	/// Converts a assetId into a url for that asset.
	/// </summary>
	/// <param name="assetId"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <returns>The url that points to that asset.</returns>
	public string GetAssetContentById(Guid assetId, int width = default, int height = default)
	{
		if (width == default || height == default)
		{
			return $"http://localhost:5115/api/v1/assets/{assetId}";
		}
		return $"http://localhost:5115/api/v1/assets/{assetId}?width={width}&height={height}";
	}
	
	/// <summary>
	/// Returns a list of assetIds associated with a product.
	/// </summary>
	/// <param name="productId"></param>
	/// <returns></returns>
	public async Task<List<Guid>> GetAssetsByProduct(Guid productId)
	{
		var response = await _httpClient.GetFromJsonAsync<GetProductAssetsIdsResponse>($"api/v1/products/{productId}/assets");
		return response?.AssetIds ?? [];
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
	/// <param name="searchString"></param>
	/// <returns>A list of tags not present on the asset, or empty list if none found.</returns>
	public async Task<(List<Tag> tagList, int totalAmount)> GetTagsNotOnAsset(Guid assetId, string searchString = "", int amount = 20, int page = 1)
	{
		string apiUrl = $"api/v1/assets/{assetId}/tags/gallery?";
		List<string> parameters = [];
		
		if (!string.IsNullOrEmpty(searchString))
		{
			parameters.Add($"searchString={searchString}");
		}
		
		string amountApiUrl = $"api/v1/tags/count?assetId={assetId}&" + string.Join('&', parameters); 
		int totalAmount = int.Parse(await _httpClient.GetStringAsync(amountApiUrl));
		
		parameters.Add($"amount={amount}");
		parameters.Add($"page={page}");

		apiUrl += string.Join('&', parameters);
		
		List<Tag>? response = await _httpClient.GetFromJsonAsync<List<Tag>>(apiUrl);
		return (response ?? [], totalAmount);
	}

	/// <summary>
	/// Returns a list of assets that are not associated with the specified product.
	/// </summary>
	/// <param name="productId">The ID of the product to check for unassigned assets.</param>
	/// <param name="searchString"></param>
	/// <param name="selectedTags"></param>
	/// <param name="amount"></param>
	/// <param name="page"></param>
	/// <returns>A list of assets not present on the product, or an empty list if none found.</returns>
	public async Task<(List<Asset> assetList, int totalAmount)> GetAssetsNotOnProduct(Guid productId, string searchString = "", HashSet<Guid>? selectedTagIds = null, int amount = 20, int page = 1)
	{
		string apiUrl = $"api/v1/products/{productId}/assets/gallery?";
		List<string> parameters = [];
		
		if (!string.IsNullOrEmpty(searchString))
		{
			parameters.Add($"searchString={searchString}");
		}
		
		if (selectedTagIds is not null && selectedTagIds.Count > 0)
		{
			parameters.Add($"selectedTagIds={string.Join(',', selectedTagIds)}");
		}
		string amountApiUrl = $"api/v1/products/{productId}/assets/gallery/count";
		int totalAmount = int.Parse(await _httpClient.GetStringAsync(amountApiUrl));
		
		parameters.Add($"amount={amount}");
		parameters.Add($"page={page}");

		apiUrl += string.Join('&', parameters);
		
		List<Asset>? response = await _httpClient.GetFromJsonAsync<List<Asset>>(apiUrl);
		return (response ?? [], totalAmount);
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
		List<Product> products = await _httpClient.GetFromJsonAsync<List<Product>>("api/v1/products") ?? [];

		foreach (Product product in products)
		{
			var enhancedProduct = new EnhancedProduct
			{
				UUID = product.UUID,
				Name = product.Name,
			};

			var assets = await GetAssetsByProduct(product.UUID);
			Guid assetId = Guid.Empty;
			if (assets.Count > 0)
			{
				assetId = assets[0];
			}
			enhancedProduct.MainAssetUUID = assetId;
			
			enhancedProducts.Add(enhancedProduct);
		}

		return enhancedProducts;
	}

	public async Task<(List<Guid> assetIds, int totalAmount)> GetAssetIds(string searchString = "", HashSet<Guid>? selectedTags = null, int amount = 20, int page = 1)
	{
		string apiUrl = "api/v1/assets?";
		List<string> parameters = [];
		
		if (!string.IsNullOrEmpty(searchString))
		{
			parameters.Add($"searchString={searchString}");
		}
		
		if (selectedTags is not null && selectedTags.Count > 0)
		{
			parameters.Add($"selectedTagIds={string.Join(',', selectedTags)}");
		}
		string amountApiUrl = "api/v1/assets/count?" + string.Join('&', parameters);
		int totalAmount = int.Parse(await _httpClient.GetStringAsync(amountApiUrl));
		
		parameters.Add($"amount={amount}");
		parameters.Add($"page={page}");

		apiUrl += string.Join('&', parameters);

		List<Guid>? assetIds = await _httpClient.GetFromJsonAsync<List<Guid>>(apiUrl);
		return (assetIds ?? [], totalAmount);
	}

	public async Task<(List<EnhancedProduct> productList, int totalAmount)> GetProducts(string searchString = "", int amount = 20, int page = 1)
	{
		string apiUrl = "api/v1/products?";
		List<string> parameters = [];
		
		if (!string.IsNullOrEmpty(searchString))
		{
			parameters.Add($"searchString={searchString}");
		}

		string amountApiUrl = "api/v1/products/count?" + string.Join('&', parameters);
		int total = int.Parse(await _httpClient.GetStringAsync(amountApiUrl));
		
		parameters.Add($"amount={amount}");
		parameters.Add($"page={page}");

		apiUrl += string.Join('&', parameters);

		List<Product>? products = await _httpClient.GetFromJsonAsync<List<Product>>(apiUrl);
		
		List<EnhancedProduct> enhancedProducts = [];

		foreach (var product in products ?? [])
		{
			var enhancedProduct = new EnhancedProduct
			{
				UUID = product.UUID,
				Name = product.Name,
			};

			var assets = await GetAssetsByProduct(product.UUID);
			Guid assetId = Guid.Empty;
			if (assets.Count > 0)
			{
				assetId = assets[0];
			}
			enhancedProduct.MainAssetUUID = assetId;
			
			enhancedProducts.Add(enhancedProduct);
		}
		
		return (enhancedProducts, amount);
	}

	public async Task SyncWithPim ()
	{
		var response = await _httpClient.GetAsync("api/v1/products/syncWithPim");
		Console.WriteLine(response.Content);
	}

	public async Task<(List<Tag> tagList, int totalAmount)> GetTags(string searchString = "", int amount = 50, int page = 1)
	{
		string apiUrl = "api/v1/tags?";
		List<string> parameters = [];
		
		if (!string.IsNullOrEmpty(searchString))
		{
			parameters.Add($"searchString={searchString}");
		}
		
		string amountApiUrl = "api/v1/tags/count?" + string.Join('&', parameters);
		int totalAmount = int.Parse(await _httpClient.GetStringAsync(amountApiUrl));
		
		parameters.Add($"amount={amount}");
		parameters.Add($"page={page}");
		
		apiUrl += string.Join('&', parameters);
		
		var response = await _httpClient.GetFromJsonAsync<List<Tag>>(apiUrl);
		return (response ?? [], totalAmount);
	}
}