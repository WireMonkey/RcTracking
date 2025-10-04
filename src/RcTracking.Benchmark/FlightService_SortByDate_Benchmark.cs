using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using RcTracking.Shared.Model;
using RcTracking.TestDoubles.Mocks;
using ZLinq;

namespace BenchmarkSuite1
{
    [MemoryDiagnoser]
    public class FlightServiceSortByDateBenchmark
    {
        private MockFlightService _flightService = null!;
        
        [GlobalSetup]
        public void Setup()
        {
            _flightService = new MockFlightService();
            
            // Create test data - 1000 flights with random dates over the past 2 years
            var random = new Random(42); // Fixed seed for consistency
            var baseDate = DateTime.Now.AddYears(-2);
            
            for (int i = 0; i < 1000; i++)
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
        public List<FlightModel> LinqOrderBy()
        {
            // Standard LINQ OrderBy approach
            return _flightService.Flights.Values.OrderBy(f => f.FlightDate).ToList();
        }

        [Benchmark]
        public FlightModel[] LinqOrderByToArrayDirect()
        {
            // Direct ToArray without converting back to List
            return _flightService.Flights.Values.OrderBy(f => f.FlightDate).ToArray();
        }

        [Benchmark]
        public List<FlightModel> ManualListSort()
        {
            // Manual approach: create list, then sort
            var flightsList = new List<FlightModel>(_flightService.Flights.Values);
            flightsList.Sort((x, y) => x.FlightDate.CompareTo(y.FlightDate));
            return flightsList;
        }

        [Benchmark]
        public List<FlightModel> ManualListSortWithCapacity()
        {
            // Manual approach with pre-allocated capacity
            var flightsList = new List<FlightModel>(_flightService.Flights.Count);
            flightsList.AddRange(_flightService.Flights.Values);
            flightsList.Sort((x, y) => x.FlightDate.CompareTo(y.FlightDate));
            return flightsList;
        }

        [Benchmark]
        public FlightModel[] ManualArraySortDirect()
        {
            // Using Array.Sort, returning array directly
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
        public List<FlightModel> PriorityQueue()
        {
            // Using PriorityQueue for sorting (available in .NET 6+)
            var priorityQueue = new PriorityQueue<FlightModel, DateOnly>();
            
            foreach (var flight in _flightService.Flights.Values)
            {
                priorityQueue.Enqueue(flight, flight.FlightDate);
            }
            
            var result = new List<FlightModel>(_flightService.Flights.Count);
            while (priorityQueue.Count > 0)
            {
                result.Add(priorityQueue.Dequeue());
            }
            return result;
        }

        [Benchmark]
        public List<FlightModel> OptimizedLinqWithBuffer()
        {
            // Pre-allocate the result list to avoid internal resizing
            var values = _flightService.Flights.Values;
            var result = new List<FlightModel>(values.Count);
            result.AddRange(values.OrderBy(f => f.FlightDate));
            return result;
        }

        [Benchmark]
        public List<FlightModel> UsingZlinqToList()
        {
            var values = _flightService.Flights.Values;
            var result = values.AsValueEnumerable()
                .OrderBy(f => f.FlightDate)
                .ToList();
            return result;
        }

        [Benchmark]
        public Dictionary<Guid, FlightModel> UsingZlinqToDictionary()
        {
            var values = _flightService.Flights.Values;
            var result = values.AsValueEnumerable()
                .OrderBy(f => f.FlightDate)
                .ToDictionary(f => f.Id, f => f);
            return result;
        }
    }
}