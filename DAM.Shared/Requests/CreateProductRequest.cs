using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

/// <summary>
/// Used when creating new products
/// </summary>
public class CreateProductRequest
{
	[Required] public required string Name { get; set; }
	[Required] public required Guid ProductId { get; set; }
}
