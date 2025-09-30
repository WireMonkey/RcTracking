using RcTracking.Shared.Model;

namespace RcTracking.UI.Interface
{
    public interface IFlightService
    {
        Dictionary<Guid, FlightModel> Flights { get; }
        bool HasLoaded { get; }
        Task LoadFlightsAsync();
        Task AddFlightAsync(FlightModel flight);
        Task UpdateFlightAsync(FlightModel flight);
        Task DeleteFlightAsync(Guid flightId);
    }
}
