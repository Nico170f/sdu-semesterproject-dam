using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

public class CreateMockProductRequest
{
	[Required] public required string Name { get; set; }
}