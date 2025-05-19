using DAM.Shared.Models;

namespace DAM.Shared.Responses;

public class CreateMockProductResponse
{
    public string ProductID { get; set; }

    public CreateMockProductResponse(Product product)
    {
        ProductID = product.UUID.ToString();
    }
}