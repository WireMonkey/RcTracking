using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using RcTracking.Shared.Model;
using RcTracking.TestDoubles.Mocks;

namespace BenchmarkSuite1
{
    [MemoryDiagnoser]
    public class FlightServiceTotalFlightsBenchmark
    {
        private MockFlightService _flightService;
        
        [GlobalSetup]
        public void Setup()
        {
            _flightService = new MockFlightService();
            
            // Create test data - 1000 flights with varying flight counts
            var random = new Random(42); // Fixed seed for consistency
            for (int i = 0; i < 1000; i++)
            {
                var flightId = Guid.NewGuid();
                var planeId = Guid.NewGuid();
                var flightDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-random.Next(365)));
                var flightCount = random.Next(1, 10); // Random flight count between 1-9
                var flight = new FlightModel(flightId, flightDate, planeId, flightCount, $"Flight {i}");
                _flightService.Flights[flightId] = flight;
            }
        }
        
        [Benchmark]
        public int CurrentLinqApproach()
        {
            // Current implementation from FlightService.TotalFlights()
            return _flightService.Flights.Count > 0 ? _flightService.Flights.Select(f => f.Value.FlightCount).Sum() : 0;
        }

        [Benchmark]
        public int OptimizedLinqValues()
        {
            // Optimized LINQ using Values directly
            return _flightService.Flights.Count > 0 ? _flightService.Flights.Values.Sum(f => f.FlightCount) : 0;
        }

        [Benchmark]
        public int ManualLoop()
        {
            // Manual loop approach
            if (_flightService.Flights.Count == 0) return 0;
            
            var total = 0;
            foreach (var flight in _flightService.Flights.Values)
            {
                total += flight.FlightCount;
            }
            return total;
        }

        [Benchmark]
        public int ManualLoopWithoutCountCheck()
        {
            // Manual loop without count check (returns 0 for empty collection naturally)
            var total = 0;
            foreach (var flight in _flightService.Flights.Values)
            {
                total += flight.FlightCount;
            }
            return total;
        }

        [Benchmark]
        public int LinqAggregateMethod()
        {
            // Using LINQ Aggregate method
            return _flightService.Flights.Count > 0 
                ? _flightService.Flights.Values.Aggregate(0, (sum, flight) => sum + flight.FlightCount) 
                : 0;
        }

        [Benchmark]
        public int LinqValuesWithoutCountCheck()
        {
            // Optimized LINQ without count check (Sum returns 0 for empty collections)
            return _flightService.Flights.Values.Sum(f => f.FlightCount);
        }
    }
}