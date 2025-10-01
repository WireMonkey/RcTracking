using RcTracking.UI.Models;

namespace RcTracking.UI.Interface
{
    public interface ICombineDataService
    {
        Dictionary<Guid, UiPlane> PlaneStats { get; }
        void CalculatePlaneStats();
        void CalculatePlaneStats(Guid planeId);
    }
}
