using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Data.Models;

public class AssetTags
{
    public Guid AssetUUID { get; set; }
    public Guid TagUUID { get; set; }
}