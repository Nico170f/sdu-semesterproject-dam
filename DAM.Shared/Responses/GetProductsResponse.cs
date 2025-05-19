using DAM.Shared.Models;

namespace DAM.Shared.Responses;

public class GetProductsResponse
{
	public required List<Product> Products { get; set; }
	public int? TotalCount { get; set; }
}