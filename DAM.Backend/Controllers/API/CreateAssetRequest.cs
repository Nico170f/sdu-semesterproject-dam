using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers.API;

public class CreateAssetRequest
{
    [Required] public string Content { get; set; }
}