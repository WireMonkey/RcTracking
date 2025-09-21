using RcTracking.ApiFunction.Model;

namespace RcTracking.ApiFunction.Interface
{
    public interface IFlightService
    {
        Task<FlightModel> CreateFlightAsync(FlightModel flight);
        Task DeleteFlightAsync(Guid id);
        Task<IEnumerable<FlightModel>> GetFlightAsync();
        Task<FlightModel?> GetFlightAsync(Guid id);
        Task<FlightModel> UpdateFlightAsync(FlightModel flight);
    }
}
