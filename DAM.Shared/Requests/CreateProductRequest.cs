using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Requests;

/// <summary>
/// Used when creating new products
/// </summary>
public class CreateProductRequest
{
	[Required] public string Name { get; set; }
	[Required] public Guid ProductUUID { get; set; }
}
