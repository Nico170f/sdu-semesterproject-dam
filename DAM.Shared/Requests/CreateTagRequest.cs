using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class CreateTagRequest
{
    [Required] public string Name { get; set; }
}