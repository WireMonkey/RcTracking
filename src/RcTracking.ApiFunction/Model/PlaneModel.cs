using System.ComponentModel.DataAnnotations;

namespace RcTracking.ApiFunction.Model;

public class PlaneModel(Guid Id, string Name)
{
    [Required]
    public Guid Id { get; init; } = Id;
    [Required]
    public string Name { get; set; } = Name;
}
