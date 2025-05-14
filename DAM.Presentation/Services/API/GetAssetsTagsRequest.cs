using System.ComponentModel.DataAnnotations;

namespace DAM.Presentation.Services.API;

public class GetAssetsTagsRequest
{
    [Required] public List<Guid> TagList { get; set; }
}