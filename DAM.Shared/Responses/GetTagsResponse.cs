using DAM.Shared.Models;

namespace DAM.Shared.Responses;

public class GetTagsResponse
{
	public required List<Tag> Tags { get; set; }
	public int? TotalCount { get; set; }
}