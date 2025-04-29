using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers.API;

public class UpdateAssetRequest : CreateImageRequest
{
    public string Content { get; set; }
}