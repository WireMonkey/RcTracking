using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace RcTracking.ApiFunction.Model
{
    public class FlightModel(Guid Id, DateOnly flightDate, Guid planeId, int flightCount = 0, string? notes = null)
    {
        [Required]
        public Guid Id { get; init; } = Id;
        [Required]
        public DateOnly FlightDate { get; init; } = flightDate;
        [Required]
        public Guid PlaneId { get; set; } = planeId;
        public int FlightCount { get; set; } = flightCount;
        public string? Notes { get; set; } = notes;

        public static FlightModel CreateDbRec(DateOnly flightDate, Guid planeId, int flightCount = 0, string? notes = null) => new(Guid.NewGuid(), flightDate, planeId, flightCount, notes);

        public void UpdateFrom(FlightModel flight)
        {
            PlaneId = flight.PlaneId;
            FlightCount = flight.FlightCount;
            Notes = flight.Notes;
        }
    }
}
