using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers.API;

public class DeleteTagRequest
{
    [Required] public string TagUUID { get; set; }
}