using DAM.Backend.Data.Models;

namespace DAM.Backend.Controllers;

public class CreateImageResponse
{
    public Guid ImageId { get; set; }

    public CreateImageResponse(Image image)
    {
        ImageId = image.UUID;
    }
}