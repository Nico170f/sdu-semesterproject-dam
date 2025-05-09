using System.ComponentModel.DataAnnotations;

namespace DAM.Presentation.Services.API;

public class CreateMockProductRequest
{
    [Required] public string Name { get; set; }
}