using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class AddProductAssetRequest
{
    [Required] public Guid AssetId { get; set; }
    public int Priority { get; set; }
}