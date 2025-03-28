using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Data.Models;
public class Group
{
    [Key]
    public Guid ImageIdentifier { get; set; }
}