using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class GetAssetsTagsRequest
{
    [Required] public List<Guid> TagList { get; set; }
}