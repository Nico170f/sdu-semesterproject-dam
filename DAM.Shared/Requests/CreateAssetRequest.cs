using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class CreateAssetRequest
{
    [Required] public string Content { get; set; }
}