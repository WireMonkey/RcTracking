using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using RcTracking.Shared.Model;
using RcTracking.UI.Events;
using RcTracking.UI.Helper;
using RcTracking.UI.Interface;
using System.Numerics;

namespace RcTracking.UI.Services
{
    public class ImageService : IImageService
    {
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly EventBus _eventBus;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly ISnackbar _snackbarService;
        private bool _hasLoaded = false;
        private long _maxAllowedSize = 15 * 1024 * 1024; // 15 MB

        public ImageService(IConfiguration configuration, EventBus eventBus, IAccessTokenProvider accessTokenProvider, ISnackbar snackbarService)
        {
            _apiUrl = configuration.GetValue<string>("apiUrl") ?? throw new ArgumentNullException(nameof(configuration), "apiUrl is missing");
            _apiKey = configuration.GetValue<string>("apiKey") ?? throw new ArgumentNullException(nameof(configuration), "apiKey is missing");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
            _snackbarService = snackbarService ?? throw new ArgumentNullException(nameof(snackbarService));
        }

        // Backward-compatible overload used by unit tests
        public ImageService(IConfiguration configuration, EventBus eventBus, ISnackbar snackbarService)
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

        private Dictionary<Guid, ImageModel> _images { get; set; } = new();

        public Dictionary<Guid, ImageModel> Images
        {
            get
            {
                return _images;
            }
        }

        public bool HasLoaded { get => _hasLoaded; }

        public async Task AddImage(Guid planeId, IBrowserFile image)
        {
            if (_images.TryGetValue(planeId, out var imageModel))
            {
                await UpdateImage(imageModel.Id, image);
                return;
            }

            try
            {
                using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
                var form = new MultipartFormDataContent();
                form.Add(new StreamContent(image.OpenReadStream(_maxAllowedSize)), "file", image.Name);
                form.Add(new StringContent(planeId.ToString()), "id");
                var response = await httpClient.PostAsync($"{_apiUrl}image", form);
                if (response.IsSuccessStatusCode)
                {
                    var addedImage = await response.Content.ReadAsStringAsync()
                        .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<ImageModel>(t.Result,
                            new System.Text.Json.JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }));
                    if (addedImage is not null)
                    {
                        _images.Add(addedImage.PlaneId, addedImage);
                        return;
                    }
                }

                _snackbarService.Add("Failed to add image to DB", Severity.Error);
            }
            catch (IOException ex) 
            { 
                Console.WriteLine(ex.ToString());
                _snackbarService.Add($"Image was to big.", Severity.Error);
            }
        }

        public Task DeleteImage(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task LoadImages()
        {
            using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
            var response = await httpClient.GetAsync($"{_apiUrl}image");
            if (response.IsSuccessStatusCode)
            {
                var apiReturn = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<List<ImageModel>>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (apiReturn is not null && apiReturn.Count > 0)
                {
                    _images.Clear();
                    _images.EnsureCapacity(apiReturn.Count);
                    foreach (var image in apiReturn)
                    {
                        _images[image.PlaneId] = image;
                    }
                }

                _hasLoaded = true;
                return;
            }
        }

        public async Task UpdateImage(Guid id, IBrowserFile image)
        {
            using var httpClient = await HttpClientHelper.CreateHttpClient(_apiUrl, _apiKey, _accessTokenProvider);
            var form = new MultipartFormDataContent();
            form.Add(new StreamContent(image.OpenReadStream(_maxAllowedSize)), "file", image.Name);
            form.Add(new StringContent(id.ToString()), "id");
            var response = await httpClient.PutAsync($"{_apiUrl}image", form);
            if (response.IsSuccessStatusCode)
            {
                var updatedImage = await response.Content.ReadAsStringAsync()
                    .ContinueWith(t => System.Text.Json.JsonSerializer.Deserialize<ImageModel>(t.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }));
                if (updatedImage is not null)
                {
                    _images[updatedImage.PlaneId] = updatedImage;
                    return;
                }
            }
        }
    }
}
