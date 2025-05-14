
namespace DAM.Presentation.Services.API;


class GetProductAssetAmountResponse {
    public int Amount { get; set; }

    public GetProductAssetAmountResponse(int amount)
    {
        Amount = amount;
    }

}