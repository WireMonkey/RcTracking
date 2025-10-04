using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RcTracking.Shared.Model;
using RcTracking.UI.Interface;
using RcTracking.TestDoubles.Mocks;

namespace BenchmarkSuite1
{
    [MemoryDiagnoser]
    public class CombineDataServiceGroupingBenchmark
    {
        private MockFlightService _flightService;
        private MockPlaneService _planeService;
        
        [GlobalSetup]
        public void Setup()
        {
            _flightService = new MockFlightService();
            _planeService = new MockPlaneService();
            
            // Create test data - 1000 flights across 50 planes
            var random = new Random(42); // Fixed seed for consistency
            var planeIds = new List<Guid>();
            
            // Create planes
            for (int i = 0; i < 50; i++)
            {
                var planeId = Guid.NewGuid();
                planeIds.Add(planeId);
                _planeService.Planes[planeId] = new PlaneModel(planeId, $"Plane {i}");
            }
            
            // Create flights
            for (int i = 0; i < 1000; i++)
            {
                var flightId = Guid.NewGuid();
                var planeId = planeIds[random.Next(planeIds.Count)];
                var flightDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-random.Next(365)));
                var flight = new FlightModel(flightId, flightDate, planeId, random.Next(1, 10), $"Flight {i}");
                _flightService.Flights[flightId] = flight;
            }
        }
        
        [Benchmark]
        public Dictionary<Guid, List<FlightModel>> CurrentGroupingApproach()
        {
            return _flightService.Flights.Values
                .GroupBy(f => f.PlaneId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        [Benchmark]
        public Dictionary<Guid, List<FlightModel>> OptimizedGroupingApproach()
        {
            var result = new Dictionary<Guid, List<FlightModel>>();
            
            foreach (var flight in _flightService.Flights.Values)
            {
                if (!result.TryGetValue(flight.PlaneId, out var flightList))
                {
                    flightList = new List<FlightModel>();
                    result[flight.PlaneId] = flightList;
                }
                flightList.Add(flight);
            }
            
            return result;
        }
    }
}