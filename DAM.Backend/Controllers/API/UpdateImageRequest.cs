using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers.API;

public class UpdateImageRequest : CreateImageRequest
{
    public string Content { get; set; }
}