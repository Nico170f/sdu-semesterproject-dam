using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Data.Models;

public class ProductImage
{
    public Guid ProductUUID { get; set; }
    public Guid ImageUUID { get; set; }
    public int Priority { get; set; }
}