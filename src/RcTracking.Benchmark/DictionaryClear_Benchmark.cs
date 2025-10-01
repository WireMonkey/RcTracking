using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using RcTracking.Shared.Model;
using Microsoft.VSDiagnostics;

namespace BenchmarkSuite1
{
    [CPUUsageDiagnoser]
    public class DictionaryClear_Benchmark
    {
        private List<FlightModel> _flights;
        [GlobalSetup]
        public void Setup()
        {
            _flights = new List<FlightModel>();
            for (int i = 0; i < 1000; i++)
            {
                _flights.Add(new FlightModel(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now), Guid.NewGuid(), 1, $"Flight {i}"));
            }
        }

        [Benchmark]
        public Dictionary<Guid, FlightModel> ClearMethod()
        {
            var dict = new Dictionary<Guid, FlightModel>(_flights.Count);
            foreach (var flight in _flights)
            {
                dict[flight.Id] = flight;
            }

            dict.Clear();
            foreach (var flight in _flights)
            {
                dict[flight.Id] = flight;
            }

            return dict;
        }

        [Benchmark]
        public Dictionary<Guid, FlightModel> NewDictionaryDefaultCapacity()
        {
            var dict = new Dictionary<Guid, FlightModel>(_flights.Count);
            foreach (var flight in _flights)
            {
                dict[flight.Id] = flight;
            }

            dict = new Dictionary<Guid, FlightModel>();
            foreach (var flight in _flights)
            {
                dict[flight.Id] = flight;
            }

            return dict;
        }

        [Benchmark]
        public Dictionary<Guid, FlightModel> NewDictionaryPreserveCapacity()
        {
            var dict = new Dictionary<Guid, FlightModel>(_flights.Count);
            foreach (var flight in _flights)
            {
                dict[flight.Id] = flight;
            }

            dict = new Dictionary<Guid, FlightModel>(_flights.Count);
            foreach (var flight in _flights)
            {
                dict[flight.Id] = flight;
            }

            return dict;
        }
    }
}