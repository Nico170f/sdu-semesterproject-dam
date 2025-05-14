using System.ComponentModel.DataAnnotations;

namespace DAM.Presentation.Services.API;

/// <summary>
/// Used when creating new products
/// </summary>
public class CreateProductRequest
{
	[Required] public string Name { get; set; }
	[Required] public Guid UUID { get; set; }
}
