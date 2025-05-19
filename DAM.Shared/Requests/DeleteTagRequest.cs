using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class DeleteTagRequest
{
	[Required] public required Guid TagId { get; set; }
}