using RcTracking.Shared.Model;
using System.Net.Http.Json;

namespace RcTracking.UI.Services
{
    public class FlightService
    {
        private readonly string _apiUrl;
        private readonly EventBus _eventBus;

        public FlightService(IConfiguration configuration, EventBus eventBus)
        {
            _apiUrl = configuration.GetValue<string>("apiUrl") ?? throw new ArgumentNullException(nameof(configuration), "apiUrl is missing");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public Dictionary<Guid,FlightModel> Flights { get; } = new();

        public async Task LoadFlightsAsync()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{_apiUrl}flight");
            if (response.IsSuccessStatusCode)
            {
                var apiReturn = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<List<FlightModel>>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (apiReturn is not null)
                {
                    Flights.Clear();
                    apiReturn.ToDictionary(f => f.Id, f => f);
                    _eventBus.Message = new Events.EventMessage { Event = Events.EventEnum.RefreshFlight };
                }
            }
        }

        public async Task AddFlightAsync(FlightModel flight)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync($"{_apiUrl}flight", flight);
            if (response.IsSuccessStatusCode)
            {
                var addedFlight = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<FlightModel>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (addedFlight is not null)
                {
                    Flights.Add(addedFlight.Id, addedFlight);
                    _eventBus.Message = new Events.EventMessage { Event = Events.EventEnum.RefreshFlight };
                }
            }
        }

        public async Task UpdateFlightAsync(FlightModel flight)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.PutAsJsonAsync($"{_apiUrl}flight/{flight.Id}", flight);
            if (response.IsSuccessStatusCode)
            {
                var updatedFlight = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<FlightModel>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (updatedFlight is not null)
                {
                    if (Flights.TryGetValue(updatedFlight.Id, out var existingFlight))
                    {
                        existingFlight.UpdateFrom(updatedFlight);
                        _eventBus.Message = new Events.EventMessage { Event = Events.EventEnum.RefreshFlight };
                    }
                }
            }
        }

        public async Task DeleteFlightAsync(Guid flightId)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.DeleteAsync($"{_apiUrl}flight/{flightId}");
            if (response.IsSuccessStatusCode)
            {
                if (Flights.Remove(flightId))
                {
                    _eventBus.Message = new Events.EventMessage { Event = Events.EventEnum.RefreshFlight };
                }
            }
        }

    }
}
