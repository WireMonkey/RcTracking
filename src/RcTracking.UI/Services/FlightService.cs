using RcTracking.Shared.Model;
using RcTracking.UI.Events;
using RcTracking.UI.Interface;
using System.Net.Http.Json;

namespace RcTracking.UI.Services
{
    public class FlightService : IFlightService
    {
        private readonly string _apiUrl;
        private readonly EventBus _eventBus;

        public FlightService(IConfiguration configuration, EventBus eventBus)
        {
            _apiUrl = configuration.GetValue<string>("apiUrl") ?? throw new ArgumentNullException(nameof(configuration), "apiUrl is missing");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        private Dictionary<Guid, FlightModel> _flights { get; set; } = new();

        public Dictionary<Guid, FlightModel> Flights
        {
            get
            {
                return _flights;
            }
        }

        public bool HasLoaded => _flights.Count != 0;

        public async Task LoadFlightsAsync()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{_apiUrl}flight");
            if (response.IsSuccessStatusCode)
            {
                var apiReturn = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<FlightModel[]>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (apiReturn is not null)
                {
                    AddSortedFlights(apiReturn);

                    _eventBus.Message = new EventMessage { Event = EventEnum.RefreshFlight };
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
                    var dFlights = new FlightModel[_flights.Count + 1];
                    dFlights[0] = addedFlight;
                    for (int i = 0; i < _flights.Count; i++)
                    {
                        dFlights[i + 1] = _flights.Values.ElementAt(i);
                    }
                    
                    AddSortedFlights(dFlights);
                    _eventBus.Message = new EventMessage { Event = EventEnum.RefreshFlight };
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
                    if (_flights.TryGetValue(updatedFlight.Id, out var existingFlight))
                    {
                        existingFlight.UpdateFrom(updatedFlight);
                        _eventBus.Message = new PlaneFlightAddedMessage { Event = EventEnum.RefreshPlane, PlaneId = updatedFlight.PlaneId };
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
                if (_flights.Remove(flightId))
                {
                    _eventBus.Message = new EventMessage { Event = EventEnum.RefreshFlight };
                }
            }
        }

        public int TotalFlights()
        {
            var total = 0;
            foreach (var flight in _flights.Values)
            {
                total += flight.FlightCount;
            }
            return total;
        }

        public int TotalFlights(int year)
        {
            var total = 0;
            foreach (var flight in _flights.Values)
            {
                if (flight.FlightDate.Year == year)
                {
                    total += flight.FlightCount;
                }
            }
            return total;
        }

        public int DaysFlying()
        {
            var uniqueDays = new HashSet<DateOnly>();
            foreach (var flight in _flights.Values)
            {
                uniqueDays.Add(flight.FlightDate);
            }
            return uniqueDays.Count;
        }

        public int DaysFlying(int year)
        {
            var uniqueDays = new HashSet<DateOnly>();
            foreach (var flight in _flights.Values)
            {
                if (flight.FlightDate.Year == year)
                {
                    uniqueDays.Add(flight.FlightDate);
                }
            }
            return uniqueDays.Count;
        }

        private void AddSortedFlights(FlightModel[] flights)
        {
            Array.Sort(flights, (x, y) => x.FlightDate.CompareTo(y.FlightDate));
            _flights.Clear();
            _flights.EnsureCapacity(flights.Length);
            foreach (var flight in flights)
            {
                _flights[flight.Id] = flight;
            }
        }

    }
}
