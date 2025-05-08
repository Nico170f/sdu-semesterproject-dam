using DAM.Presentation.Models;

namespace DAM.Presentation.Services.API;

public class CreateMockProductResponse
{
    public string ProductID { get; set; }

    public CreateMockProductResponse(Product product)
    {
        ProductID = product.UUID.ToString();
    }
}