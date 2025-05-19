using System.ComponentModel.DataAnnotations;

namespace DAM.Shared.Models;

public class Product
{
    [Key] public Guid UUID { get; set; }
    public string Name { get; set; }

}