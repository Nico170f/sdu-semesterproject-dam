using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Data.Models;

public class ImageTags
{
    public Guid ImageUUID { get; set; }
    public Guid TagUUID { get; set; }
}