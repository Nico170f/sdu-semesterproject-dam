using System.ComponentModel.DataAnnotations;

namespace DAM.Presentation.Services.API;

public class CreateTagRequest
{
    [Required] public string Name { get; set; }
}