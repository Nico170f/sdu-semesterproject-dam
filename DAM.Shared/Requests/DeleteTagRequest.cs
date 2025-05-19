using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class DeleteTagRequest
{
    [Required] public string TagUUID { get; set; }
}