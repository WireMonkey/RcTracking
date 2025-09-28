﻿using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using RcTracking.Shared.Model;
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
        var plane = new PlaneModel(Guid.Empty, "GetTest");
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

    [Test]
    [Category("FlightCrud"), Order(210)]
    public async Task FlightCreate() 
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var nPlane = new PlaneModel(Guid.Empty, "FlightCreateTest");
        var planeResponse = await client.PostAsJsonAsync("/api/Plane", nPlane);
        var plane = await planeResponse.Content.ReadFromJsonAsync<PlaneModel>();

        var flight = new FlightModel(Guid.Empty, DateOnly.FromDateTime(DateTime.UtcNow), plane!.Id, 1, "Test flight creation");
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
        var id = flights?.First().Id;

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
        var flight = flights!.First();
        flight.Notes = "Updated notes";

        var response = await client.PutAsJsonAsync($"/api/Flight", flight);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        
        var returnModel = await response.Content.ReadFromJsonAsync<FlightModel>();
        Assert.That(returnModel, Is.Not.Null);
        Assert.That(returnModel.Notes, Is.EqualTo("Updated notes"));
    }

    [Test]
    [Category("FlightCrud"), Order(240)]
    public async Task FlightDelete()
    {
        var client = _app.CreateHttpClient("rc-tracking-function");
        var allResponse = await client.GetAsync("/api/Flight");
        var flights = await allResponse.Content.ReadFromJsonAsync<List<FlightModel>>();
        var id = flights?.First().Id;

        var response = await client.DeleteAsync($"/api/Flight?id={id}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }
}
