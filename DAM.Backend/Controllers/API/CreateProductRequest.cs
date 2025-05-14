using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers.API;

/// <summary>
/// Used when creating new products
/// </summary>
public class CreateProductRequest
{
	[Required] public string Name { get; set; }
	[Required] public Guid UUID { get; set; }
}
