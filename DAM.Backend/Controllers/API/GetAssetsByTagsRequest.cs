using System.ComponentModel.DataAnnotations;
using DAM.Backend.Data.Models;

namespace DAM.Backend.Controllers.API;

public class GetAssetsByTagsRequest
{
    [Required] public List<Tag> tagList { get; set; }
}
