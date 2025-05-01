using System.ComponentModel.DataAnnotations;
using DAM.Backend.Data.Models;

namespace DAM.Backend.Controllers.API;

public class GetAssetsByTagsResponse
{
    [Required]
    public List<Image> imageList { get; set; } 
}