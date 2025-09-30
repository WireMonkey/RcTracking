using RcTracking.Shared.Model;
using RcTracking.UI.Events;
using RcTracking.UI.Interface;
using RcTracking.UI.Models;

namespace RcTracking.UI.Services
{
    public class CombineDataService : ICombineDataService
    {
        private readonly IPlaneService _planeService;
        private readonly IFlightService _flightService;
        private readonly EventBus _eventBus;

        public CombineDataService(IPlaneService planeService, IFlightService flightService, EventBus eventBus)
        {
            _planeService = planeService ?? throw new ArgumentNullException(nameof(planeService));
            _flightService = flightService ?? throw new ArgumentNullException(nameof(flightService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.OnMessage += OnMessage;

            if (_planeService.HasLoaded && _flightService.HasLoaded)
            {
                CalculatePlaneStats();
            }
        }
        
        private Dictionary<Guid, UiPlane> _planeStats = new();

        public Dictionary<Guid, UiPlane> PlaneStats => _planeStats;

        private void OnMessage(object? sender, EventMessage? e)
        {
            if (e is null) return;

            switch (e.Event)
            {
                case EventEnum.FlightAdded:
                case EventEnum.PlaneAdded:
                    var guid = (e as PlaneFlightAddedMessage)?.PlaneId;
                    CalculatePlaneStats(guid ?? Guid.Empty);
                    break;

                case EventEnum.RefreshFlight:
                case EventEnum.RefreshPlane:
                    CalculatePlaneStats();
                    break;

                case EventEnum.Clear:
                    _planeStats.Clear();
                    break;

                default:
                    break;
            }
        }

        public void CalculatePlaneStats()
        {
            _planeStats.Clear();
            var flightsByPlane = GroupFlightsByPlane();

            foreach (var planeId in _planeService.Planes.Keys)
            {
                if(flightsByPlane.TryGetValue(planeId, out var flights))
                {
                    var totalFlights = flights.Sum(f => f.FlightCount);
                    var lastFlightDate = flights.Max(f => f.FlightDate);
                    var daysFlown = flights.Count;
                    var avgFlightsPerDay = daysFlown > 0 ? totalFlights / daysFlown : 0;
                    var planeStats = new UiPlane
                    {
                        Id = planeId,
                        TotalFlights = totalFlights,
                        LastFlight = lastFlightDate,
                        DaysFlown = daysFlown,
                        AvgFlightsPerDay = avgFlightsPerDay
                    };

                    _planeStats[planeId] = planeStats;

                }
                else
                {
                    _planeStats[planeId] = new UiPlane { Id = planeId };
                }

            }
        }

        public void CalculatePlaneStats(Guid planeId)
        {
            if (_planeService.Planes.ContainsKey(planeId))
            {
                var flightsByPlane = GroupFlightsByPlane(planeId);
                if (flightsByPlane.TryGetValue(planeId, out var flights))
                {
                    var totalFlights = flights.Sum(f => f.FlightCount);
                    var lastFlightDate = flights.Max(f => f.FlightDate);
                    var daysFlown = flights.Count;
                    var avgFlightsPerDay = daysFlown > 0 ? totalFlights / daysFlown : 0;
                    var planeStats = new UiPlane
                    {
                        Id = planeId,
                        TotalFlights = totalFlights,
                        LastFlight = lastFlightDate,
                        DaysFlown = daysFlown,
                        AvgFlightsPerDay = avgFlightsPerDay
                    };
                    _planeStats[planeId] = planeStats;
                }
                else
                {
                    _planeStats[planeId] = new UiPlane { Id = planeId };
                }
            }
        }

        private Dictionary<Guid, List<FlightModel>> GroupFlightsByPlane(Guid? id = null)
        {
            var result = new Dictionary<Guid, List<FlightModel>>();

            foreach (var flight in _flightService.Flights.Values)
            {
                if (id.HasValue && flight.PlaneId != id.Value)
                {
                    continue;
                }
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
