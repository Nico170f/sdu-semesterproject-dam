
namespace DAM.Shared.Responses;


public class GetProductAssetAmountResponse {
    public int Amount { get; set; }

    public GetProductAssetAmountResponse(int amount)
    {
        Amount = amount;
    }

}