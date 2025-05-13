using System.ComponentModel.DataAnnotations;

namespace DAM.Presentation.Services.API;

public class CreateAssetRequest
{
    [Required] public string Content { get; set; }
}