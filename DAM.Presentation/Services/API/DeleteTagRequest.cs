using System.ComponentModel.DataAnnotations;

namespace DAM.Presentation.Services.API;

public class DeleteTagRequest
{
    [Required] public string TagUUID { get; set; }
}