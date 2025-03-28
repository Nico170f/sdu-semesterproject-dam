using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Data.Models;

public class ImageGroup
{
    [Key]
    public Group Group { get; set; }
    [Key]
    public Image Image { get; set; }
}