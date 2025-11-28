using System.ComponentModel.DataAnnotations;

namespace DatingApp.Domain.Entities;

public class Group(string name)
{
    [Key]
    public string Name { get; set; } = name;

    //nav property
    public ICollection<Connection> Connections { get; set; } = [];
}