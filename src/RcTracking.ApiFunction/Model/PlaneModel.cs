namespace RcTracking.ApiFunction.Model;

public class PlaneModel(Guid Id, string Name)
{
    public Guid Id { get; init; } = Id;
    public string Name { get; set; } = Name;
}
