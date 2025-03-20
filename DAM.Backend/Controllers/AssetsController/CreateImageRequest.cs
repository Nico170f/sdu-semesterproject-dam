using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Controllers;

public class CreateImageRequest
{
    [Required]
    public byte[] Content { get; set; }
    
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public bool IsShown { get; set; }
    
    public int? Priority { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}