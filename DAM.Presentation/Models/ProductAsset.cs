namespace DAM.Presentation.Models;

public class ProductAsset
{
    public Guid ProductUUID { get; set; }
    public Guid AssetUUID { get; set; }
    public int Priority { get; set; }
}