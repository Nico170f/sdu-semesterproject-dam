
namespace DAM.Presentation.Services.API;

class GetProductAssetsIdsResponse {

    public List<Guid> ImageIds { get; set; }

    public GetProductAssetsIdsResponse(List<Guid> imageIds)
    {
        ImageIds = imageIds;
    }
}