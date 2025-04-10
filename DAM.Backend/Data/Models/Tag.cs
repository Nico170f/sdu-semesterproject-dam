using System.ComponentModel.DataAnnotations;

namespace DAM.Backend.Data.Models;

public class Tag
{
    [Key] public Guid UUID { get; set; }
    public string Name { get; set; }
    public bool IsAddedByUser { get; set; }

    // public Tag(string name, bool isAddedByUser)
    // {
    //     this.Name = name;
    //     this.IsAddedByUser = isAddedByUser;
    // }

}