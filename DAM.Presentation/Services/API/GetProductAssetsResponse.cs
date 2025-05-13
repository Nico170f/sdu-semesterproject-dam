
namespace DAM.Presentation.Services.API;

class GetProductAssetsIdsResponse {

    public List<Guid> AssetIds { get; set; }

    public GetProductAssetsIdsResponse(List<Guid> assetIds)
    {
        AssetIds = assetIds;
    }
}