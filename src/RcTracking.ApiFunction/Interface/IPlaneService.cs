using RcTracking.Shared.Model;

namespace RcTracking.ApiFunction.Interface
{
    public interface IPlaneService
    {
        Task<PlaneModel> CreatePlaneAsync(string name);
        Task DeletePlaneAsync(Guid id);
        Task<IEnumerable<PlaneModel>> GetPlanesAsync();
        Task<PlaneModel?> GetPlaneAsync(Guid id);
        Task<PlaneModel> UpdatePlaneAsync(Guid id, string name);
    }
}
