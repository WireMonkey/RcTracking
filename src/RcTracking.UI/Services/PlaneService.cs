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
        private readonly ILogger<PlaneService> _logger;

        public PlaneService(IConfiguration configuration, EventBus eventBus, IAccessTokenProvider accessTokenProvider, ISnackbar snackbarService, ILogger<PlaneService> logger)
        {
            _apiUrl = configuration.GetValue<string>("apiUrl") ?? throw new ArgumentNullException(nameof(configuration), "apiUrl is missing");
            _apiKey = configuration.GetValue<string>("apiKey") ?? throw new ArgumentNullException(nameof(configuration), "apiKey is missing");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
            _snackbarService = snackbarService ?? throw new ArgumentNullException(nameof(snackbarService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Backward-compatible overload used by unit tests
        public PlaneService(IConfiguration configuration, EventBus eventBus, ISnackbar snackbarService)
            : this(configuration, eventBus, new DefaultAccessTokenProvider(), snackbarService, new Logger<PlaneService>(new LoggerFactory()))
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
            try
            {
                using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
                var response = await httpClient.GetAsync($"{_apiUrl}plane");
                response.EnsureSuccessStatusCode();
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while loading planes from API");
            }

            _snackbarService.Add("Failed to load planes from DB", Severity.Error);
        }

        public async Task<Guid> AddPlaneAsync(PlaneModel plane)
        {
            try
            {
                using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
                var response = await httpClient.PostAsJsonAsync($"{_apiUrl}plane", plane);
                response.EnsureSuccessStatusCode();
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
                    return addedPlane.Id;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while adding plane to API");
            }

            _snackbarService.Add("Failed to add plane to DB", Severity.Error);
            return Guid.Empty;
        }

        public async Task UpdatePlaneAsync(PlaneModel plane)
        {
            try
            {
                using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
                var response = await httpClient.PutAsJsonAsync($"{_apiUrl}plane", plane);
                response.EnsureSuccessStatusCode();
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating plane in API");
            }

            _snackbarService.Add("Failed to update plane in DB", Severity.Error);
        }

        public async Task DeletePlaneAsync(Guid planeId)
        {
            try
            {
                using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
                var response = await httpClient.DeleteAsync($"{_apiUrl}plane/{planeId}");
                response.EnsureSuccessStatusCode();
                if (_planes.ContainsKey(planeId))
                {
                    _planes.Remove(planeId);
                    _eventBus.Message = new EventMessage { Event = EventEnum.RefreshPlane };
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting plane from API");
            }

            _snackbarService.Add("Failed to delete plane from DB", Severity.Error);
        }

        public int WorkingPlanesCount()
        {
            return _planes.Values.Count(p => p.Flying);
        }
    }
}
