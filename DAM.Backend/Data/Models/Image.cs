using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace DAM.Backend.Data.Models;

public class Image
{
    [Key] public Guid UUID { get; set; }
    public string Content { get; set; } 
    public int Width { get; set; }
    public int Height { get; set; }
    [DataType(DataType.DateTime)] public DateTime CreatedAt { get; set; }
    [DataType(DataType.DateTime)] public DateTime UpdatedAt { get; set; }
}