using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers;

public class CreateImageRequest
{
    [Required] public string Content { get; set; }

    // [Required] public string ProductId { get; set; }

    // [Required] public bool? IsShown { get; set; } = true;

    // public int? Priority { get; set; }
    // public int? Width { get; set; }
    // public int? Height { get; set; }
}