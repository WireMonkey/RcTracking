using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using RcTracking.ApiFunction.Model;
using System.Net.Http.Json;

namespace RcTracking.Test;

public class ApiTests
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
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    [Test, Order(1)]
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
        var response = await client.PostAsJsonAsync("/api/Plane?name=GetTest", "");
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
        var id = planes?.First().Id;

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
        var id = planes!.First().Id;
        var updatedPlane = new PlaneModel(id, "UpdatedName");

        var response = await client.PutAsJsonAsync($"/api/Plane", updatedPlane);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var returnModel = await response.Content.ReadFromJsonAsync<PlaneModel>();
        Assert.That(returnModel, Is.Not.Null);
        Assert.That(returnModel.Name, Is.EqualTo("UpdatedName"));
    }

    [Test]
    [Category("PlaneCrud"), Order(140)]
    public async Task PlaneDelete()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Plane");
        var planes = await allResponse.Content.ReadFromJsonAsync<List<PlaneModel>>();
        var id = planes?.First().Id;

        var response = await client.DeleteAsync($"/api/Plane?id={id}");

        Assert.That(response.IsSuccessStatusCode, Is.True);
    }
}
