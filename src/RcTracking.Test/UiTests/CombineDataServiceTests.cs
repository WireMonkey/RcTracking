using RcTracking.UI.Services;
using RcTracking.UI.Events;
using RcTracking.Shared.Model;
using RcTracking.TestDoubles.Mocks;

namespace RcTracking.Test.uiTests;

public class CombineDataServiceTests
{
    private static FlightModel NewFlight(Guid planeId, DateOnly date, int count = 1, string notes = "") => new(Guid.NewGuid(), date, planeId, count, notes);
    private MockPlaneService planeService;
    private MockFlightService flightService;
    private EventBus eventBus;


    [SetUp]
    public void Setup()
    {
        planeService = new MockPlaneService();
        flightService = new MockFlightService();
        eventBus = new EventBus();
    }

    [Test]
    [Category("UiTests")]
    public void CalculatePlaneStats_All()
    {
        // Arrange
        var svc = new CombineDataService(planeService, flightService, eventBus);

        var plane1 = new PlaneModel(Guid.NewGuid(), "Plane1");
        var plane2 = new PlaneModel(Guid.NewGuid(), "Plane2");
        planeService.Planes[plane1.Id] = plane1;
        planeService.Planes[plane2.Id] = plane2;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        flightService.Flights[Guid.NewGuid()] = NewFlight(plane1.Id, today, 2);
        flightService.Flights[Guid.NewGuid()] = NewFlight(plane1.Id, today.AddDays(-1), 3);
        flightService.Flights[Guid.NewGuid()] = NewFlight(plane2.Id, today, 5);

        // Act
        svc.CalculatePlaneStats();

        // Assert
        Assert.That(svc.PlaneStats.Count, Is.EqualTo(2));

        var p1Stats = svc.PlaneStats[plane1.Id];
        Assert.That(p1Stats.TotalFlights, Is.EqualTo(5)); // 2 + 3
        Assert.That(p1Stats.DaysFlown, Is.EqualTo(2));
        Assert.That(p1Stats.AvgFlightsPerDay, Is.EqualTo(5 / 2));
        Assert.That(p1Stats.LastFlight, Is.EqualTo(today));

        var p2Stats = svc.PlaneStats[plane2.Id];
        Assert.That(p2Stats.TotalFlights, Is.EqualTo(5));
        Assert.That(p2Stats.DaysFlown, Is.EqualTo(1));
        Assert.That(p2Stats.AvgFlightsPerDay, Is.EqualTo(5));
    }

    [Test]
    [Category("UiTests")]
    public void CalculatePlaneStats_Single()
    {
        var svc = new CombineDataService(planeService, flightService, eventBus);

        var plane1 = new PlaneModel(Guid.NewGuid(), "Plane1");
        var plane2 = new PlaneModel(Guid.NewGuid(), "Plane2");
        planeService.Planes[plane1.Id] = plane1;
        planeService.Planes[plane2.Id] = plane2;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);

        flightService.Flights[Guid.NewGuid()] = NewFlight(plane1.Id, today, 2);
        flightService.Flights[Guid.NewGuid()] = NewFlight(plane1.Id, yesterday, 1);
        flightService.Flights[Guid.NewGuid()] = NewFlight(plane1.Id, twoDaysAgo, 4);
        flightService.Flights[Guid.NewGuid()] = NewFlight(plane2.Id, today, 7);

        // Act
        svc.CalculatePlaneStats(plane1.Id);

        // Assert
        Assert.That(svc.PlaneStats.Count, Is.EqualTo(1));
        var p1Stats = svc.PlaneStats[plane1.Id];
        Assert.That(p1Stats.TotalFlights, Is.EqualTo(7)); // 2 + 1 + 4
        Assert.That(p1Stats.DaysFlown, Is.EqualTo(3));
        Assert.That(p1Stats.AvgFlightsPerDay, Is.EqualTo(7 / 3));
        Assert.That(p1Stats.LastFlight, Is.EqualTo(today));
        Assert.That(svc.PlaneStats.ContainsKey(plane2.Id), Is.False);
    }
}
