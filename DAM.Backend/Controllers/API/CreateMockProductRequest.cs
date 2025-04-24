using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers.API;

public class CreateMockProductRequest
{
    [Required] public string Name { get; set; }
}