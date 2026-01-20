using Microsoft.Extensions.Configuration;
using RcTracking.Shared.Model;
using RcTracking.UI.Services;

namespace RcTracking.Test.uiTests;

public class FlightServiceTests
{
    private FlightService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"apiUrl", "http://localhost/"}
            })
            .Build();
        // Provide default/null values for all required FlightService constructor parameters
        // (apiUrl, apiKey, eventBus, accessTokenProvider, snackbarService)
        return new FlightService(
            config["apiUrl"] ?? string.Empty,
            string.Empty, // apiKey
            new EventBus(),
            null!,        // IAccessTokenProvider (use null-forgiving operator for test)
            null!         // ISnackbar (use null-forgiving operator for test)
        );
    }

    private static FlightModel NewFlight(int flightCount = 1) => new(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid(), flightCount, "test");

    private static FlightModel NewFlightWithDate(DateOnly date, int flightCount = 1) => new(Guid.NewGuid(), date, Guid.NewGuid(), flightCount, "test");

    [Test]
    [Category("UiTests")]
    public void TotalFlightsNoFlightsReturnsZero()
    {
        var svc = CreateService();
        var total = svc.TotalFlights();
        Assert.That(total, Is.EqualTo(0));
    }

    [Test]
    [Category("UiTests")]
    public void TotalFlightsWithFlightsSumsFlightCount()
    {
        var svc = CreateService();
        var f1 = NewFlight(2);
        var f2 = NewFlight(3);
        svc.Flights[f1.Id] = f1;
        svc.Flights[f2.Id] = f2;
        var total = svc.TotalFlights();
        Assert.That(total, Is.EqualTo(5));
    }

    [Test]
    [Category("UiTests")]
    public void TotalFlightsAfterUpdateReflectsChange()
    {
        var svc = CreateService();
        var f1 = NewFlight(4);
        svc.Flights[f1.Id] = f1;
        Assert.That(svc.TotalFlights(), Is.EqualTo(4));
        svc.Flights[f1.Id].FlightCount = 7; // simulate update
        var total = svc.TotalFlights();
        Assert.That(total, Is.EqualTo(7));
    }

    // New test for TotalFlights(int year)
    [Test]
    [Category("UiTests")]
    public void TotalFlightsWithYearOnlyCountsSpecifiedYear()
    {
        var svc = CreateService();
        // 2024 flights
        var f2024_1 = NewFlightWithDate(new DateOnly(2024, 1, 10), 2);
        var f2024_2 = NewFlightWithDate(new DateOnly(2024, 6, 5), 3);
        // Other years
        var f2023 = NewFlightWithDate(new DateOnly(2023, 12, 31), 5);
        var f2025 = NewFlightWithDate(new DateOnly(2025, 1, 1), 7);

        svc.Flights[f2024_1.Id] = f2024_1;
        svc.Flights[f2024_2.Id] = f2024_2;
        svc.Flights[f2023.Id] = f2023;
        svc.Flights[f2025.Id] = f2025;

        var total2024 = svc.TotalFlights(2024);
        Assert.That(total2024, Is.EqualTo(5)); // only 2 + 3 from 2024 flights
    }

    // DaysFlying Tests

    [Test]
    [Category("UiTests")]
    public void DaysFlyingNoFlightsReturnsZero()
    {
        var svc = CreateService();
        var days = svc.DaysFlying();
        Assert.That(days, Is.EqualTo(0));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingSingleFlightReturnsOne()
    {
        var svc = CreateService();
        var flight = NewFlightWithDate(new DateOnly(2024, 1, 15));
        svc.Flights[flight.Id] = flight;
        
        var days = svc.DaysFlying();
        Assert.That(days, Is.EqualTo(1));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingMultipleFlightsSameDayReturnsOne()
    {
        var svc = CreateService();
        var date = new DateOnly(2024, 1, 15);
        var flight1 = NewFlightWithDate(date, 2);
        var flight2 = NewFlightWithDate(date, 3);
        var flight3 = NewFlightWithDate(date, 1);
        
        svc.Flights[flight1.Id] = flight1;
        svc.Flights[flight2.Id] = flight2;
        svc.Flights[flight3.Id] = flight3;
        
        var days = svc.DaysFlying();
        Assert.That(days, Is.EqualTo(1));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingMultipleFlightsDifferentDaysReturnsCorrectCount()
    {
        var svc = CreateService();
        var flight1 = NewFlightWithDate(new DateOnly(2024, 1, 15));
        var flight2 = NewFlightWithDate(new DateOnly(2024, 1, 16));
        var flight3 = NewFlightWithDate(new DateOnly(2024, 1, 17));
        var flight4 = NewFlightWithDate(new DateOnly(2024, 1, 15)); // Same as flight1
        
        svc.Flights[flight1.Id] = flight1;
        svc.Flights[flight2.Id] = flight2;
        svc.Flights[flight3.Id] = flight3;
        svc.Flights[flight4.Id] = flight4;
        
        var days = svc.DaysFlying();
        Assert.That(days, Is.EqualTo(3));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingFlightsAcrossMultipleYearsCountsAllDays()
    {
        var svc = CreateService();
        var flight1 = NewFlightWithDate(new DateOnly(2023, 12, 31));
        var flight2 = NewFlightWithDate(new DateOnly(2024, 1, 1));
        var flight3 = NewFlightWithDate(new DateOnly(2024, 6, 15));
        var flight4 = NewFlightWithDate(new DateOnly(2025, 3, 10));
        
        svc.Flights[flight1.Id] = flight1;
        svc.Flights[flight2.Id] = flight2;
        svc.Flights[flight3.Id] = flight3;
        svc.Flights[flight4.Id] = flight4;
        
        var days = svc.DaysFlying();
        Assert.That(days, Is.EqualTo(4));
    }

    // DaysFlying(year) Tests

    [Test]
    [Category("UiTests")]
    public void DaysFlyingWithYearNoFlightsReturnsZero()
    {
        var svc = CreateService();
        var days = svc.DaysFlying(2024);
        Assert.That(days, Is.EqualTo(0));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingWithYearNoFlightsInSpecifiedYearReturnsZero()
    {
        var svc = CreateService();
        var flight1 = NewFlightWithDate(new DateOnly(2023, 1, 15));
        var flight2 = NewFlightWithDate(new DateOnly(2025, 1, 15));
        
        svc.Flights[flight1.Id] = flight1;
        svc.Flights[flight2.Id] = flight2;
        
        var days = svc.DaysFlying(2024);
        Assert.That(days, Is.EqualTo(0));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingWithYearSingleFlightInYearReturnsOne()
    {
        var svc = CreateService();
        var flight = NewFlightWithDate(new DateOnly(2024, 6, 15));
        svc.Flights[flight.Id] = flight;
        
        var days = svc.DaysFlying(2024);
        Assert.That(days, Is.EqualTo(1));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingWithYearMultipleFlightsSameDayInYearReturnsOne()
    {
        var svc = CreateService();
        var date = new DateOnly(2024, 6, 15);
        var flight1 = NewFlightWithDate(date, 2);
        var flight2 = NewFlightWithDate(date, 3);
        var flight3 = NewFlightWithDate(new DateOnly(2023, 6, 15)); // Different year
        
        svc.Flights[flight1.Id] = flight1;
        svc.Flights[flight2.Id] = flight2;
        svc.Flights[flight3.Id] = flight3;
        
        var days = svc.DaysFlying(2024);
        Assert.That(days, Is.EqualTo(1));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingWithYearMultipleFlightsDifferentDaysInYearReturnsCorrectCount()
    {
        var svc = CreateService();
        var flight1 = NewFlightWithDate(new DateOnly(2024, 1, 15));
        var flight2 = NewFlightWithDate(new DateOnly(2024, 3, 20));
        var flight3 = NewFlightWithDate(new DateOnly(2024, 6, 10));
        var flight4 = NewFlightWithDate(new DateOnly(2024, 1, 15)); // Same as flight1
        var flight5 = NewFlightWithDate(new DateOnly(2023, 1, 15)); // Different year
        var flight6 = NewFlightWithDate(new DateOnly(2025, 3, 20)); // Different year
        
        svc.Flights[flight1.Id] = flight1;
        svc.Flights[flight2.Id] = flight2;
        svc.Flights[flight3.Id] = flight3;
        svc.Flights[flight4.Id] = flight4;
        svc.Flights[flight5.Id] = flight5;
        svc.Flights[flight6.Id] = flight6;
        
        var days = svc.DaysFlying(2024);
        Assert.That(days, Is.EqualTo(3));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingWithYearLeapYearHandlesFebruary29()
    {
        var svc = CreateService();
        var flight1 = NewFlightWithDate(new DateOnly(2024, 2, 28)); // 2024 is a leap year
        var flight2 = NewFlightWithDate(new DateOnly(2024, 2, 29)); // Leap day
        var flight3 = NewFlightWithDate(new DateOnly(2024, 3, 1));
        
        svc.Flights[flight1.Id] = flight1;
        svc.Flights[flight2.Id] = flight2;
        svc.Flights[flight3.Id] = flight3;
        
        var days = svc.DaysFlying(2024);
        Assert.That(days, Is.EqualTo(3));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingWithYearFirstAndLastDayOfYearCountsBoth()
    {
        var svc = CreateService();
        var flight1 = NewFlightWithDate(new DateOnly(2024, 1, 1)); // First day
        var flight2 = NewFlightWithDate(new DateOnly(2024, 12, 31)); // Last day
        var flight3 = NewFlightWithDate(new DateOnly(2023, 12, 31)); // Previous year
        var flight4 = NewFlightWithDate(new DateOnly(2025, 1, 1)); // Next year
        
        svc.Flights[flight1.Id] = flight1;
        svc.Flights[flight2.Id] = flight2;
        svc.Flights[flight3.Id] = flight3;
        svc.Flights[flight4.Id] = flight4;
        
        var days = svc.DaysFlying(2024);
        Assert.That(days, Is.EqualTo(2));
    }

    [Test]
    [Category("UiTests")]
    public void DaysFlyingCompareYearSpecificVsTotalCorrectRelationship()
    {
        var svc = CreateService();
        var flight1 = NewFlightWithDate(new DateOnly(2023, 6, 15));
        var flight2 = NewFlightWithDate(new DateOnly(2024, 3, 20));
        var flight3 = NewFlightWithDate(new DateOnly(2024, 8, 10));
        var flight4 = NewFlightWithDate(new DateOnly(2025, 1, 5));
        
        svc.Flights[flight1.Id] = flight1;
        svc.Flights[flight2.Id] = flight2;
        svc.Flights[flight3.Id] = flight3;
        svc.Flights[flight4.Id] = flight4;
        
        var totalDays = svc.DaysFlying();
        var days2023 = svc.DaysFlying(2023);
        var days2024 = svc.DaysFlying(2024);
        var days2025 = svc.DaysFlying(2025);
        
        Assert.That(totalDays, Is.EqualTo(4));
        Assert.That(days2023, Is.EqualTo(1));
        Assert.That(days2024, Is.EqualTo(2));
        Assert.That(days2025, Is.EqualTo(1));
        Assert.That(days2023 + days2024 + days2025, Is.EqualTo(totalDays));
    }
}