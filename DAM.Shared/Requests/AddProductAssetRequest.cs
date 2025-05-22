using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class AddProductAssetRequest
{
    [Required] public required Guid AssetId { get; set; }
    [Required] public required int Priority { get; set; }
}