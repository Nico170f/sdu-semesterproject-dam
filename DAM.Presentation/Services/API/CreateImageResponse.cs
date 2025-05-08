using DAM.Presentation.Models;

namespace DAM.Presentation.Services.API;

public class CreateImageResponse
{
    public string ImageId { get; set; }

    public CreateImageResponse(Image image)
    {
        ImageId = image.UUID.ToString();
    }
}