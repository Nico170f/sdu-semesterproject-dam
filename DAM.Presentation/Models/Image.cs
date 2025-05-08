using System.ComponentModel.DataAnnotations;

namespace DAM.Presentation.Models;

public class Image
{
    [Key] public Guid UUID { get; set; }
    public string Content { get; set; } 
    public int Width { get; set; }
    public int Height { get; set; }
    [DataType(DataType.DateTime)] public DateTime CreatedAt { get; set; }
    [DataType(DataType.DateTime)] public DateTime UpdatedAt { get; set; }
}