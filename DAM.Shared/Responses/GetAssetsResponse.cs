using DAM.Shared.Models;

namespace DAM.Shared.Responses;

public class GetAssetsResponse
{
	public required List<Asset> Assets { get; set; }
	public int? TotalCount { get; set; }
}