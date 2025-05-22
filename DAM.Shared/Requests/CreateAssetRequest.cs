using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class CreateAssetRequest
{
    [Required] public required string Content { get; set; }
}