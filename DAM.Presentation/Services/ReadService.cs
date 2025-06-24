using System.Net.Http.Json;
using DAM.Shared.Models;
using DAM.Shared.Responses;
using Microsoft.AspNetCore.WebUtilities;

namespace DAM.Presentation.Services;

public class ReadService(IHttpClientFactory httpClientFactory) : BaseService(httpClientFactory)
{

	#region Tags
	
	/// <summary>
	/// Retrieves tags from the API with optional filtering parameters.
	/// </summary>
	/// <param name="assetIdParent">Optional parent asset ID to filter tags by parent.</param>
	/// <param name="assetIdToAvoid">Optional asset ID to exclude from the results.</param>
	/// <param name="searchString">Optional search string to filter tags by name or value.</param>
	/// <param name="amount">Optional number of tags to return per page.</param>
	/// <param name="page">Optional page number for pagination.</param>
	/// <returns>A <see cref="GetTagsResponse"/> containing the filtered tags, or null if the request fails.</returns>
	public async Task<GetTagsResponse?> GetTags(
		Guid? assetIdParent = null,
		Guid? assetIdToAvoid = null,
		string? searchString = null, 
		int? amount = null, 
		int? page = null)
	{
		var queryParams = new Dictionary<string, string?>();
		
		if (assetIdParent.HasValue)
			queryParams.Add("assetIdParent", assetIdParent.ToString());
		if (assetIdToAvoid.HasValue)
			queryParams.Add("assetIdToAvoid", assetIdToAvoid.ToString());
		if (!string.IsNullOrEmpty(searchString))
			queryParams.Add("searchString", searchString);
		if (amount.HasValue)
			queryParams.Add("amount", amount.ToString());
		if (page.HasValue)
			queryParams.Add("page", page.ToString());

		string apiUrl = QueryHelpers.AddQueryString("api/v1/tags", queryParams);
		var response = await HttpClient.GetFromJsonAsync<GetTagsResponse>(apiUrl);

		return response;
	}

	#endregion

	#region Assets
	
	/// <summary>
	/// Retrieves assets from the API with optional filtering parameters.
	/// </summary>
	/// <param name="productIdParent">Optional parent product ID to filter assets.</param>
	/// <param name="productIdToAvoid">Optional product ID to exclude from the results.</param>
	/// <param name="searchString">Optional search string to filter assets.</param>
	/// <param name="selectedTags">Optional set of selected tags to filter assets.</param>
	/// <param name="amount">Optional number of assets to return per page.</param>
	/// <param name="page">Optional page number for pagination.</param>
	/// <returns>A <see cref="GetAssetsResponse"/> containing the filtered assets, or null if the request fails.</returns>
	public async Task<GetAssetsResponse?> GetAssets(
		Guid? productIdParent = null,
		Guid? productIdToAvoid = null,
		string? searchString = null, 
		HashSet<Tag>? selectedTags = null,
		int? amount = null, 
		int? page = null)
	{
		var queryParams = new Dictionary<string, string?>();
		
		if (productIdParent.HasValue)
			queryParams.Add("productIdParent", productIdParent.ToString());
		if (productIdToAvoid.HasValue)
			queryParams.Add("productIdToAvoid", productIdToAvoid.ToString());
		if (!string.IsNullOrEmpty(searchString))
			queryParams.Add("searchString", searchString);
		if (selectedTags is not null && selectedTags.Count > 0)
			queryParams.Add("selectedTags", string.Join(',', selectedTags.Select(tag => tag.UUID)));
		if (amount.HasValue)
			queryParams.Add("amount", amount.ToString());
		if (page.HasValue)
			queryParams.Add("page", page.ToString());

		string apiUrl = QueryHelpers.AddQueryString("api/v1/assets", queryParams);
		var response = await HttpClient.GetFromJsonAsync<GetAssetsResponse>(apiUrl);

		return response;
	}
	
