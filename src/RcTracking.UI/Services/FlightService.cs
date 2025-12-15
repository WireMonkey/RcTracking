using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using RcTracking.Shared.Model;
using RcTracking.UI.Events;
using RcTracking.UI.Helper;
using RcTracking.UI.Interface;
using System.Net.Http.Json;

namespace RcTracking.UI.Services
{
    public class FlightService : IFlightService
    {
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly EventBus _eventBus;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly ISnackbar _snackbarService;
        private bool _hasLoaded = false;

        public FlightService(IConfiguration configuration, EventBus eventBus, IAccessTokenProvider accessTokenProvider, ISnackbar snackbarService)
        {
            _apiUrl = configuration.GetValue<string>("apiUrl") ?? throw new ArgumentNullException(nameof(configuration), "apiUrl is missing");
            _apiKey = configuration.GetValue<string>("apiKey") ?? throw new ArgumentNullException(nameof(configuration), "apiKey is missing");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
            _snackbarService = snackbarService ?? throw new ArgumentNullException(nameof(snackbarService));
        }

        // Backward-compatible overload used by unit tests which don't need an access token provider
        public FlightService(IConfiguration configuration, EventBus eventBus, ISnackbar snackbarService)
            : this(configuration, eventBus, new DefaultAccessTokenProvider(), snackbarService)
        {
        }

        private class DefaultAccessTokenProvider : IAccessTokenProvider
        {
            public ValueTask<AccessTokenResult> RequestAccessToken()
            {
                return new ValueTask<AccessTokenResult>(default(AccessTokenResult));
            }

            public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
            {
                return new ValueTask<AccessTokenResult>(default(AccessTokenResult));
            }
        }

        private Dictionary<Guid, FlightModel> _flights { get; set; } = new();

        public Dictionary<Guid, FlightModel> Flights
        {
            get
            {
                return _flights;
            }
        }

        public bool HasLoaded { get => _hasLoaded; }

        public async Task LoadFlightsAsync()
        {
            _snackbarService.Add("Loading flights");
            using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
            var response = await httpClient.GetAsync($"{_apiUrl}flight");
            if (response.IsSuccessStatusCode)
            {
                var apiReturn = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<FlightModel[]>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (apiReturn is not null && apiReturn.Length > 0)
                {
                    AddSortedFlights(apiReturn);
                }

                _eventBus.Message = new EventMessage { Event = EventEnum.RefreshFlight };
                _hasLoaded = true;
                return;
            }

            _snackbarService.Add("Failed to load flights from API", Severity.Error);
        }

        public async Task AddFlightAsync(FlightModel flight)
        {
            using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
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
                    return;
                }
            }

            _snackbarService.Add("Failed to add flight to DB", Severity.Error);
        }

        public async Task UpdateFlightAsync(FlightModel flight)
        {
            using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
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
                        return;
                    }
                }
            }

            _snackbarService.Add("Failed to update flight in DB", Severity.Error);
        }

        public async Task DeleteFlightAsync(Guid flightId)
        {
            using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
            var response = await httpClient.DeleteAsync($"{_apiUrl}flight/{flightId}");
            if (response.IsSuccessStatusCode)
            {
                if (_flights.Remove(flightId))
                {
                    _eventBus.Message = new EventMessage { Event = EventEnum.RefreshFlight };
                    return;
                }
            }

            _snackbarService.Add("Failed to delete flight from DB", Severity.Error);
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
