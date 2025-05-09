using System.ComponentModel.DataAnnotations;
using DAM.Backend.Data.Models;

namespace DAM.Backend.Controllers.API;

public class GetAssetsTagsRequest
{
    [Required] public List<Guid> tagList { get; set; }
}