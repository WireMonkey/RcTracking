using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using RcTracking.Shared.Model;
using RcTracking.TestDoubles.Mocks;

namespace BenchmarkSuite1
{
    [MemoryDiagnoser]
    public class FlightServiceComplexSortByDateBenchmark
    {
        private MockFlightService _flightService = null!;
        
        [Params(100, 1000)]
        public int FlightCount { get; set; }
        
        [GlobalSetup]
        public void Setup()
        {
            _flightService = new MockFlightService();
            
            // Create test data with varying flight counts
            var random = new Random(42); // Fixed seed for consistency
            var baseDate = DateTime.Now.AddYears(-2);
            
            for (int i = 0; i < FlightCount; i++)
            {
                var flightId = Guid.NewGuid();
                var planeId = Guid.NewGuid();
                var flightDate = DateOnly.FromDateTime(baseDate.AddDays(random.Next(730))); // Random date within 2 years
                var flightCount = random.Next(1, 10);
                var flight = new FlightModel(flightId, flightDate, planeId, flightCount, $"Flight {i}");
                
                _flightService.Flights[flightId] = flight;
            }
        }
        
        [Benchmark(Baseline = true)]
        public List<FlightModel> StandardLinqOrderBy()
        {
            return _flightService.Flights.Values.OrderBy(f => f.FlightDate).ToList();
        }

        [Benchmark]
        public List<FlightModel> OptimizedListSort()
        {
            var flightsList = new List<FlightModel>(_flightService.Flights.Count);
            flightsList.AddRange(_flightService.Flights.Values);
            flightsList.Sort((x, y) => x.FlightDate.CompareTo(y.FlightDate));
            return flightsList;
        }

        [Benchmark]
        public FlightModel[] ArraySortDirect()
        {
            var flightsArray = new FlightModel[_flightService.Flights.Count];
            var index = 0;
            foreach (var flight in _flightService.Flights.Values)
            {
                flightsArray[index++] = flight;
            }
            
            Array.Sort(flightsArray, (x, y) => x.FlightDate.CompareTo(y.FlightDate));
            return flightsArray;
        }

        [Benchmark]
        public List<FlightModel> SortByDateThenByFlightCount()
        {
            // More complex sorting: by date first, then by flight count
            return _flightService.Flights.Values
                .OrderBy(f => f.FlightDate)
                .ThenBy(f => f.FlightCount)
                .ToList();
        }

        [Benchmark]
        public List<FlightModel> SortByDateThenByFlightCountManual()
        {
            // Manual implementation of complex sorting
            var flightsList = new List<FlightModel>(_flightService.Flights.Count);
            flightsList.AddRange(_flightService.Flights.Values);
            flightsList.Sort((x, y) => 
            {
                var dateComparison = x.FlightDate.CompareTo(y.FlightDate);
                return dateComparison != 0 ? dateComparison : x.FlightCount.CompareTo(y.FlightCount);
            });
            return flightsList;
        }

        [Benchmark]
        public List<FlightModel> SortTop10Recent()
        {
            // Common scenario: get top 10 most recent flights
            return _flightService.Flights.Values
                .OrderByDescending(f => f.FlightDate)
                .Take(10)
                .ToList();
        }

        [Benchmark]
        public List<FlightModel> SortTop10RecentManual()
        {
            // Manual approach for top 10 - potentially more efficient for small results
            var allFlights = new List<FlightModel>(_flightService.Flights.Values);
            allFlights.Sort((x, y) => y.FlightDate.CompareTo(x.FlightDate));
            
            var result = new List<FlightModel>(Math.Min(10, allFlights.Count));
            for (int i = 0; i < Math.Min(10, allFlights.Count); i++)
            {
                result.Add(allFlights[i]);
            }
            return result;
        }
    }

    // Custom comparer for demonstration
    public class FlightDateComparer : IComparer<FlightModel>
    {
        public int Compare(FlightModel x, FlightModel y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.FlightDate.CompareTo(y.FlightDate);
        }
    }
}