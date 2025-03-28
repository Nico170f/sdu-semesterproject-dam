using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Data.Models;

public class TagImageGroup
{
    [Key]
    public int TagImageGroupID { get; set; }
    public Tag Tag { get; set; }
    public Image Image { get; set; }

}