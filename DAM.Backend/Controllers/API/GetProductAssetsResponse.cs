
namespace DAM.Backend.Controllers.API;

class GetProductAssetsIdsResponse {

    public List<Guid> ImageIds { get; set; }

    public GetProductAssetsIdsResponse(List<Guid> imageIds)
    {
        ImageIds = imageIds;
    }
}