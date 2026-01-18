using RcTracking.UI.Models;

namespace RcTracking.UI.Interface
{
    public interface ICombineDataService
    {
        Dictionary<Guid, UiPlane> PlaneStats { get; }
        Dictionary<DateOnly, Dictionary<Guid, int>> MonthyStats { get; }
        void CalculatePlaneStats();
        void CalculatePlaneStats(Guid planeId);
        void CalculateMonthyStats(Guid monthId);
    }
}
