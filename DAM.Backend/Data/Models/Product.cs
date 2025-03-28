using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Data.Models;

public class Product
{
    [Key]
    public string UUID { get; set; }
    public string Name { get; set; }

}