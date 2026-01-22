using RcTracking.Shared.Model;
using RcTracking.UI.Interface;

namespace RcTracking.TestDoubles.Mocks;

public class MockPlaneService : IPlaneService
{
    public Dictionary<Guid, PlaneModel> Planes { get; } = new();
    public bool HasLoaded => true;
    public Task LoadPlanesAsync() => Task.CompletedTask;
    public Task<Guid> AddPlaneAsync(PlaneModel plane) { Planes[plane.Id] = plane; return Task.FromResult(Guid.NewGuid()); }
    public Task UpdatePlaneAsync(PlaneModel plane) { Planes[plane.Id] = plane; return Task.CompletedTask; }
    public Task DeletePlaneAsync(Guid planeId) { Planes.Remove(planeId); return Task.CompletedTask; }
    public int WorkingPlanesCount() { return Planes.Values.Count(p => p.Flying); }
}
