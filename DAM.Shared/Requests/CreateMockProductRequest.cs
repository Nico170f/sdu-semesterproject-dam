using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class CreateMockProductRequest
{
    [Required] public string Name { get; set; }
}