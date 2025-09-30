using Microsoft.EntityFrameworkCore;
using RcTracking.ApiFunction.Context;
using RcTracking.ApiFunction.Interface;
using RcTracking.Shared.Model;

namespace RcTracking.ApiFunction.Service;

public class FlightService : IFlightService
{
    private readonly FlightContext _flightContext;

    public FlightService(FlightContext flightContext) 
    { 
        _flightContext = flightContext;
    }

    public async Task<FlightModel> CreateFlightAsync(FlightModel flight)
    {
        var dbRec = FlightModel.CreateDbRec(flight.FlightDate, flight.PlaneId, flight.FlightCount, flight.Notes);
        await _flightContext.AddAsync(flight);
        await _flightContext.SaveChangesAsync();
        return dbRec;
    }

    public async Task DeleteFlightAsync(Guid id)
    {
        var dbRec = await _flightContext.FindAsync<FlightModel>(id);
        if(dbRec != null)
        {
            _flightContext.Flights.Remove(dbRec);
            await _flightContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<FlightModel>> GetFlightAsync()
    {
        return await _flightContext.Flights
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<FlightModel?> GetFlightAsync(Guid id)
    {
        return await _flightContext.FindAsync<FlightModel>(id);
    }

    public async Task<FlightModel> UpdateFlightAsync(FlightModel flight)
    {
        var dbRec = await _flightContext.FindAsync<FlightModel>(flight.Id) ?? throw new KeyNotFoundException($"Flight with id {flight.Id} not found");
        dbRec!.UpdateFrom(flight);
        await _flightContext.SaveChangesAsync();
        return dbRec;
    }
}
