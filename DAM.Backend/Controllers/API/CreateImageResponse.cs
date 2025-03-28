using DAM.Backend.Data.Models;

namespace DAM.Backend.Controllers;

public class CreateImageResponse
{
    public string ImageId { get; set; }

    public CreateImageResponse(Image image)
    {
        ImageId = image.UUID;
    }
}