using System.ComponentModel.DataAnnotations;

namespace DAM.Presentation.Services.API;

public class CreateImageRequest
{
    [Required] public string Content { get; set; }
}