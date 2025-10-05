using System.ComponentModel.DataAnnotations;

namespace RcTracking.Shared.Model;

public class PlaneModel(Guid Id, string Name, string? Propeller = null, string? Battery = null, string? Notes = null, bool Flying = false)
{
    [Required]
    public Guid Id { get; init; } = Id;
    [Required]
    public string Name { get; set; } = Name;
    public string? Propeller {  get; set; } = Propeller;
    public string? Battery { get; set; } = Battery;
    public bool Flying { get; set; } = Flying;
    public string? Notes { get; set; } = Notes;

    public static PlaneModel CreateDbRec(PlaneModel model) => new(Guid.NewGuid(), model.Name, model.Propeller, model.Battery, model.Notes, model.Flying);
}
