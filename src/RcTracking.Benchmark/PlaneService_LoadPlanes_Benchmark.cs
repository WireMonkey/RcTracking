using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using RcTracking.Shared.Model;
using RcTracking.TestDoubles.Mocks;

namespace BenchmarkSuite1
{
    [MemoryDiagnoser]
    public class PlaneService_LoadPlanes_Benchmark
    {
        private MockPlaneService _planeService;
        private List<PlaneModel> _apiReturn;
        
        [GlobalSetup]
        public void Setup()
        {
            _planeService = new MockPlaneService();
            
            // Create test data - 1000 planes to simulate a realistic scenario
            _apiReturn = new List<PlaneModel>();
            for (int i = 0; i < 1000; i++)
            {
                _apiReturn.Add(new PlaneModel(Guid.NewGuid(), $"Plane {i}"));
            }
        }
        
        [Benchmark]
        public Dictionary<Guid, PlaneModel> CurrentToDictionaryApproach()
        {
            // Simulate the current approach in PlaneService.LoadPlanesAsync
            var planes = new Dictionary<Guid, PlaneModel>();
            planes.Clear();
            planes = _apiReturn.ToDictionary(f => f.Id, f => f);
            return planes;
        }

        [Benchmark]
        public Dictionary<Guid, PlaneModel> OptimizedManualLoop()
        {
            // Optimized approach using manual loop with capacity
            var planes = new Dictionary<Guid, PlaneModel>(_apiReturn.Count);
            foreach (var plane in _apiReturn)
            {
                planes[plane.Id] = plane;
            }
            return planes;
        }

        [Benchmark]
        public Dictionary<Guid, PlaneModel> OptimizedEnsureCapacity()
        {
            // Optimized approach using EnsureCapacity
            var planes = new Dictionary<Guid, PlaneModel>();
            planes.EnsureCapacity(_apiReturn.Count);
            foreach (var plane in _apiReturn)
            {
                planes[plane.Id] = plane;
            }
            return planes;
        }
    }
}