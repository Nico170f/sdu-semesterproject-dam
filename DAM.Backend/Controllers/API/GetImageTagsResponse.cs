using DAM.Backend.Data.Models;

namespace DAM.Backend.Controllers.API;

public class GetImageTagsResponse
{
    public List<Tag> imageTags { get; set; } 
}