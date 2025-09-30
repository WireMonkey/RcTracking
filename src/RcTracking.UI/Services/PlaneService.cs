using RcTracking.Shared.Model;
using RcTracking.UI.Events;
using RcTracking.UI.Interface;
using System.Net.Http.Json;

namespace RcTracking.UI.Services
{
    public class PlaneService : IPlaneService
    {
        private readonly string _apiUrl;
        private readonly EventBus _eventBus;

        public PlaneService(IConfiguration configuration, EventBus eventBus)
        {
            _apiUrl = configuration.GetValue<string>("apiUrl") ?? throw new ArgumentNullException(nameof(configuration), "apiUrl is missing");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        private Dictionary<Guid, PlaneModel> _planes { get; set; } = new();
        
        public Dictionary<Guid, PlaneModel> Planes
        {
            get
            {
                return _planes;
            }
        }

        public bool HasLoaded => _planes.Count != 0;

        public async Task LoadPlanesAsync()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{_apiUrl}plane");
            if (response.IsSuccessStatusCode)
            {
                var apiReturn = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<List<PlaneModel>>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (apiReturn is not null)
                {
                    _planes.Clear();
                    _planes.EnsureCapacity(apiReturn.Count);
                    foreach (var plane in apiReturn)
                    {
                        _planes[plane.Id] = plane;
                    }
                    _eventBus.Message = new EventMessage { Event = EventEnum.RefreshPlane };
                }
            }
        }

        public async Task AddPlaneAsync(PlaneModel plane)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync($"{_apiUrl}plane", plane);
            if (response.IsSuccessStatusCode)
            {
                var addedPlane = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<PlaneModel>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (addedPlane is not null)
                {
                    _planes.Add(addedPlane.Id, addedPlane);
                    _eventBus.Message = new PlaneFlightAddedMessage { Event = EventEnum.RefreshPlane, PlaneId = addedPlane.Id };
                }
            }
        }

        public async Task UpdatePlaneAsync(PlaneModel plane)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.PutAsJsonAsync($"{_apiUrl}plane/{plane.Id}", plane);
            if (response.IsSuccessStatusCode)
            {
                var updatedPlane = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<PlaneModel>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (updatedPlane is not null)
                {
                    _planes[updatedPlane.Id] = updatedPlane;
                    _eventBus.Message = new EventMessage { Event = EventEnum.PlaneUpdated };
                }
            }
        }

        public async Task DeletePlaneAsync(Guid planeId)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.DeleteAsync($"{_apiUrl}plane/{planeId}");
            if (response.IsSuccessStatusCode)
            {
                if (_planes.ContainsKey(planeId))
                {
                    _planes.Remove(planeId);
                    _eventBus.Message = new EventMessage { Event = EventEnum.RefreshPlane };
                }
            }
        }
    }
}
