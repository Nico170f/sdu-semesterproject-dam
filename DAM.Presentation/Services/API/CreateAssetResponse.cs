using DAM.Presentation.Models;

namespace DAM.Presentation.Services.API;

public class CreateAssetResponse
{
    public string AssetId { get; set; }

    public CreateAssetResponse(Asset asset)
    {
        AssetId = asset.UUID.ToString();
    }
}