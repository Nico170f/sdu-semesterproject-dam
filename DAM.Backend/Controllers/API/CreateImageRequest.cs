using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers.API;

public class CreateImageRequest
{
    [Required] public string Content { get; set; }
}