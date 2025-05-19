using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class CreateTagRequest
{
	[Required] public required string Name { get; set; }
}