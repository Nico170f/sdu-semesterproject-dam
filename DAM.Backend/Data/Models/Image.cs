using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace DAM.Backend.Data.Models;

public class Image
{
    [Key]
    public string UUID { get; set; }
    public string Content { get; set; } //changed to string from byte[]
    public int Width { get; set; }
    public int Height { get; set; }
    public Product? Product { get; set; }
    public int Priority { get; set; }
    public bool IsShown { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime UpdatedAt { get; set; }

}