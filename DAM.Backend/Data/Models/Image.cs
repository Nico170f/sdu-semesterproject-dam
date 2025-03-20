using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Data.Models;

public class Image
{
    [Key]
    public Guid UUID { get; set; }
    public byte[] Content { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Product Product { get; set; }
    public int Priority { get; set; }
    public bool IsShown { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime UpdatedAt { get; set; }

}