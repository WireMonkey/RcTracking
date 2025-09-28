using System.ComponentModel.DataAnnotations;

namespace RcTracking.Shared.Model;

public class PlaneModel(Guid Id, string Name)
{
    [Required]
    public Guid Id { get; init; } = Id;
    [Required]
    public string Name { get; set; } = Name;

    public static PlaneModel CreateDbRec(string name) => new(Guid.NewGuid(), name);
}
