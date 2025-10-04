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

    public int TotalFlights()
    {
        var total = 0;
        foreach (var f in Flights.Values)
        {
            total += f.FlightCount;
        }
        return total;
    }

    public int TotalFlights(int year)
    {
        var total = 0;
        foreach (var f in Flights.Values)
        {
            if (f.FlightDate.Year == year)
            {
                total += f.FlightCount;
            }
        }
        return total;
    }

    public int DaysFlying()
    {
        var uniqueDays = new HashSet<DateOnly>();
        foreach (var flight in Flights.Values)
        {
            uniqueDays.Add(flight.FlightDate);
        }
        return uniqueDays.Count;
    }

    public int DaysFlying(int year)
    {
        var uniqueDays = new HashSet<DateOnly>();
        foreach (var flight in Flights.Values)
        {
            if (flight.FlightDate.Year == year)
            {
                uniqueDays.Add(flight.FlightDate);
            }
        }
        return uniqueDays.Count;
    }
}
