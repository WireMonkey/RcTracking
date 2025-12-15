using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using RcTracking.Shared.Model;
using RcTracking.UI.Events;
using RcTracking.UI.Helper;
using RcTracking.UI.Interface;
using System.Net.Http.Json;

namespace RcTracking.UI.Services
{
    public class PlaneService : IPlaneService
    {
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly EventBus _eventBus;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly ISnackbar _snackbarService;
        private bool _hasLoaded = false;

        public PlaneService(IConfiguration configuration, EventBus eventBus, IAccessTokenProvider accessTokenProvider, ISnackbar snackbarService)
        {
            _apiUrl = configuration.GetValue<string>("apiUrl") ?? throw new ArgumentNullException(nameof(configuration), "apiUrl is missing");
            _apiKey = configuration.GetValue<string>("apiKey") ?? throw new ArgumentNullException(nameof(configuration), "apiKey is missing");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
            _snackbarService = snackbarService ?? throw new ArgumentNullException(nameof(snackbarService));
        }

        // Backward-compatible overload used by unit tests
        public PlaneService(IConfiguration configuration, EventBus eventBus, ISnackbar snackbarService)
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

        private Dictionary<Guid, PlaneModel> _planes { get; set; } = new();

        public Dictionary<Guid, PlaneModel> Planes
        {
            get
            {
                return _planes;
            }
        }

        public bool HasLoaded { get => _hasLoaded; }

        public async Task LoadPlanesAsync()
        {
            using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
            var response = await httpClient.GetAsync($"{_apiUrl}plane");
            if (response.IsSuccessStatusCode)
            {
                var apiReturn = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<List<PlaneModel>>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (apiReturn is not null && apiReturn.Count > 0)
                {
                    _planes.Clear();
                    _planes.EnsureCapacity(apiReturn.Count);
                    foreach (var plane in apiReturn)
                    {
                        _planes[plane.Id] = plane;
                    }
                }
                
                _eventBus.Message = new EventMessage { Event = EventEnum.RefreshPlane };
                _hasLoaded = true;
                return;
            }

            _snackbarService.Add("Failed to load planes from DB", Severity.Error);
        }

        public async Task AddPlaneAsync(PlaneModel plane)
        {
            using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
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
                    _eventBus.Message = new PlaneFlightAddedMessage { Event = EventEnum.PlaneAdded, PlaneId = addedPlane.Id };
                    return;
                }
            }

            _snackbarService.Add("Failed to add plane to DB", Severity.Error);
        }

        public async Task UpdatePlaneAsync(PlaneModel plane)
        {
            using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
            var response = await httpClient.PutAsJsonAsync($"{_apiUrl}plane", plane);
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
                    return;
                }
            }

            _snackbarService.Add("Failed to update plane in DB", Severity.Error);
        }

        public async Task DeletePlaneAsync(Guid planeId)
        {
            using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
            var response = await httpClient.DeleteAsync($"{_apiUrl}plane/{planeId}");
            if (response.IsSuccessStatusCode)
            {
                if (_planes.ContainsKey(planeId))
                {
                    _planes.Remove(planeId);
                    _eventBus.Message = new EventMessage { Event = EventEnum.RefreshPlane };
                    return;
                }
            }

            _snackbarService.Add("Failed to delete plane from DB", Severity.Error);
        }
    }
}
