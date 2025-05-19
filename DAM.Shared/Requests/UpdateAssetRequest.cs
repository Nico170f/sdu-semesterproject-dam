using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class UpdateAssetRequest
{
	[Required] public required string Content { get; set; }
}