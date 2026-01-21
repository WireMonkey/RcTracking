using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using RcTracking.Shared.Model;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Numerics;

namespace RcTracking.Test;

public class ApiIntergrationTests
{
    private DistributedApplication _app;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.RcTracking_AppHost>();

        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        // To capture logs from your tests, see the "Capture logs from tests" section
        // in the documentation or refer to LoggingTest.cs for a complete example

        _app = await builder.BuildAsync();

        await _app.StartAsync();
    }


    [OneTimeTearDown]
    public async Task TearDown()
    {
        await PlaneCleanup();
        await FlightCleanup();
        await ImageCleanup();

        if (_app != null)
        {
            await _app.StopAsync();
            //await _app.DisposeAsync();
        }
    }

    [Test]
    [Category("Health"), Order(1)]
    public async Task StartsHealthy()
    {
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("rc-tracking-function");
        
        var client = _app.CreateHttpClient("rc-tracking-function");
        var response = await client.GetAsync("/api/Health");

        Assert.That(response.IsSuccessStatusCode, Is.True, "Request to /Healthy should succeed");
    }

    [Test]
    [Category("PlaneCrud"), Order(110)]
    public async Task PlaneCreate()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var plane = new PlaneModel(Guid.Empty, "IntTest - Plane");
        var response = await client.PostAsJsonAsync("/api/Plane", plane);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var returnModel = await response.Content.ReadFromJsonAsync<PlaneModel>();
        Assert.That(returnModel, Is.Not.Null);
        Assert.That(returnModel.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    [Category("PlaneCrud"), Order(120)]
    public async Task PlaneGet()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var response = await client.GetAsync("/api/Plane");
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var planes = await response.Content.ReadFromJsonAsync<List<PlaneModel>>();
        Assert.That(planes, Is.Not.Null);
    }

    [Test]
    [Category("PlaneCrud"), Order(121)]
    public async Task PlaneGetSingle()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Plane");
        var planes = await allResponse.Content.ReadFromJsonAsync<List<PlaneModel>>();
        var id = planes.First(x => x.Name.Contains("IntTest"))?.Id ?? Guid.Empty;
        Assert.That(id, Is.Not.EqualTo(Guid.Empty));

        var response = await client.GetAsync($"/api/Plane?id={id}");
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var plane = await response.Content.ReadFromJsonAsync<PlaneModel>();
        Assert.That(plane, Is.Not.Null);
        Assert.That(plane.Id, Is.EqualTo(id));
    }

    [Test]
    [Category("PlaneCrud"), Order(130)]
    public async Task PlaneUpdate()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Plane");
        var planes = await allResponse.Content.ReadFromJsonAsync<List<PlaneModel>>();
        var id = planes.First(x => x.Name.Contains("IntTest"))?.Id ?? Guid.Empty;
        Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        var updatedPlane = new PlaneModel(id, "IntTest - Update");

        var response = await client.PutAsJsonAsync($"/api/Plane", updatedPlane);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var returnModel = await response.Content.ReadFromJsonAsync<PlaneModel>();
        Assert.That(returnModel, Is.Not.Null);
        Assert.That(returnModel.Name, Is.EqualTo("IntTest - Update"));
    }

    [Test]
    [Category("PlaneCrud"), Order(140)]
    public async Task PlaneDelete()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Plane");
        var planes = await allResponse.Content.ReadFromJsonAsync<List<PlaneModel>>();
        var id = planes.First(x => x.Name.Contains("IntTest"))?.Id ?? Guid.Empty;
        Assert.That(id, Is.Not.EqualTo(Guid.Empty));

        var response = await client.DeleteAsync($"/api/Plane?id={id}");

        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    [Category("FlightCrud"), Order(210)]
    public async Task FlightCreate() 
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var nPlane = new PlaneModel(Guid.Empty, "IntTest - Flight");
        var planeResponse = await client.PostAsJsonAsync("/api/Plane", nPlane);
        var plane = await planeResponse.Content.ReadFromJsonAsync<PlaneModel>();

        var flight = new FlightModel(Guid.Empty, DateOnly.FromDateTime(DateTime.UtcNow), plane!.Id, 1, "IntTest Test flight creation");
        var response = await client.PostAsJsonAsync($"/api/Flight", flight);

        Assert.That(response.IsSuccessStatusCode, Is.True);
        var returnModel = await response.Content.ReadFromJsonAsync<FlightModel>();

        Assert.That(returnModel, Is.Not.Null);
        Assert.That(returnModel.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    [Category("FlightCrud"), Order(220)]
    public async Task FlightGet()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var response = await client.GetAsync("/api/Flight");
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var flights = await response.Content.ReadFromJsonAsync<List<FlightModel>>();
        Assert.That(flights, Is.Not.Null);
    }

    [Test]
    [Category("FlightCrud"), Order(221)]
    public async Task FlightGetSingle()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Flight");
        var flights = await allResponse.Content.ReadFromJsonAsync<List<FlightModel>>();
        var id = flights.Where(x => x.Notes.Contains("IntTest")).First()?.Id ?? Guid.Empty;
        Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        
        var response = await client.GetAsync($"/api/Flight?id={id}");
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var flight = await response.Content.ReadFromJsonAsync<FlightModel>();
        Assert.That(flight, Is.Not.Null);
        Assert.That(flight.Id, Is.EqualTo(id));
    }

    [Test]
    [Category("FlightCrud"), Order(230)]
    public async Task FlightUpdate()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Flight");
        var flights = await allResponse.Content.ReadFromJsonAsync<List<FlightModel>>();
        var flight = flights.Where(x => x.Notes.Contains("IntTest")).First();
        Assert.That(flight, Is.Not.Null);

        flight.Notes = "IntTest - Updated notes";

        var response = await client.PutAsJsonAsync($"/api/Flight", flight);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        
        var returnModel = await response.Content.ReadFromJsonAsync<FlightModel>();
        Assert.That(returnModel, Is.Not.Null);
        Assert.That(returnModel.Notes, Is.EqualTo("IntTest - Updated notes"));
    }

    [Test]
    [Category("FlightCrud"), Order(240)]
    public async Task FlightDelete()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Flight");
        var flights = await allResponse.Content.ReadFromJsonAsync<List<FlightModel>>();
        var id = flights.Where(x => x.Notes.Contains("IntTest")).First()?.Id ?? Guid.Empty;
        Assert.That(id, Is.Not.EqualTo(Guid.Empty));

        var response = await client.DeleteAsync($"/api/Flight?id={id}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    [Category("ImageCrud"), Order(310)]
    public async Task ImageCreate()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var planeM = new PlaneModel(Guid.Empty, "IntTest - Image");
        var planeResponse = await client.PostAsJsonAsync("/api/Plane", planeM);
        var plane = await planeResponse.Content.ReadFromJsonAsync<PlaneModel>();

        // prepare multipart form data with planeId and file
        var testFilesDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestFiles"));
        Directory.CreateDirectory(testFilesDir);
        var imagePath = Path.Combine(testFilesDir, "test.png");
        // If test image doesn't exist, write a tiny 1x1 PNG so tests don't rely on external files
        if (!File.Exists(imagePath))
        {
            var pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGMAAQAABQABDQottAAAAABJRU5ErkJggg==";
            var bytes = Convert.FromBase64String(pngBase64);
            File.WriteAllBytes(imagePath, bytes);
        }

        using var fs = File.OpenRead(imagePath);
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(plane!.Id.ToString()), "planeId");
        content.Add(new StringContent("true"), "isTest");
        var streamContent = new StreamContent(fs);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(streamContent, "file", Path.GetFileName(imagePath));

        var response = await client.PostAsync("/api/Image", content);

        Assert.That(response.IsSuccessStatusCode, Is.True);

        var returnModel = await response.Content.ReadFromJsonAsync<ImageModel>();
        Assert.That(returnModel, Is.Not.Null);
        Assert.That(returnModel.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(returnModel.PlaneId, Is.EqualTo(plane.Id));
    }

    [Test]
    [Category("ImageCrud"), Order(320)]
    public async Task ImageGet()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var response = await client.GetAsync("/api/Image");
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var images = await response.Content.ReadFromJsonAsync<List<ImageModel>>();
        Assert.That(images, Is.Not.Null);
    }

    [Test]
    [Category("ImageCrud"), Order(321)]
    public async Task ImageGetSingle()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Image");
        var images = await allResponse.Content.ReadFromJsonAsync<List<ImageModel>>();
        var id = images?.First(x => x.IsTest ).Id;

        var response = await client.GetAsync($"/api/Image?id={id}");
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var image = await response.Content.ReadFromJsonAsync<ImageModel>();
        Assert.That(image, Is.Not.Null);
        Assert.That(image.Id, Is.EqualTo(id));
    }

    [Test]
    [Category("ImageCrud"), Order(330)]
    public async Task ImageUpdate()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Image");
        var images = await allResponse.Content.ReadFromJsonAsync<List<ImageModel>>();
        var img = images!.First(x => x.IsTest);

        // for update the function expects the form field named "planeId" to contain the id to update
        var testFilesDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestFiles"));
        Directory.CreateDirectory(testFilesDir);
        var imagePath = Path.Combine(testFilesDir, "test.png");
        if (!File.Exists(imagePath))
        {
            var pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGMAAQAABQABDQottAAAAABJRU5ErkJggg==";
            var bytes = Convert.FromBase64String(pngBase64);
            File.WriteAllBytes(imagePath, bytes);
        }

        using var fs = File.OpenRead(imagePath);
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(img.Id.ToString()), "planeId");
        var streamContent = new StreamContent(fs);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(streamContent, "file", Path.GetFileName(imagePath));

        var response = await client.PutAsync($"/api/Image", content);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var returnModel = await response.Content.ReadFromJsonAsync<ImageModel>();
        Assert.That(returnModel, Is.Not.Null);
        Assert.That(returnModel.Id, Is.EqualTo(img.Id));
    }

    [Test]
    [Category("ImageCrud"), Order(340)]
    public async Task ImageDelete()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Image");
        var images = await allResponse.Content.ReadFromJsonAsync<List<ImageModel>>();
        var id = images?.First(x => x.IsTest).Id;

        var response = await client.DeleteAsync($"/api/Image?id={id}");

        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    private async Task PlaneCleanup()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Plane");
        var planes = await allResponse.Content.ReadFromJsonAsync<List<PlaneModel>>();

        var testPlanes = planes.Where(x => x.Name.Contains("IntTest") || x.Name.Contains("InitTest")).ToAsyncEnumerable();
        await foreach (var plane in testPlanes)
        {
            await client.DeleteAsync($"/api/Plane?id={plane.Id}");
        }
    }

    private async Task FlightCleanup()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Flight");
        var flights = await allResponse.Content.ReadFromJsonAsync<List<FlightModel>>();
        var testFlights = flights.Where(x => x.Notes.Contains("IntTest")|| x.Notes.Contains("InitTest")).ToAsyncEnumerable();
        await foreach (var flight in testFlights)
        {
            await client.DeleteAsync($"/api/Flight?id={flight.Id}");
        }
    }

    private async Task ImageCleanup()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Image");
        var images = await allResponse.Content.ReadFromJsonAsync<List<ImageModel>>();
        var testImages = images.Where(x => x.IsTest).ToAsyncEnumerable();
        await foreach (var image in testImages)
        {
            await client.DeleteAsync($"/api/Image?id={image.Id}");
        }
    }
}
