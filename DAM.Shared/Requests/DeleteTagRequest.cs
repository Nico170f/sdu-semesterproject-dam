using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class DeleteTagRequest
{
    [Required] public Guid TagUUID { get; set; }
}