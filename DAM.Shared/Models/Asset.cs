using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Models;

public class Asset
{
    [Key] public Guid UUID { get; set; }
    public string Content { get; set; } 
    public int Width { get; set; }
    public int Height { get; set; }
    [DataType(DataType.DateTime)] public DateTime CreatedAt { get; set; }
    [DataType(DataType.DateTime)] public DateTime UpdatedAt { get; set; }
}