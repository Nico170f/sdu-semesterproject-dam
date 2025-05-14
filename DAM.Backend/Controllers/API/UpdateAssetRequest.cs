using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers.API;

public class UpdateAssetRequest : CreateAssetRequest
{
    public string Content { get; set; }
}