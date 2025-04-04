using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Data.Models;

public class ImageGroup
{
    [Key]
    public int ImageGroupId { get; set; }
    public Group Group { get; set; }
    public Image Image { get; set; }
}