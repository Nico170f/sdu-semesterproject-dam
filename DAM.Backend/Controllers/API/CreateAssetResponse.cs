using DAM.Backend.Data.Models;

namespace DAM.Backend.Controllers.API;

public class CreateAssetResponse
{
    public string AssetId { get; set; }

    public CreateAssetResponse(Asset asset)
    {
        AssetId = asset.UUID.ToString();
    }
}