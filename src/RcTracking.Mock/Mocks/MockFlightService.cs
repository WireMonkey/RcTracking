using RcTracking.Shared.Model;
using RcTracking.UI.Interface;

namespace RcTracking.TestDoubles.Mocks;

public class MockFlightService : IFlightService
{
    public Dictionary<Guid, FlightModel> Flights { get; } = new();
    public bool HasLoaded => true;
    public Task LoadFlightsAsync() => Task.CompletedTask;
    public Task AddFlightAsync(FlightModel flight) { Flights[flight.Id] = flight; return Task.CompletedTask; }
    public Task UpdateFlightAsync(FlightModel flight) { Flights[flight.Id] = flight; return Task.CompletedTask; }
    public Task DeleteFlightAsync(Guid flightId) { Flights.Remove(flightId); return Task.CompletedTask; }
}