	/// <summary>
	/// Gets the asset content URL by asset ID, optionally with dimensions.
	/// </summary>
	/// <param name="assetId">The ID of the asset.</param>
	/// <param name="dimensions">Optional width and height for the asset.</param>
	/// <returns>The URL to access the asset content.</returns>
	public string GetAssetContentByAssetId(Guid assetId, (int width, int height) dimensions = default /* (0, 0) */)
	{
		if (dimensions == default)
			return $"{HttpClient.BaseAddress}api/v1/assets/{assetId}";
		
		return $"{HttpClient.BaseAddress}api/v1/assets/{assetId}?width={dimensions.width}&height={dimensions.height}";
	}
	
	/// <summary>
	/// Gets the asset content URL by product ID and priority, optionally with dimensions.
	/// </summary>
	/// <param name="productId">The ID of the product.</param>
	/// <param name="priority">The priority of the asset.</param>
	/// <param name="dimensions">Optional width and height for the asset.</param>
	/// <returns>The URL to access the asset content.</returns>
	public string GetAssetContentByProductId(Guid productId, int priority = 0, (int width, int height) dimensions = default /* (0, 0) */)
	{
		if(dimensions == default)
			return $"{HttpClient.BaseAddress}api/v1/products/{productId}/assets/{priority}";

		return $"{HttpClient.BaseAddress}api/v1/products/{productId}/assets/{priority}?width={dimensions.width}&height={dimensions.height}";
	}
	
	/// <summary>
	/// Returns a list of asset IDs associated with a product.
	/// </summary>
	/// <param name="productId">The ID of the product.</param>
	/// <returns>A list of asset IDs.</returns>
	public async Task<List<Guid>> GetAssetsByProduct(Guid productId)
	{
		var response = await HttpClient.GetFromJsonAsync<GetProductAssetsIdsResponse>($"api/v1/products/{productId}/assets");
		return response?.AssetIds ?? [];
	}
	
	/// <summary>
	/// Returns a complete list of all asset IDs.
	/// </summary>
	/// <returns>A list of all asset IDs.</returns>
	public async Task<List<Guid>> GetAllAssetIds ()
	{
		List<Guid>? guids = await HttpClient.GetFromJsonAsync<List<Guid>>("api/v1/assets");
		return guids ?? [];
	}

    #endregion

	#region Products

	/// <summary>
	/// Returns a product name based on a product ID.
	/// </summary>
	/// <param name="productGuid">The ID of the product.</param>
	/// <returns>The name of the product. If not exist, "No product found!".</returns>
	public async Task<string> GetProductName(Guid productGuid)
	{
		var response = await HttpClient.GetFromJsonAsync<GetProductResponse>($"api/v1/products/{productGuid}");
		return response?.Product.Name ?? "No product found!";
	}

	/// <summary>
	/// Retrieves products from the API with optional filtering parameters.
	/// </summary>
	/// <param name="searchString">Optional search string to filter products.</param>
	/// <param name="amount">Optional number of products to return per page.</param>
	/// <param name="page">Optional page number for pagination.</param>
	/// <returns>A <see cref="GetProductsResponse"/> containing the filtered products, or null if the request fails.</returns>
	public async Task<GetProductsResponse?> GetProducts(
		string? searchString = null, 
		int? amount = null, 
		int? page = null)
	{
		var queryParams = new Dictionary<string, string?>();
		
		if (!string.IsNullOrEmpty(searchString))
			queryParams.Add("searchString", searchString);
		if (amount.HasValue)
			queryParams.Add("amount", amount.ToString());
		if (page.HasValue)
			queryParams.Add("page", page.ToString());

		string apiUrl = QueryHelpers.AddQueryString("api/v1/products", queryParams);
		var response = await HttpClient.GetFromJsonAsync<GetProductsResponse>(apiUrl);

		return response;
	}
	
	#endregion

	#region Others

	/// <summary>
	/// Synchronizes products with the PIM system via the API.
	/// </summary>
	public async Task SyncWithPim ()
	{
		var response = await HttpClient.GetAsync("api/v1/products/syncWithPim");
		Console.WriteLine(response.Content);
	}

	#endregion
	
}
