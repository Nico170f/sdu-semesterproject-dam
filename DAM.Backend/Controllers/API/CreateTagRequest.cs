using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers.API;

public class CreateTagRequest
{
    [Required] public string Name { get; set; }
}