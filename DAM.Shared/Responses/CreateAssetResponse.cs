using DAM.Shared.Models;

namespace DAM.Shared.Responses;

public class CreateAssetResponse
{
    public string AssetId { get; set; }

    public CreateAssetResponse(Asset asset)
    {
        AssetId = asset.UUID.ToString();
    }
}